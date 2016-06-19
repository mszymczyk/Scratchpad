//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

using Sce.Atf;
using Sce.Atf.Applications;

using AtfKeys = Sce.Atf.Input.Keys;
using AtfKeyEventArgs = Sce.Atf.Input.KeyEventArgs;
using AtfMouseEventArgs = Sce.Atf.Input.MouseEventArgs;
using AtfMessage = Sce.Atf.Input.Message;
using AtfDragEventArgs = Sce.Atf.Input.DragEventArgs;

using WfKeys = System.Windows.Forms.Keys;
using WfKeyEventArgs = System.Windows.Forms.KeyEventArgs;
using WfMouseEventArgs = System.Windows.Forms.MouseEventArgs;
using WfMessage = System.Windows.Forms.Message;
using WfDragEventArgs = System.Windows.Forms.DragEventArgs;

using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using DeviceContext = SharpDX.Direct3D11.DeviceContext;
using System.Collections.Generic;

namespace TextureEditor
{
    /// <summary>
    /// A control for using OpenGL to do the painting</summary>
    /// <remarks>This class's constructor initializes OpenGL so that other tools that use OpenGL, such as
    /// the Texture Manager, work even if BeginPaint has not been called. This allows
    /// Panel3D to work in a tabbed interface like the FAST Editor.</remarks>
	public class TexturePreviewWindowSharpDX : InteropControl
    {
        /// <summary>
        /// Constructor</summary>
        /// <remarks>This constructor initializes OpenGL so that other tools that use OpenGL, such as
        /// the Texture Manager, work even if BeginPaint has not been called. This allows
        /// Panel3D to work in a tabbed interface like the FAST Editor.</remarks>
		public TexturePreviewWindowSharpDX( IContextRegistry contextRegistry )
        {
			//m_textureSelectionContext = new TextureSelectionContext( contextRegistry );
			StartSharpDxIfNecessary();

			ShowRedChannel = true;
			ShowGreenChannel = true;
			ShowBlueChannel = true;
			ShowAlphaChannel = true;
			SourceTextureGamma = 2.2f;
			ExportedTextureGamma = 1.0f;
			VisibleMip = 0;
			VisibleSlice = 0;

			SizeChanged += new EventHandler( this.MyButton1_SizeChanged );
        }

        /// <summary>
        /// Disposes resources</summary>
        /// <param name="disposing">True to release both managed and unmanaged resources;
        /// false to release only unmanaged resources</param>
        protected override void Dispose(bool disposing)
        {
             StopSharpDx(disposing);
             base.Dispose(disposing);
        }

		[StructLayout(LayoutKind.Explicit, Size = 64)]
		struct ConstantBufferData
		{
			[FieldOffset(0)]
			public int xOffset;
			[FieldOffset(4)]
			public int yOffset;
			[FieldOffset(8)]
			public int mipLevel;
			[FieldOffset(12)]
			public int sliceIndex;
			[FieldOffset( 16 )]
			public float gamma;
			[FieldOffset( 20 )]
			public float gammaExp;
			[FieldOffset( 24 )]
			public int flipYExp;
			[FieldOffset( 28 )]
			public int redVisible;
			[FieldOffset( 32 )]
			public int greenVisible;
			[FieldOffset( 36 )]
			public int blueVisible;
			[FieldOffset( 40 )]
			public int alphaVisible;
		};

