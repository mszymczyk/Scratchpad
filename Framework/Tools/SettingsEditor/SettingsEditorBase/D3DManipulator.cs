using System;
using System.ComponentModel.Composition;
using System.IO;

using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using DeviceContext = SharpDX.Direct3D11.DeviceContext;
using System.Collections.Generic;

using Sce.Atf;
using System.Windows.Forms;
using Sce.Atf.Applications;
using Sce.Atf.VectorMath;
using System.Runtime.InteropServices;
using System.Reflection;

namespace SettingsEditor
{
    /// <summary>
    /// Component for editing direction and orientation
    /// </summary>
    [Export( typeof( IInitializable ) )]
    [Export( typeof( D3DManipulator ) )]
    [PartCreationPolicy( CreationPolicy.Shared )]
    public class D3DManipulator : IInitializable, IControlHostClient
    {
        [ImportingConstructor]
        public D3DManipulator( ICommandService commandService,
            IControlHostService controlHostService,
            IContextRegistry contextRegistry )
        {
            m_controlHostService = controlHostService;
            m_contextRegistry = contextRegistry;

            m_controlInfo = new ControlInfo(
                "Direction Editor", //Is the ID in the layout. We'll localize DisplayName instead.
                "Edits direction of selected property".Localize(),
                StandardControlGroup.Bottom )
            {
                DisplayName = "Direction Manipulator".Localize()
            };
        }

        public static D3DManipulator Instance
        {
            get { return ms_d3dManipulator; }
        }

        public Control Control
        {
            get { return m_control; }
        }

        public Vec3F Direction
        {
            get { return m_direction; }
            set
            {
                Vec3F newDir = value;
                newDir.Normalize();
                if ( !newDir.Equals( m_direction, 0.001f ) )
                {
                    //float x = (float)Math.Round( newDir.X, 4 );
                    //float y = (float)Math.Round( newDir.Y, 4 );
                    //float z = (float)Math.Round( newDir.Z, 4 );
                    float x = newDir.X;
                    float y = newDir.Y;
                    float z = newDir.Z;
                    m_direction.X = x;
                    m_direction.Y = y;
                    m_direction.Z = z;

                    m_control.SetArrowDirection( new Vector3( x, y, z ) );

                    m_control.InvalidateWrap();
                }
            }
        }

        private void SetDirection( Vec3F newDir, bool force = false, bool fireValueChangedEvent = false, bool invalidateControl = false )
        {
            newDir.Normalize();
            if ( force || !newDir.Equals( m_direction, 0.001f ) )
            {
                //float x = (float)Math.Round( newDir.X, 4 );
                //float y = (float)Math.Round( newDir.Y, 4 );
                //float z = (float)Math.Round( newDir.Z, 4 );
                float x = newDir.X;
                float y = newDir.Y;
                float z = newDir.Z;
                m_direction.X = x;
                m_direction.Y = y;
                m_direction.Z = z;

                if ( fireValueChangedEvent )
                    OnValueChanged( EventArgs.Empty );

                if ( invalidateControl )
                    m_control.InvalidateWrap();
            }
        }

        /// <summary>
        /// Event that is raised after value is changed</summary>
        public event EventHandler DirectionChanged;

        /// <summary>
        /// Raises the <see cref="E:Sce.Atf.Controls.IntInputControl.ValueChanged"/> event</summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data</param>
        protected virtual void OnValueChanged( EventArgs e )
        {
            DirectionChanged.Raise( this, e );
        }

        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component by connecting to Hub</summary>
        void IInitializable.Initialize()
        {
            m_control = new Dx11Control( this );
            //m_control.Size = new System.Drawing.Size( 512, 512 );
            m_control.Dock = DockStyle.Fill;

            ms_d3dManipulator = this;

            // for case when we want to dock control as an editor
            // right now DirectionManiupulator is implemented to be used with PropertyView
            //m_controlHostService.RegisterControl( m_control, m_controlInfo, this );
        }

        #endregion

        #region IControlHostClient Members

