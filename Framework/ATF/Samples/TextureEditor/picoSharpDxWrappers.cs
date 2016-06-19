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

		public void Dispose( bool disposing )
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

	public class TextureWrap : IDisposable
	{
		public static TextureWrap LoadTexture( string filename, SharpDX.Direct3D11.Device device )
		{
			SharpDX.Direct3D11.ImageInformation? ii = SharpDX.Direct3D11.ImageInformation.FromFile( filename );
			if ( ii != null )
			{
				ImageLoadInformation ili = ImageLoadInformation.Default;
				ili.MipLevels = ii.Value.MipLevels;
				ili.PSrcInfo = IntPtr.Zero;

				SharpDX.Direct3D11.Resource res = SharpDX.Direct3D11.Texture2D.FromFile( device, filename, ili );

				if ( res != null )
				{
					SharpDX.Direct3D11.ShaderResourceView srv;
					if ( ( ii.Value.OptionFlags & ResourceOptionFlags.TextureCube ) != 0 )
					{
						Texture2D tex = res as Texture2D;
						Texture2DDescription texDesc = tex.Description;

						ShaderResourceViewDescription d = new ShaderResourceViewDescription();
						d.Dimension = ShaderResourceViewDimension.Texture2DArray;
						d.Format = texDesc.Format;
						d.Texture2DArray.ArraySize = texDesc.ArraySize;
						d.Texture2DArray.MipLevels = texDesc.MipLevels;
						d.Texture2DArray.MostDetailedMip = 0;
						srv = new ShaderResourceView( device, res, d );
					}
					else
					{
						srv = new ShaderResourceView( device, res );
					}

					return new TextureWrap( filename, res, srv );
				}
			}

			return null;
		}

		public static void SafeDispose( ref TextureWrap tex )
		{
			if ( tex != null )
			{
				tex.Dispose();
				tex = null;
			}
		}

		private TextureWrap( string filename, SharpDX.Direct3D11.Resource tex, SharpDX.Direct3D11.ShaderResourceView texSrv )
		{
			m_tex = tex;
			m_texSrv = texSrv;

			Filename = filename;

			SharpDX.DXGI.Format tmpFormat = SharpDX.DXGI.Format.Unknown;

			ResourceDimension dim = m_tex.Dimension;
			TextureType = dim;

			if ( dim == ResourceDimension.Texture1D )
			{
				Texture1D tex1D = m_tex as Texture1D;
				Texture1DDescription desc = tex1D.Description;
				Format = desc.Format;
				Width = desc.Width;
				Height = 1;
				Depth = 1;
				MipLevels = desc.MipLevels;
				ArraySize = desc.ArraySize;
				IsCubeMap = ( desc.OptionFlags & ResourceOptionFlags.TextureCube ) > 0;
				tmpFormat = desc.Format;
			}
			else if ( dim == ResourceDimension.Texture2D )
			{
				Texture2D tex2D = m_tex as Texture2D;
				Texture2DDescription desc = tex2D.Description;
				Format = desc.Format;
				Width = desc.Width;
				Height = desc.Height;
				Depth = 1;
				MipLevels = desc.MipLevels;
				ArraySize = desc.ArraySize;
				IsCubeMap = ( desc.OptionFlags & ResourceOptionFlags.TextureCube ) > 0;
				if ( IsCubeMap )
					ArraySize /= 6;
				tmpFormat = desc.Format;
			}
			else if ( dim == ResourceDimension.Texture3D )
			{
				Texture3D tex3D = m_tex as Texture3D;
				Texture3DDescription desc = tex3D.Description;
				Format = desc.Format;
				Width = desc.Width;
				Height = desc.Height;
				Depth = desc.Depth;
				MipLevels = desc.MipLevels;
				ArraySize = 0;
				IsCubeMap = false;
				tmpFormat = desc.Format;
			}
			else
			{
				throw new Exception( "Unsupported resource type type" );
			}

			if ( tmpFormat == SharpDX.DXGI.Format.B8G8R8A8_UNorm_SRgb
			||	tmpFormat == SharpDX.DXGI.Format.B8G8R8X8_UNorm_SRgb
			||	tmpFormat == SharpDX.DXGI.Format.BC1_UNorm_SRgb
			||	tmpFormat == SharpDX.DXGI.Format.BC2_UNorm_SRgb
			||	tmpFormat == SharpDX.DXGI.Format.BC3_UNorm_SRgb
			||	tmpFormat == SharpDX.DXGI.Format.BC7_UNorm_SRgb
			||	tmpFormat == SharpDX.DXGI.Format.R8G8B8A8_UNorm_SRgb
				)
			{
				IsSrgbFormat = true;
			}
			else
			{
				IsSrgbFormat = false;
			}
		}

		~TextureWrap()
		{
			Dispose( false );
		}

		public void Dispose( bool disposing )
		{
			if ( !disposing )
				return;

			if ( m_tex != null )
			{
				m_tex.Dispose();
				m_tex = null;
			}

			if ( m_texSrv != null )
			{
				m_texSrv.Dispose();
				m_texSrv = null;
			}
		}
		public void Dispose()
		{
			Dispose( true );
			GC.SuppressFinalize( this );
		}

		public SharpDX.Direct3D11.Resource m_tex;
		public SharpDX.Direct3D11.ShaderResourceView m_texSrv;

		/// <summary>
		/// File this texture was loaded from</summary>
		public string Filename { get; set; }

		/// <summary>
		/// Type of texture</summary>
		public SharpDX.Direct3D11.ResourceDimension TextureType { get; set; }

		/// <summary>
		/// Textures texel format</summary>
		public SharpDX.DXGI.Format Format { get; set; }

		/// <summary>
		/// Does this texture has srgb format?</summary>
		public bool IsSrgbFormat { get; set; }

		/// <summary>
		/// Width of texture</summary>
		public int Width { get; set; }

		/// <summary>
		/// Height of texture</summary>
		public int Height { get; set; }

		/// <summary>
		/// Depth of texture</summary>
		public int Depth { get; set; }

		/// <summary>
		/// Number of mipmaps this texture has</summary>
		public int MipLevels { get; set; }

		/// <summary>
		/// Number of slices this texture has</summary>
		public int ArraySize { get; set; }

		/// <summary>
		/// Is this texture a cubemap?</summary>
		public bool IsCubeMap { get; set; }
	}
}