		void SubmitFullscreenQuad()
		{
			var vertices = Buffer.Create( m_device, BindFlags.VertexBuffer, new[]
                                  {
                                      new Vector4(-1, -1, 0.5f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                                      new Vector4( 1, -1, 0.5f, 1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
                                      new Vector4(-1,  1, 0.5f, 1.0f), new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
                                      new Vector4( 1,  1, 0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f)
                                  } );

			m_context.InputAssembler.SetVertexBuffers( 0, new VertexBufferBinding( vertices, 32, 0 ) );
			vertices.Dispose();

			m_context.Draw( 4, 0 );
		}

		void DrawBackground()
		{
			m_context.PixelShader.Set( GetPsShader( "PS_Clear2" ) );
			var vertices = Buffer.Create( m_device, BindFlags.VertexBuffer, new[]
                                  {
                                      new Vector4(-1,  0, 0.5f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                                      new Vector4( 1,  0, 0.5f, 1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
                                      new Vector4(-1,  1, 0.5f, 1.0f), new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
                                      new Vector4( 1,  1, 0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f)
                                  } );

			m_context.InputAssembler.SetVertexBuffers( 0, new VertexBufferBinding( vertices, 32, 0 ) );
			vertices.Dispose();

			m_context.Draw( 4, 0 );
		}

		void DrawGradient()
		{
			m_context.PixelShader.Set( GetPsShader( "PS_Gradient" ) );

			var vertices = Buffer.Create( m_device, BindFlags.VertexBuffer, new[]
                                  {
									  //new Vector4(-0.95f,  0.15f, 0.5f, 1.0f), new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
									  //new Vector4( 0.95f,  0.15f, 0.5f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
									  //new Vector4(-0.95f,  0.35f, 0.5f, 1.0f), new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
									  //new Vector4( 0.95f,  0.35f, 0.5f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f)
                                      new Vector4(-1,  0.15f, 0.5f, 1.0f), new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
                                      new Vector4( 1,  0.15f, 0.5f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                                      new Vector4(-1,  0.35f, 0.5f, 1.0f), new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
                                      new Vector4( 1,  0.35f, 0.5f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f)
                                  } );

			m_context.InputAssembler.SetVertexBuffers( 0, new VertexBufferBinding( vertices, 32, 0 ) );
			vertices.Dispose();

			m_context.Draw( 4, 0 );
		}

		void SetupConstantBuffer()
		{
			ConstantBufferData data = new ConstantBufferData();
			int visibleMip = Math.Min( VisibleMip, m_texProperties.MipLevels - 1 );
			data.mipLevel = visibleMip;

			int visibleSlice = Math.Min( VisibleSlice, m_texProperties.ArraySize - 1 );
			data.sliceIndex = visibleSlice;

			data.gamma = SourceTextureGamma;
			data.gammaExp = ExportedTextureGamma;
			data.flipYExp = m_metadata.FlipY ? 1 : 0;

			data.redVisible = ShowRedChannel ? 1 : 0;
			data.greenVisible = ShowGreenChannel ? 1 : 0;
			data.blueVisible = ShowBlueChannel ? 1 : 0;
			data.alphaVisible = ShowAlphaChannel ? 1 : 0;

			var constantBuffer = Buffer.Create( m_device, BindFlags.ConstantBuffer, ref data );
			m_context.PixelShader.SetConstantBuffer( 0, constantBuffer );
			constantBuffer.Dispose();
		}

		void DrawTexture()
		{
			int textureFileWidth = m_tex.Width;
			int textureFileHeight = m_tex.Height;
			bool cubeMap = m_tex.IsCubeMap;

			float windowAspect = (float)ClientSize.Width / (float)ClientSize.Height;

			if ( fitSizeRequest )
			{
				fitSizeRequest = false;

				m_texW = 2 * (float)textureFileWidth / (float)ClientSize.Width;
				m_texH = 2 * (float)textureFileHeight / (float)ClientSize.Height;
			}
			else if ( fitInWindowRequest )
			{
				fitInWindowRequest = false;

				m_texW = 2.0f;
				m_texH = 2.0f;
				float texAspect = (float)textureFileWidth / (float)textureFileHeight;
				if ( windowAspect > texAspect )
				{
					//texW = (float)srvDesc.Height / (float)srvDesc.Width;
					m_texW = (float)textureFileWidth / (float)textureFileHeight;
					m_texW *= 2;
					m_texW *= (float)ClientSize.Height / (float)ClientSize.Width;
					//texW *= windowAspect;
				}
				else
				{
					m_texH = (float)textureFileHeight / (float)textureFileWidth;
					m_texH *= 2;
					m_texH *= windowAspect;
				}
			}

			m_context.PixelShader.SetSampler( 0, m_ssPoint );

			if ( DisplayMode == TextureDisplayMode.Source )
				m_context.PixelShader.SetShaderResource( 0, m_tex.m_texSrv );
			else if ( DisplayMode == TextureDisplayMode.Exported )
				m_context.PixelShader.SetShaderResource( 1, m_texExp.m_texSrv );
			else if ( DisplayMode == TextureDisplayMode.Difference )
			{
				m_context.PixelShader.SetShaderResource( 0, m_tex.m_texSrv );
				m_context.PixelShader.SetShaderResource( 1, m_texExp.m_texSrv );
			}

			float texWh = m_texW * 0.5f * m_texScale;
			float texHh = m_texH * 0.5f * m_texScale;
			float xOffset = m_texPosition.X * m_texScale;
			float yOffset = m_texPosition.Y * m_texScale;

			if ( cubeMap )
			{
				// draw background
				//
				{
					// Instantiate Vertex buffer from vertex data
					var verticesBg = Buffer.Create( m_device, BindFlags.VertexBuffer, new[]
										  {
											  new Vector4(-texWh + xOffset, -texHh + yOffset, 0.5f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
											  new Vector4( texWh + xOffset, -texHh + yOffset, 0.5f, 1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
											  new Vector4(-texWh + xOffset,  texHh + yOffset, 0.5f, 1.0f), new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
											  new Vector4( texWh + xOffset,  texHh + yOffset, 0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f)
										  } );

					m_context.InputAssembler.SetVertexBuffers( 0, new VertexBufferBinding( verticesBg, 32, 0 ) );
					verticesBg.Dispose();

					m_context.PixelShader.Set( GetPsShader( "PS_Clear" ) );
					m_context.Draw( 4, 0 );
				}

				// draw cubemap
				//

				PsShaderWrap ps = null;
				//m_psDictionary.TryGetValue( "PS_Tex2DArray_Sample", out ps );
				m_psDictionary.TryGetValue( "PS_TexCube_Sample", out ps );			
				m_context.PixelShader.Set( ps.m_ps );

				SetupConstantBuffer();

				float tw = ( texWh * 2 ) / 4;
				float th = ( texHh * 2 ) / 3;
				float tx = -texWh + xOffset + 0;
				float ty = -texHh + yOffset + 0;

				// cubemap layout
				// https://msdn.microsoft.com/en-us/library/windows/desktop/bb204881(v=vs.85).aspx
				//

				var vertices = Buffer.Create( m_device, BindFlags.VertexBuffer, new[]
									  {
  										  // face 1
										  //
										  new Vector4(tx,			ty + th,	0.5f, 1.0f), new Vector4(0, 1, 1, 1),
										  new Vector4(tx + tw,		ty + th,	0.5f, 1.0f), new Vector4(1, 1, 1, 1),
										  new Vector4(tx,			ty + th*2,	0.5f, 1.0f), new Vector4(0, 0, 1, 1),
										  new Vector4(tx + tw,		ty + th*2,	0.5f, 1.0f), new Vector4(1, 0, 1, 1),

										  // face 4
										  //
										  new Vector4(tx + tw,		ty + th,	0.5f, 1.0f), new Vector4(0, 1, 4, 1),
										  new Vector4(tx + tw*2,	ty + th,	0.5f, 1.0f), new Vector4(1, 1, 4, 1),
										  new Vector4(tx + tw,		ty + th*2,	0.5f, 1.0f), new Vector4(0, 0, 4, 1),
										  new Vector4(tx + tw*2,	ty + th*2,	0.5f, 1.0f), new Vector4(1, 0, 4, 1),

										  // face 0
										  //
										  new Vector4(tx + tw*2,	ty + th,	0.5f, 1.0f), new Vector4(0, 1, 0, 1),
										  new Vector4(tx + tw*3,	ty + th,	0.5f, 1.0f), new Vector4(1, 1, 0, 1),
										  new Vector4(tx + tw*2,	ty + th*2,	0.5f, 1.0f), new Vector4(0, 0, 0, 1),
										  new Vector4(tx + tw*3,	ty + th*2,	0.5f, 1.0f), new Vector4(1, 0, 0, 1),

										  // face 5
										  //
										  new Vector4(tx + tw*3,	ty + th,	0.5f, 1.0f), new Vector4(0, 1, 5, 1),
										  new Vector4(tx + tw*4,	ty + th,	0.5f, 1.0f), new Vector4(1, 1, 5, 1),
										  new Vector4(tx + tw*3,	ty + th*2,	0.5f, 1.0f), new Vector4(0, 0, 5, 1),
										  new Vector4(tx + tw*4,	ty + th*2,	0.5f, 1.0f), new Vector4(1, 0, 5, 1),

										  // face 2
										  //
										  new Vector4(tx + tw,		ty + th*2,	0.5f, 1.0f), new Vector4(0, 1, 2, 1),
										  new Vector4(tx + tw*2,	ty + th*2,	0.5f, 1.0f), new Vector4(1, 1, 2, 1),
										  new Vector4(tx + tw,		ty + th*3,	0.5f, 1.0f), new Vector4(0, 0, 2, 1),
										  new Vector4(tx + tw*2,	ty + th*3,	0.5f, 1.0f), new Vector4(1, 0, 2, 1),

										  // face 3
										  //
										  new Vector4(tx + tw,		ty,			0.5f, 1.0f), new Vector4(0, 1, 3, 1),
										  new Vector4(tx + tw*2,	ty,			0.5f, 1.0f), new Vector4(1, 1, 3, 1),
										  new Vector4(tx + tw,		ty + th,	0.5f, 1.0f), new Vector4(0, 0, 3, 1),
										  new Vector4(tx + tw*2,	ty + th,	0.5f, 1.0f), new Vector4(1, 0, 3, 1)

						  } );

				m_context.InputAssembler.SetVertexBuffers( 0, new VertexBufferBinding( vertices, 32, 0 ) );
				vertices.Dispose();

				m_context.Draw( 24, 0 );
			}
			else
			{
				// Instantiate Vertex buffer from vertex data
				var vertices = Buffer.Create( m_device, BindFlags.VertexBuffer, new[]
									  {
										  new Vector4(-texWh + xOffset, -texHh + yOffset, 0.5f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
										  new Vector4( texWh + xOffset, -texHh + yOffset, 0.5f, 1.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
										  new Vector4(-texWh + xOffset,  texHh + yOffset, 0.5f, 1.0f), new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
										  new Vector4( texWh + xOffset,  texHh + yOffset, 0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f)
									  } );

				m_context.InputAssembler.SetVertexBuffers( 0, new VertexBufferBinding( vertices, 32, 0 ) );
				vertices.Dispose();

				// draw background
				//
				m_context.PixelShader.Set( GetPsShader( "PS_Clear" ) );
				m_context.Draw( 4, 0 );

				SetupConstantBuffer();

				PsShaderWrap ps = null;

				if ( m_tex.m_texSrv.Description.Dimension == ShaderResourceViewDimension.Texture2D )
				{
					if ( DisplayMode == TextureDisplayMode.Source )
						m_psDictionary.TryGetValue( "PS_Tex2D_Sample", out ps );
					else if ( DisplayMode == TextureDisplayMode.Exported )
						m_psDictionary.TryGetValue( "PS_Tex2D_Sample_Exp", out ps );
					else
						m_psDictionary.TryGetValue( "PS_Tex2D_Sample_Diff", out ps );
					m_context.PixelShader.Set( ps.m_ps );
				}
				else if ( m_tex.m_texSrv.Description.Dimension == ShaderResourceViewDimension.Texture2DArray )
				{
					if ( DisplayMode == TextureDisplayMode.Source || DisplayMode == TextureDisplayMode.Exported )
						m_psDictionary.TryGetValue( "PS_Tex2DArray_Sample", out ps );
					else
						m_psDictionary.TryGetValue( "PS_Tex2DArray_Sample_Diff", out ps );
				}

				if ( ps == null )
				{
				}
				else
				{
					m_context.PixelShader.Set( ps.m_ps );
					m_context.Draw( 4, 0 );
				}
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Control.Paint"></see> event</summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs"></see> that contains the event data</param>
		protected override void OnPaint( PaintEventArgs e )
		{
			m_context.InputAssembler.InputLayout = m_inputLayout;
			m_context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
			m_context.VertexShader.Set( m_vsPassThrough );

			BlendStateDescription bsd = BlendStateDescription.Default();

			RenderTargetBlendDescription bsd0 = bsd.RenderTarget[0];
			bsd0.IsBlendEnabled = true;
			bsd0.BlendOperation = BlendOperation.Add;
			bsd0.AlphaBlendOperation = BlendOperation.Add;
			bsd0.SourceBlend = BlendOption.SourceAlpha;
			bsd0.SourceAlphaBlend = BlendOption.SourceAlpha;
			bsd0.DestinationBlend = BlendOption.InverseSourceAlpha;
			bsd0.DestinationAlphaBlend = BlendOption.InverseSourceAlpha;
			bsd.RenderTarget[0] = bsd0;

			BlendState bs = new BlendState( m_device, bsd );
			m_context.OutputMerger.SetBlendState( bs );
			bs.Dispose();

			m_context.OutputMerger.SetTargets( m_renderView );
			m_context.ClearRenderTargetView( m_renderView, Color.DarkGray );
			//m_context.OutputMerger.SetTargets( m_renderViewIntermediate );
			//m_context.ClearRenderTargetView( m_renderViewIntermediate, Color.DarkGray );

			RasterizerStateDescription rsd = RasterizerStateDescription.Default();
			rsd.IsFrontCounterClockwise = true;
			RasterizerState rs = new RasterizerState( m_device, rsd );
			m_context.Rasterizer.State = rs;
			rs.Dispose();
			m_context.Rasterizer.SetViewport( new Viewport( 0, 0, ClientSize.Width, ClientSize.Height, 0.0f, 1.0f ) );

			//DrawBackground();
			if ( m_tex != null )
				DrawTexture();

			//DrawGradient();

			//m_context.OutputMerger.SetTargets( m_renderView );

			//BlendStateDescription bsd2 = BlendStateDescription.Default();
			//BlendState bs2 = new BlendState( m_device, bsd2 );
			//m_context.OutputMerger.SetBlendState( bs2 );
			//bs2.Dispose();

			//m_context.PixelShader.Set( GetPsShader( "PS_Present" ) );
			//m_context.PixelShader.SetSampler( 0, m_ssPoint );
			//m_context.PixelShader.SetShaderResource( 0, m_renderViewIntermediateSrv );
			//SubmitFullscreenQuad();

			m_swapChain.Present( 0, PresentFlags.None );
		}

        /// <summary>
        /// Performs custom actions after the PaintBackground event occurs</summary>
        /// <param name="e">The <see cref="System.Windows.Forms.PaintEventArgs"/> instance containing the event data</param>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
			//base.OnPaintBackground( e );
        }

        /// <summary>
        /// Performs custom actions after the <see cref="E:System.Windows.Forms.Control.FontChanged"></see> event event occurs</summary>
        /// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data</param>
        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            if (m_isStarted)
                Invalidate();
        }

        //protected override void OnGotFocus(EventArgs e)
        //{
        //    System.Diagnostics.Debug.WriteLine("GotFocus");
        //}

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseWheel"></see> event</summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"></see> that contains the event data</param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            // e.Delta is by default multiple of WHEEL_DELTA which for current versions of windows is 120
            //
            int delta = e.Delta / 120;
            float mult = 1.1f;
            if (delta < 0)
                mult = 0.9f;

			float scaleX = -0.1f / (float)ClientSize.Width;
			float scaleY = 0.1f / (float)ClientSize.Height;
			int xDiff = e.Location.X - ClientSize.Width / 2;
			int yDiff = e.Location.Y - ClientSize.Height / 2;

            for (int idetent = 0; idetent < Math.Abs(delta); ++idetent)
            {
                m_texScale *= mult;

				m_texPosition += new Vector4((float)xDiff * scaleX, (float)yDiff * scaleY, 0.0f, 0.0f);
			}

            Invalidate();
            base.OnMouseWheel(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            m_leftMouseButtonDown = true;
            m_mouseMoveStartLocation_ = e.Location;
            Focus();
            base.OnMouseDown(e);
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            m_leftMouseButtonDown = false;
            base.OnMouseUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if ( m_leftMouseButtonDown )
            {
                System.Drawing.Size s = new System.Drawing.Size(m_mouseMoveStartLocation_);
                m_mouseMoveStartLocation_ = e.Location;
                System.Drawing.Point locDiff = e.Location - s;

                float scaleX = 2.0f / (float)ClientSize.Width;
                float scaleY = -2.0f / (float)ClientSize.Height;

                m_texPosition += new Vector4((float)locDiff.X * scaleX, (float)locDiff.Y * scaleY, 0.0f, 0.0f);
                Invalidate();
            }
            base.OnMouseMove(e);
        }

		private void MyButton1_SizeChanged(object sender, System.EventArgs e)
		{
			m_renderView.Dispose();

			//m_renderViewIntermediate.Dispose();
			//m_renderViewIntermediateSrv.Dispose();

			m_context.Flush();
			m_context.ClearState();

			// Create Device and SwapChain
			m_swapChain.ResizeBuffers( 1, ClientSize.Width, ClientSize.Height, backBufferFormat, SwapChainFlags.None );

			ResizeBackBuffer();

            Invalidate();

			//System.Diagnostics.Debug.WriteLine("ClientSize: {0}", ClientSize);
		}

        /// <summary>
        /// Client entry to initialize custom OpenGL resources</summary>
        protected virtual void Initialize() 
        {
            // Compile Vertex and Pixel shaders

            Assembly assembly = Assembly.GetExecutingAssembly();
            var names = assembly.GetManifestResourceNames();

            var resourceName = "TextureEditor.Resources.shaders.fx";
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                m_shadersFxText = reader.ReadToEnd();
            }

            var vertexShaderByteCode = ShaderBytecode.Compile(m_shadersFxText, "VS", "vs_4_0", ShaderFlags.None, EffectFlags.None);
            m_vsPassThrough = new VertexShader(m_device, vertexShaderByteCode);

            // Layout from VertexShader input signature
            m_inputLayout = new InputLayout(
                m_device,
                ShaderSignature.GetInputSignature(vertexShaderByteCode),
                new[]
                    {
                        new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                        new InputElement("TEXCOORD", 0, Format.R32G32B32A32_Float, 16, 0)
                    });

            SamplerStateDescription ssd = SamplerStateDescription.Default();
            ssd.Filter = Filter.MinMagMipPoint;
            m_ssPoint = new SamplerState(m_device, ssd);


			AddPsShader( "PS_Clear" );
			AddPsShader( "PS_Clear2" );
			AddPsShader( "PS_Tex2D_Sample" );
			AddPsShader( "PS_Tex2D_Sample_Exp" );
			AddPsShader( "PS_Tex2D_Sample_Diff" );
			AddPsShader( "PS_Tex2DArray_Sample" );
			AddPsShader( "PS_Tex2DArray_Sample_Diff" );
			AddPsShader( "PS_Tex2D_Load" );
			AddPsShader( "PS_TexCube_Sample" );
			AddPsShader( "PS_Present" );
			AddPsShader( "PS_Gradient" );
		}

        /// <summary>
        /// Client entry to unload custom OpenGL resources</summary>
        protected virtual void Shutdown() 
        {
			foreach( PsShaderWrap ps in m_psDictionary.Values )
			{
				ps.Dispose();
			}
			m_psDictionary.Clear();

			if (m_vsPassThrough != null)
            {
                m_vsPassThrough.Dispose();
                m_vsPassThrough = null;
            }

            if ( m_ssPoint != null )
            {
                m_ssPoint.Dispose();
                m_ssPoint = null;
            }

			TextureWrap.SafeDispose( ref m_tex );
			TextureWrap.SafeDispose( ref m_texExp );
        }

		void AddPsShader( string entryName )
		{
			m_psDictionary.Add(entryName, new PsShaderWrap(m_device, m_shadersFxText, entryName));
		}

		PixelShader GetPsShader( string entryName )
		{
			PsShaderWrap ps = null;
			m_psDictionary.TryGetValue( entryName, out ps );
			return ps.m_ps;
		}

		void ResizeBackBuffer()
		{
			// New RenderTargetView from the backbuffer
			var backBuffer = Texture2D.FromSwapChain<Texture2D>( m_swapChain, 0 );
			m_renderView = new RenderTargetView( m_device, backBuffer );

			//Texture2DDescription td = new Texture2DDescription();
			//td.ArraySize = 1;
			//td.BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource;
			//td.CpuAccessFlags = CpuAccessFlags.None;
			//td.Format = Format.R32G32B32A32_Float;
			//td.Height = backBuffer.Description.Height;
			//td.MipLevels = 1;
			//td.OptionFlags = ResourceOptionFlags.None;
			//td.SampleDescription.Count = 1;
			//td.SampleDescription.Quality = 0;
			//td.Usage = ResourceUsage.Default;
			//td.Width = backBuffer.Description.Width;
			//Texture2D rt = new Texture2D( m_device, td );

			//m_renderViewIntermediate = new RenderTargetView( m_device, rt );
			//m_renderViewIntermediateSrv = new ShaderResourceView( m_device, rt );
			//rt.Dispose();
			backBuffer.Dispose();
		}

        private void StartSharpDxIfNecessary()
        {
            if (!m_isStarted)
            {
                try
                {
                    // Attempt To Get A Device Context
					//m_hdc = User.GetDC( Handle );
					//if (m_hdc == IntPtr.Zero)
					//	throw new InvalidOperationException( "Can't get device context" );

					// SwapChain description
					var desc = new SwapChainDescription()
					{
						BufferCount = 1,
						ModeDescription= 
								   new ModeDescription( ClientSize.Width, ClientSize.Height,
												new Rational( 60, 1 ), backBufferFormat ),
						IsWindowed = true,
						OutputHandle = Handle,
						SampleDescription = new SampleDescription( 1, 0 ),
						SwapEffect = SwapEffect.Discard,
						Usage = Usage.RenderTargetOutput
					};

					// Create Device and SwapChain
					Device.CreateWithSwapChain( DriverType.Hardware, 0, desc, out m_device, out m_swapChain );
					m_context = m_device.ImmediateContext;

					// Ignore all windows events
					var factory = m_swapChain.GetParent<Factory>();
					factory.MakeWindowAssociation( Handle, WindowAssociationFlags.IgnoreAll );
					factory.Dispose();

					ResizeBackBuffer();
					//m_context.Rasterizer.SetViewport( new Viewport( 0, 0, ClientSize.Width, ClientSize.Height, 0.0f, 1.0f ) );
					//m_context.OutputMerger.SetTargets( m_renderView );

                    Initialize();
                    m_isStarted = true;
                }
                catch (Exception ex)
                {
                    StopSharpDx(false);
                    Outputs.WriteLine(OutputMessageType.Error, ex.Message);
                    Outputs.WriteLine(OutputMessageType.Info, ex.StackTrace);
                }
            }
        }

        private void StopSharpDx(bool disposing)
        {
            Shutdown();

			//if (disposing && (m_hdc != IntPtr.Zero))
			if (disposing)
			{
				m_renderView.Dispose();
				//m_renderViewIntermediate.Dispose();
				//m_renderViewIntermediateSrv.Dispose();
				m_context.ClearState();
				m_context.Flush();
				m_device.Dispose();
				m_context.Dispose();
				m_swapChain.Dispose();
			}

            m_isStarted = false;
        }

		public TextureProperties showResource( TextureMetadata metadata )
        {
			string localPath = metadata.LocalPath;
			if (localPath == null)
                return null;

			TextureWrap.SafeDispose( ref m_tex );
			TextureWrap.SafeDispose( ref m_texExp );

			TextureProperties tp = null;

			m_tex = TextureWrap.LoadTexture( localPath, m_device );
			string resFilenameDemoWin = TextureExporter.GetDataWinTexture( localPath );
			m_texExp = TextureWrap.LoadTexture( resFilenameDemoWin, m_device );

			if ( m_tex != null )
			{
				tp = new TextureProperties( this, m_tex, m_texExp );
			}

			if ( m_texExp == null )
			{
				DisplayMode = TextureDisplayMode.Source;
			}

			fitInWindowRequest = true;
            Invalidate();

			m_texProperties = tp;
			m_metadata = metadata;
			return tp;
        }

        public void fitInWindow()
        {
            m_texPosition = new Vector4(0, 0, 0, 0);
            m_texScale = 1.0f;
			fitInWindowRequest = true;
            Invalidate();
        }
		public void fitSize()
		{
			m_texPosition = new Vector4(0, 0, 0, 0);
			m_texScale = 1.0f;
			fitSizeRequest = true;
			Invalidate();
		}

		public TextureProperties SelectedTexture
		{
			get { return m_texProperties; }
		}

		public enum TextureDisplayMode
		{
			Source,
			Exported,
			Difference
		};

		public TextureDisplayMode DisplayMode { get; set; }
		public bool ShowRedChannel { get; set; }
		public bool ShowGreenChannel { get; set; }
		public bool ShowBlueChannel { get; set; }
		public bool ShowAlphaChannel { get; set; }
		public float SourceTextureGamma { get; set; }
		public float ExportedTextureGamma { get; set; }
		public int VisibleMip { get; set; }
		public int VisibleSlice { get; set; }
		
		//TextureSelectionContext m_textureSelectionContext;

        //private IntPtr m_hdc;
		private Device m_device;
		private SwapChain m_swapChain;
		private DeviceContext m_context;
		private RenderTargetView m_renderView;
		//private RenderTargetView m_renderViewIntermediate;
		//private ShaderResourceView m_renderViewIntermediateSrv;
		private bool m_isStarted;

        private InputLayout m_inputLayout;
        //private Buffer m_screenQuad;

		private string m_shadersFxText;
        private VertexShader m_vsPassThrough;
		private Dictionary<string, PsShaderWrap> m_psDictionary = new Dictionary<string, PsShaderWrap>();
		private SamplerState m_ssPoint;

		// original texture
		//
		private TextureWrap m_tex;
		// texture exported to dataWin
		//
		private TextureWrap m_texExp;

		private TextureProperties m_texProperties;
		private TextureMetadata m_metadata;

        private Vector4 m_texPosition = new Vector4(0, 0, 0, 0);
        private float m_texScale = 1.0f;
		private float m_texW = 2.0f;
		private float m_texH = 2.0f;

        private bool m_leftMouseButtonDown = false;
        private System.Drawing.Point m_mouseMoveStartLocation_;

		private bool fitSizeRequest = false;
		private bool fitInWindowRequest = false;

		private static readonly SharpDX.DXGI.Format backBufferFormat = Format.R8G8B8A8_UNorm_SRgb;
    }
}