        /// <summary>
        /// Notifies the client that its Control has been activated. Activation occurs when
        /// the control gets focus, or a parent "host" control gets focus.</summary>
        /// <param name="control">Client Control that was activated</param>
        void IControlHostClient.Activate( Control control )
        {
        }

        /// <summary>
        /// Notifies the client that its Control has been deactivated. Deactivation occurs when
        /// another control or "host" control gets focus.</summary>
        /// <param name="control">Client Control that was deactivated</param>
        void IControlHostClient.Deactivate( Control control )
        {
        }

        /// <summary>
        /// Requests permission to close the client's Control</summary>
        /// <param name="control">Client control to be closed</param>
        /// <returns>True if the control can close, or false to cancel</returns>
        bool IControlHostClient.Close( Control control )
        {
            return true;
        }

        #endregion

        internal class Dx11Control : InteropControl
        {
            private static readonly SharpDX.DXGI.Format ms_backBufferFormat = Format.R8G8B8A8_UNorm_SRgb;

            internal Dx11Control( D3DManipulator d3dManipulator )
            {
                m_outerD3DManipulator = d3dManipulator;

                // this is required to remove flickering of directx painted graphics
                // copied from Sce.Atf.Controls.Adaptable.D2dAdaptableControl
                DoubleBuffered = false;
                SetStyle(
                   ControlStyles.ResizeRedraw |
                   ControlStyles.AllPaintingInWmPaint |
                   ControlStyles.Opaque |
                   ControlStyles.UserPaint
                   , true );

                MouseDown += Dx11Control_MouseDown;
                MouseUp += Dx11Control_MouseUp;
                MouseMove += Dx11Control_MouseMove;
            }

            public void SetArrowDirection( Vector3 dir )
            {
                m_arrowWorld = CreateBasisZAxis( dir );
            }

            private void Dx11Control_MouseDown( object sender, MouseEventArgs e )
            {
                m_initialMousePos = new System.Drawing.Point( e.X, e.Y );
                m_initialMousePosValid = true;
                m_arrowStartRotation = Quaternion.RotationMatrix( m_arrowWorld );
            }

            private void Dx11Control_MouseUp( object sender, MouseEventArgs e )
            {
                m_initialMousePosValid = false;
            }

            private static Vector3 CalcVectorOnUnitSphere( int x, int y, int w, int h )
            {
                float xp = Sce.Atf.MathUtil.Clamp<float>( (float)x / (float)w - 0.5f, -0.5f, 0.5f );
                float yp = Sce.Atf.MathUtil.Clamp<float>( 0.5f - (float)y / (float)h, -0.5f, 0.5f );
                float r = (float)Math.Sqrt( xp * xp + yp * yp );
                Vector3 v;
                if ( r < 0.5f )
                {
                    v = new Vector3( 2 * xp, 2 * yp, (float)Math.Sqrt( 1 - 4 * r * r ) );
                }
                else
                {
                    v = new Vector3( xp / r, yp / r, 0 );
                }

                return v;
            }

            private Vector3 CalcAxisOfRotation( Vector3 v1, Vector3 v2 )
            {
                Vector3 v1_x_v2 = Vector3.Cross( v1, v2 );
                v1_x_v2.Normalize();
                return v1_x_v2;
            }

            // provided vector will be z axis of the frame
            private static Matrix CreateBasisZAxis( Vector3 n )
            {
                // http://orbit.dtu.dk/fedora/objects/orbit:113874/datastreams/file_75b66578-222e-4c7d-abdf-f7e255100209/content
                //
                Matrix m = Matrix.Identity;
                if ( n.Z < -0.9999999f )
                {
                    m.Row1 = new Vector4( 0, -1, 0, 0 );
                    m.Row2 = new Vector4( -1, 0, 0, 0 );
                    m.Row3 = new Vector4( n, 0 );
                    return m;
                }

                float a = 1 / (1 + n.Z);
                float b = -n.X * n.Y * a;
                m.Row1 = new Vector4( 1 - n.X * n.X * a, b, -n.X, 0 );
                m.Row2 = new Vector4( b, 1 - n.Y * n.Y * a, -n.Y, 0 );
                m.Row3 = new Vector4( n, 0 );
                return m;
            }

            private void Dx11Control_MouseMove( object sender, MouseEventArgs e )
            {
                if ( m_initialMousePosValid )
                {
                    if ( e.X != m_initialMousePos.X || e.Y != m_initialMousePos.Y )
                    {
                        int dx = e.X - m_initialMousePos.X;
                        int dy = e.Y - m_initialMousePos.Y;

                        // this is a unit sphere trackball
                        // for algorithm see http://www.uio.no/studier/emner/matnat/ifi/INF3320/h08/undervisningsmateriale/oblig2.pdf 

                        Vector3 v1 = CalcVectorOnUnitSphere( m_initialMousePos.X, m_initialMousePos.Y, ClientSize.Width, ClientSize.Height );
                        Vector3 v2 = CalcVectorOnUnitSphere( e.X, e.Y, ClientSize.Width, ClientSize.Height );

                        Vector3 a = CalcAxisOfRotation( v1, v2 );

                        float len = (float)Math.Sqrt( (float)(dx * dx + dy * dy) ); // angle from AntTweakBar
                        float angle = len * 0.01f;
                        //float angle = (float)Math.Acos( Vector3.Dot(v1, v2) ); // angle in the paper

                        Quaternion viewQ = Quaternion.RotationMatrix( m_view );
                        viewQ.Invert();
                        a = Vector3.Transform( a, viewQ );

                        Quaternion q = Quaternion.RotationAxis( a, angle );

                        Quaternion final = q * m_arrowStartRotation;
                        m_arrowWorld = Matrix.RotationQuaternion( final );
                    }
                    else
                    {
                        m_arrowWorld = Matrix.RotationQuaternion( m_arrowStartRotation );
                    }

                    Vec3F dir = new Vec3F( m_arrowWorld.Row3.X, m_arrowWorld.Row3.Y, m_arrowWorld.Row3.Z );
                    m_outerD3DManipulator.SetDirection( dir, false, true, true );
                }
            }

            /// <summary>
            /// Disposes resources</summary>
            /// <param name="disposing">True to release both managed and unmanaged resources;
            /// false to release only unmanaged resources</param>
            protected override void Dispose( bool disposing )
            {
                StopSharpDx( disposing );
                base.Dispose( disposing );
            }

            private void ResizeBackBuffer()
            {
                // color buffer
                // New RenderTargetView from the backbuffer
                var backBuffer = Texture2D.FromSwapChain<Texture2D>( m_swapChain, 0 );
                if ( m_renderView != null )
                    m_renderView.Dispose();
                m_renderView = new RenderTargetView( m_device, backBuffer );

                // depth buffer
                Texture2DDescription dsrd = new Texture2DDescription
                {
                    Format = Format.D16_UNorm,
                    ArraySize = 1,
                    MipLevels = 1,
                    Width = backBuffer.Description.Width,
                    Height = backBuffer.Description.Height,
                    SampleDescription = new SampleDescription( 1, 0 ),
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.DepthStencil,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None
                };

                Texture2D dsr = new Texture2D( m_device, dsrd );

                DepthStencilViewDescription dsvd = new DepthStencilViewDescription();
                dsvd.Format = Format.D16_UNorm;
                dsvd.Dimension = DepthStencilViewDimension.Texture2D;
                dsvd.Texture2D.MipSlice = 0;
                if ( m_depthStencilView != null )
                    m_depthStencilView.Dispose();
                m_depthStencilView = new DepthStencilView( m_device, dsr, dsvd );

                backBuffer.Dispose();
                dsr.Dispose();
            }

            private void StartSharpDxIfNecessary()
            {
                if ( !m_isStarted )
                {
                    try
                    {
                        // SwapChain description
                        var desc = new SwapChainDescription()
                        {
                            BufferCount = 1,
                            ModeDescription =
                                       new ModeDescription( ClientSize.Width, ClientSize.Height,
                                                    new Rational( 60, 1 ), ms_backBufferFormat ),
                            IsWindowed = true,
                            OutputHandle = Handle,
                            SampleDescription = new SampleDescription( 1, 0 ),
                            SwapEffect = SwapEffect.Discard,
                            Usage = Usage.RenderTargetOutput
                        };

                        // Create Device and SwapChain
                        Device.CreateWithSwapChain( DriverType.Hardware, DeviceCreationFlags.None, desc, out m_device, out m_swapChain );
                        m_context = m_device.ImmediateContext;

                        // Ignore all windows events
                        var factory = m_swapChain.GetParent<Factory>();
                        factory.MakeWindowAssociation( Handle, WindowAssociationFlags.IgnoreAll );
                        factory.Dispose();

                        ResizeBackBuffer();

                        Initialize();

                        m_isStarted = true;
                    }
                    catch ( Exception ex )
                    {
                        StopSharpDx( false );
                        Outputs.WriteLine( OutputMessageType.Error, ex.Message );
                        Outputs.WriteLine( OutputMessageType.Info, ex.StackTrace );
                    }
                }
            }

            private void StopSharpDx( bool disposing )
            {
                Shutdown();

                if ( disposing )
                {
                    m_renderView.Dispose();
                    m_depthStencilView.Dispose();
                    m_context.ClearState();
                    m_context.Flush();
                    m_device.Dispose();
                    m_context.Dispose();
                    m_swapChain.Dispose();
                }

                m_isStarted = false;
            }


            /// <summary>
            /// Client entry to initialize custom OpenGL resources</summary>
            protected virtual void Initialize()
            {
                // Compile Vertex and Pixel shaders

                Assembly assembly = Assembly.GetExecutingAssembly();
                var names = assembly.GetManifestResourceNames();

                var resourceName = "SettingsEditor.Resources.D3DManipulatorShaders.fx";
                using ( Stream stream = assembly.GetManifestResourceStream( resourceName ) )
                using ( StreamReader reader = new StreamReader( stream ) )
                {
                    m_D3DManipulatorShadersText = reader.ReadToEnd();
                }

                var vertexShaderByteCode = ShaderBytecode.Compile( m_D3DManipulatorShadersText, "VS", "vs_4_0", ShaderFlags.None, EffectFlags.None );
                m_vs = new VertexShader( m_device, vertexShaderByteCode );

                // Layout from VertexShader input signature
                m_inputLayout = new InputLayout(
                    m_device,
                    ShaderSignature.GetInputSignature( vertexShaderByteCode ),
                    new[]
                        {
                        new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                        new InputElement("NORMAL", 0, Format.R32G32B32_Float, 0, 1),
                        } );

                BlendStateDescription bsd = BlendStateDescription.Default();
                m_blendState = new BlendState( m_device, bsd );

                RasterizerStateDescription rsd = RasterizerStateDescription.Default();
                rsd.IsFrontCounterClockwise = true;
                //rsd.CullMode = CullMode.None;
                rsd.CullMode = CullMode.Back;
                //rsd.FillMode = FillMode.Wireframe;
                m_rasterizerState = new RasterizerState( m_device, rsd );

                DepthStencilStateDescription dssd = DepthStencilStateDescription.Default();
                m_depthStencilState = new DepthStencilState( m_device, dssd );

                ConstantBufferData data = new ConstantBufferData();
                data.worldViewProj = Matrix.Identity;
                data.world = Matrix.Identity;
                data.worldIT = Matrix.Identity;
                m_constantBuffer = Buffer.Create( m_device, BindFlags.ConstantBuffer, ref data );

                AddPsShader( "PS_Color" );

                CreateGrid();
                CreateArrow();
            }

            /// <summary>
            /// Client entry to unload custom OpenGL resources</summary>
            protected virtual void Shutdown()
            {
                foreach ( PsShaderWrap ps in m_psDictionary.Values )
                    ps.Dispose();
                m_psDictionary.Clear();

                if ( m_vs != null )
                {
                    m_vs.Dispose();
                    m_vs = null;
                }

                if (m_blendState != null)
                {
                    m_blendState.Dispose();
                    m_blendState = null;
                }
                if (m_rasterizerState != null)
                {
                    m_rasterizerState.Dispose();
                    m_rasterizerState = null;
                }
                if (m_depthStencilState != null)
                {
                    m_depthStencilState.Dispose();
                    m_depthStencilState = null;
                }
                if (m_constantBuffer != null)
                {
                    m_constantBuffer.Dispose();
                    m_constantBuffer = null;
                }
            }

            private void CreateGrid()
            {
                const int nSteps = 10;
                const int nLines = (nSteps + 1) * 2;
                const int nVertices = nLines * 2;
                Vector3[] vertices = new Vector3[nVertices];
                int iVertex = 0;

                const float w = 2.0f;
                float startZ = w * -0.5f;
                float endZ = startZ + w;
                const float dZ = w / nSteps;

                const float startX = w * -0.5f;
                const float endX = startX + w;
                const float dX = w / nSteps;

                float x = startX;

                for ( int ix = 0; ix <= nSteps; ++ix )
                {
                    vertices[iVertex] = new Vector3( x, 0, startZ );
                    ++iVertex;
                    vertices[iVertex] = new Vector3( x, 0, endZ );
                    ++iVertex;

                    x += dX;
                }

                float z = startZ;

                for ( int iz = 0; iz <= nSteps; ++iz )
                {
                    vertices[iVertex] = new Vector3( startX, 0, z );
                    ++iVertex;
                    vertices[iVertex] = new Vector3( endX, 0, z );
                    ++iVertex;

                    z += dZ;
                }

                System.Diagnostics.Debug.Assert( iVertex == nVertices );

                m_gridBuffer = Buffer.Create( m_device, BindFlags.VertexBuffer, vertices );
                m_nGridBufferVertices = nVertices;
            }


            private void CreateArrow()
            {
                const int nSubdivs = 16;
                const int nVerticesCylinder = nSubdivs * 2;
                const int nVerticesCylinderCap = nSubdivs + 1;
                const int nVerticesHead = nSubdivs * 2;
                const int nVerticesHeadCap = nSubdivs + 1;
                const int nVertices = nVerticesCylinder + nVerticesCylinderCap + nVerticesHead + nVerticesHeadCap;

                Vector3[] vertices = new Vector3[nVertices];
                Vector3[] normals = new Vector3[nVertices];
                int iVertex = 0;

                // cylinder
                const float cradius = 0.1f; // cylinder's radius
                const float hradius = 0.2f; // head's radius
                const float cbz = -0.5f; // cylinder's bottom cap z
                const float ctz = 0.25f; // cylinder's top cap z
                const float hh = 0.4f; // head's height

                float angle = 0;
                float deltaAngle = (2 * (float)Math.PI) / nSubdivs;

                for ( int ix = 0; ix < nSubdivs; ++ix )
                {
                    float xn = (float)Math.Cos( angle );
                    float yn = (float)Math.Sin( angle );
                    float x = xn * cradius;
                    float y = yn * cradius;

                    vertices[ix] = new Vector3( x, y, cbz );
                    normals[ix] = new Vector3( xn, yn, 0 );
                    vertices[ix + nSubdivs] = new Vector3( x, y, ctz );
                    normals[ix + nSubdivs] = new Vector3( xn, yn, 0 );
                    // bottom cup
                    vertices[ix + 2 * nSubdivs] = vertices[ix];
                    normals[ix + 2 * nSubdivs] = new Vector3( 0, 0, -1 );

                    angle += deltaAngle;
                }

                iVertex += nSubdivs * 3;

                // cylinder's bottom cap
                vertices[iVertex] = new Vector3( 0, 0, cbz );
                normals[iVertex] = new Vector3( 0, 0, -1 );
                int cylinderCapVertexIndex = iVertex;
                ++iVertex;



                // cylinder's head
                int firstCylindersHeadIndex = iVertex;
                angle = 0;
                for ( int ix = 0; ix < nSubdivs; ++ix )
                {
                    float xn = (float)Math.Cos( angle );
                    float yn = (float)Math.Sin( angle );
                    float x = xn * hradius;
                    float y = yn * hradius;

                    vertices[iVertex] = new Vector3( x, y, ctz );
                    // head tip, must have individual normal for each subdiv
                    vertices[iVertex + nSubdivs] = new Vector3( 0, 0, ctz + hh );
                    vertices[iVertex + 2 * nSubdivs] = new Vector3( x, y, ctz );

                    ++iVertex;

                    angle += deltaAngle;
                }

                iVertex += nSubdivs * 2;

                // cylinder's head normals
                for ( int ix = 0; ix < nSubdivs; ++ix )
                {
                    Vector3 v0 = vertices[firstCylindersHeadIndex + ix];
                    Vector3 v1 = vertices[firstCylindersHeadIndex + nSubdivs + ix];
                    int v2index = ix == (nSubdivs - 1) ? firstCylindersHeadIndex : firstCylindersHeadIndex + ix + 1;
                    Vector3 v2 = vertices[v2index];
                    Vector3 e0 = v2 - v0;
                    Vector3 e1 = v1 - v0;
                    Vector3 n = Vector3.Cross( e0, e1 );
                    n.Normalize();
                    normals[firstCylindersHeadIndex + ix] = n;
                    normals[firstCylindersHeadIndex + nSubdivs + ix] = n;
                    normals[firstCylindersHeadIndex + 2 * nSubdivs + ix] = new Vector3( 0, 0, -1 );

                    angle += deltaAngle;
                }

                // head cap center
                vertices[iVertex] = new Vector3( 0, 0, ctz );
                normals[iVertex] = new Vector3( 0, 0, -1 );
                int headCapVertexIndex = iVertex;
                ++iVertex;

                System.Diagnostics.Debug.Assert( iVertex == nVertices );

                m_arrowVertexBuffer = Buffer.Create( m_device, BindFlags.VertexBuffer, vertices );
                m_arrowNormalBuffer = Buffer.Create( m_device, BindFlags.VertexBuffer, normals );

                // index buffer
                const int nCylinderIndices = nSubdivs * 6;
                const int nCylinderCapIndices = nSubdivs * 3;
                const int nHeadIndices = nSubdivs * 3;
                const int nHeadCapIndices = nSubdivs * 3;
                const int nIndices = nCylinderIndices + nCylinderCapIndices + nHeadIndices + nHeadCapIndices;

                int[] indices = new int[nIndices];
                int iIndex = 0;

                // cylinder indices
                int baseVertex = 0;
                for ( int ix = 0; ix < nSubdivs; ++ix )
                {
                    indices[iIndex + 0] = baseVertex;
                    indices[iIndex + 1] = ix == (nSubdivs - 1) ? 0 : baseVertex + 1;
                    indices[iIndex + 2] = baseVertex + nSubdivs;
                    iIndex += 3;

                    indices[iIndex + 0] = baseVertex + nSubdivs;
                    indices[iIndex + 1] = ix == (nSubdivs - 1) ? 0 : baseVertex + 1;
                    indices[iIndex + 2] = ix == (nSubdivs - 1) ? nSubdivs : baseVertex + nSubdivs + 1;
                    iIndex += 3;

                    baseVertex += 1;
                }

                // bottom cap indices
                int firstCylinderCapVertex = nSubdivs * 2;
                baseVertex = firstCylinderCapVertex;
                for ( int ix = 0; ix < nSubdivs; ++ix )
                {
                    indices[iIndex + 0] = cylinderCapVertexIndex;
                    indices[iIndex + 1] = ix == (nSubdivs - 1) ? firstCylinderCapVertex : baseVertex + 1;
                    indices[iIndex + 2] = baseVertex;
                    iIndex += 3;

                    baseVertex += 1;
                }

                // head indices
                baseVertex = firstCylindersHeadIndex;
                for ( int ix = 0; ix < nSubdivs; ++ix )
                {
                    //indices[iIndex + 0] = headTipVertexIndex;
                    indices[iIndex + 0] = ix == (nSubdivs - 1) ? firstCylindersHeadIndex + nSubdivs : firstCylindersHeadIndex + nSubdivs + ix;
                    indices[iIndex + 1] = baseVertex;
                    indices[iIndex + 2] = ix == (nSubdivs - 1) ? firstCylindersHeadIndex : baseVertex + 1;
                    iIndex += 3;

                    baseVertex += 1;
                }

                // head cap indices
                baseVertex = firstCylindersHeadIndex + nSubdivs * 2;
                for ( int ix = 0; ix < nSubdivs; ++ix )
                {
                    indices[iIndex + 0] = headCapVertexIndex;
                    indices[iIndex + 1] = ix == (nSubdivs - 1) ? firstCylindersHeadIndex + nSubdivs * 2 : baseVertex + 1;
                    indices[iIndex + 2] = baseVertex;
                    iIndex += 3;

                    baseVertex += 1;
                }

                System.Diagnostics.Debug.Assert( iIndex == nIndices );

                m_arrowIndexBuffer = Buffer.Create( m_device, BindFlags.IndexBuffer, indices );
                m_nArrowIndices = nIndices;
            }

            [StructLayout( LayoutKind.Explicit, Size = 192 )]
            struct ConstantBufferData
            {
                [FieldOffset( 0 )]
                public Matrix worldViewProj;
                [FieldOffset( 64 )]
                public Matrix world;
                [FieldOffset( 128 )]
                public Matrix worldIT;
                //[FieldOffset( 192 )]
                //public Vec4F color;
            };

            public class PsShaderWrap : IDisposable
            {
                public PsShaderWrap( Device device, string sourceCode, string entryName )
                {
                    var pixelShaderByteCode = ShaderBytecode.Compile( sourceCode, entryName, "ps_4_0", ShaderFlags.None, EffectFlags.None );
                    m_ps = new PixelShader( device, pixelShaderByteCode );
                }

                ~PsShaderWrap()
                {
                    Dispose( false );
                }

                private void Dispose( bool disposing )
                {
                    if ( !disposing )
                        return;

                    if ( m_ps != null )
                    {
                        m_ps.Dispose();
                        m_ps = null;
                    }
                }
                public void Dispose()
                {
                    Dispose( true );
                    GC.SuppressFinalize( this );
                }

                public PixelShader m_ps;
            }

            void AddPsShader( string entryName )
            {
                m_psDictionary.Add( entryName, new PsShaderWrap( m_device, m_D3DManipulatorShadersText, entryName ) );
            }

            PixelShader GetPsShader( string entryName )
            {
                PsShaderWrap ps = null;
                m_psDictionary.TryGetValue( entryName, out ps );
                return ps.m_ps;
            }

            public void InvalidateWrap()
            {
                if ( m_invalidated )
                    return;

                m_invalidated = true;
                Invalidate();
            }

            /// <summary>
            /// Raises the <see cref="E:System.Windows.Forms.Control.Paint"></see> event</summary>
            /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs"></see> that contains the event data</param>
            protected override void OnPaint( PaintEventArgs e )
            {
                StartSharpDxIfNecessary();

                m_context.ClearState();

                m_context.ClearRenderTargetView( m_renderView, SharpDX.Color.LightGray );
                m_context.ClearDepthStencilView( m_depthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0 );

                m_context.OutputMerger.SetTargets( m_depthStencilView, m_renderView );
                m_context.OutputMerger.SetBlendState( m_blendState );
                m_context.OutputMerger.DepthStencilState = m_depthStencilState;

                m_context.Rasterizer.State = m_rasterizerState;
                m_context.Rasterizer.SetViewport( new Viewport( 0, 0, ClientSize.Width, ClientSize.Height, 0.0f, 1.0f ) );

                m_context.InputAssembler.InputLayout = m_inputLayout;
                m_context.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineList;
                m_context.VertexShader.Set( m_vs );

                ConstantBufferData data = new ConstantBufferData();

                float aspect = (float)ClientSize.Width / (float)ClientSize.Height;

                Matrix.PerspectiveFovRH( (float)Math.PI * 0.25f, aspect, 0.01f, 100, out m_proj );
                m_view = Matrix.LookAtRH( new Vector3( 1.5f, 1.5f, 1.5f ), new Vector3( 0, 0, 0 ), new Vector3( 0, 1, 0 ) );
                //m_view = Matrix.LookAtRH( new Vector3( 0, 0, 2.5f ), new Vector3( 0, 0, 0 ), new Vector3( 0, 1, 0 ) ); // for debug

                data.worldViewProj = m_gridWorld * m_view * m_proj;
                data.world = m_gridWorld;
                data.worldIT = Matrix.Transpose( Matrix.Invert( m_gridWorld ) );

                m_context.UpdateSubresource( ref data, m_constantBuffer );
                m_context.VertexShader.SetConstantBuffer( 0, m_constantBuffer );
                m_context.PixelShader.SetConstantBuffer( 0, m_constantBuffer );

                // draw grid
                m_context.InputAssembler.SetVertexBuffers( 0, new VertexBufferBinding( m_gridBuffer, 12, 0 ) );
                m_context.PixelShader.Set( GetPsShader( "PS_Color" ) );
                m_context.Draw( m_nGridBufferVertices, 0 );

                data.worldViewProj = m_arrowWorld * m_view * m_proj;
                data.world = m_arrowWorld;
                data.worldIT = Matrix.Transpose( Matrix.Invert( m_arrowWorld ) );
                m_context.UpdateSubresource( ref data, m_constantBuffer );

                // draw arrow
                m_context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
                m_context.InputAssembler.SetVertexBuffers( 0, new VertexBufferBinding( m_arrowVertexBuffer, 12, 0 ) );
                m_context.InputAssembler.SetVertexBuffers( 1, new VertexBufferBinding( m_arrowNormalBuffer, 12, 0 ) );
                m_context.InputAssembler.SetIndexBuffer( m_arrowIndexBuffer, Format.R32_UInt, 0 );
                m_context.DrawIndexed( m_nArrowIndices, 0, 0 );

                m_swapChain.Present( 0, PresentFlags.None );
                m_invalidated = false;
            }

            private Device m_device;
            private SwapChain m_swapChain;
            private DeviceContext m_context;
            private RenderTargetView m_renderView;
            private DepthStencilView m_depthStencilView;
            private bool m_isStarted;

            private string m_D3DManipulatorShadersText;
            private VertexShader m_vs;
            private InputLayout m_inputLayout;
            private BlendState m_blendState;
            private RasterizerState m_rasterizerState;
            private DepthStencilState m_depthStencilState;

            private Dictionary<string, PsShaderWrap> m_psDictionary = new Dictionary<string, PsShaderWrap>();

            private Buffer m_constantBuffer;

            private Buffer m_gridBuffer;
            private int m_nGridBufferVertices;

            private Buffer m_arrowVertexBuffer;
            private Buffer m_arrowNormalBuffer;
            private Buffer m_arrowIndexBuffer;
            private int m_nArrowIndices;

            private System.Drawing.Point m_initialMousePos = new System.Drawing.Point( 0, 0 );
            private bool m_initialMousePosValid = false;
            private bool m_invalidated = false;

            private Matrix m_proj = Matrix.Identity;
            private Matrix m_view = Matrix.Identity;
            private Matrix m_gridWorld = Matrix.Identity;
            private Matrix m_arrowWorld = Matrix.Identity;
            private Quaternion m_arrowStartRotation = Quaternion.Identity;

            private D3DManipulator m_outerD3DManipulator;
        }

        // Required MEF Imports
        private readonly IControlHostService m_controlHostService;
        private readonly IContextRegistry m_contextRegistry;

        private readonly ControlInfo m_controlInfo;

        private Dx11Control m_control;
        private Vec3F m_direction = new Vec3F( 0, 0, 1 );

        private static D3DManipulator ms_d3dManipulator;
    }
}

