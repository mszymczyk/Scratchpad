//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel.Composition;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Rendering;
using Sce.Atf.Rendering.Dom;
using System;
using Sce.Atf.Dom;
using Sce.Atf.Adaptation;
using System.IO;
using Sce.Atf.Controls;
using SharpDX.DXGI;
using System.Threading;
using System.Collections.Generic;

namespace TextureEditor
{   
    /// <summary>
    /// A MEF component for providing user commands related to the RenderView component</summary>
    //[Export(typeof(IInitializable))]
    //[Export(typeof(TextureViewCommands))]
    //[PartCreationPolicy(CreationPolicy.Shared)]
    public class TextureViewCommands : ICommandClient
    {
		public TextureViewCommands( ICommandService commandService, TexturePreviewWindowSharpDX panel3D, MainForm mainForm, SchemaLoader schemaLoader )
        {
            m_previewWindow = panel3D;
			m_mainForm = mainForm;
			m_schemaLoader = schemaLoader;

            commandService.RegisterCommand(
               Command.FitInWindow,
               StandardMenu.View,
               CommandGroup,
               "Fit In Window",
               "Fits texture to cover whole window",
               Keys.F,
               //null,
			   Sce.Atf.Resources.FitToSizeImage,
               CommandVisibility.Default,
               this);
		
			commandService.RegisterCommand(
			   Command.FitSize,
			   StandardMenu.View,
			   CommandGroup,
			   "Fit Size",
			   "Resizes texture to it's original size",
			   Keys.F,
				//null,
			   Sce.Atf.Resources.PinGreyImage,
			   CommandVisibility.Default,
			   this);

			commandService.RegisterCommand(
				Command.ShowSource,
				StandardMenu.View,
				CommandGroup,
				"Show source",
				"Displays source texture",
				Keys.None,
				Resources.SourceImage,
				CommandVisibility.Default,
				this );

			commandService.RegisterCommand(
				Command.ShowExported,
				StandardMenu.View,
				CommandGroup,
				"Show exported",
				"Displays exported texture",
				Keys.None,
				Resources.ExportedImage,
				CommandVisibility.Default,
				this );

			commandService.RegisterCommand(
				Command.ShowDiff,
				StandardMenu.View,
				CommandGroup,
				"Show difference",
				"Displays difference between source and exported image",
				Keys.None,
				Resources.DifferenceImage,
				CommandVisibility.Default,
				this );



			commandService.RegisterCommand(
				Command.ShowRedChannel,
				StandardMenu.View,
				CommandGroup,
				"Show red channel",
				"Enables display of red channel",
				Keys.None,
				Resources.ShowRedChannelImage,
				CommandVisibility.Default,
				this );

			commandService.RegisterCommand(
				Command.ShowGreenChannel,
				StandardMenu.View,
				CommandGroup,
				"Show green channel",
				"Enables display of green channel",
				Keys.None,
				Resources.ShowGreenChannelImage,
				CommandVisibility.Default,
				this );

			commandService.RegisterCommand(
				Command.ShowBlueChannel,
				StandardMenu.View,
				CommandGroup,
				"Show blue channel",
				"Enables display of blue channel",
				Keys.None,
				Resources.ShowBlueChannelImage,
				CommandVisibility.Default,
				this );

			commandService.RegisterCommand(
				Command.ShowAlphaChannel,
				StandardMenu.View,
				CommandGroup,
				"Show alpha channel",
				"Enables display of alpha channel",
				Keys.None,
				Resources.ShowAlphaChannelImage,
				CommandVisibility.Default,
				this );


			commandService.RegisterCommand(
			   Command.ExportOne,
			   StandardMenu.File,
			   CommandGroup,
			   "Export One",
			   "Exports currently selected texture",
			   Keys.R,
				//null,
			   Sce.Atf.Resources.ComponentImage,
			   CommandVisibility.Default,
			   this );

			commandService.RegisterCommand(
			   Command.ExpartAll,
			   StandardMenu.File,
			   CommandGroup,
			   "Export All",
			   "Exports all textures",
			   Keys.A,
				//null,
			   Sce.Atf.Resources.ComponentsImage,
			   CommandVisibility.Default,
			   this );

			m_sourceTextureGamma = new ToolStripComboBox();
			m_sourceTextureGamma.DropDownStyle = ComboBoxStyle.DropDownList;
			m_sourceTextureGamma.Name = "Source texture gamma".Localize();
			m_sourceTextureGamma.ComboBox.Width = m_sourceTextureGamma.ComboBox.Width / 3;
			m_sourceTextureGamma.ComboBox.Items.Add( "1,0" );
			m_sourceTextureGamma.ComboBox.Items.Add( "2,2" );
			m_sourceTextureGamma.ComboBox.Items.Add( "2,4" );
			m_sourceTextureGamma.ComboBox.SelectedIndex = 1;
			m_sourceTextureGamma.ToolTipText = "Source texture gamma".Localize();
			//m_sourceTextureGamma.ComboBox.Validating += ComboBox_Validating;
			//m_sourceTextureGamma.ComboBox.KeyPress += ComboBox_KeyPress;
			m_sourceTextureGamma.ComboBox.SelectedIndexChanged += sourceTexture_ComboBox_SelectedIndexChanged;

			m_exportedTextureGamma = new ToolStripComboBox();
			m_exportedTextureGamma.DropDownStyle = ComboBoxStyle.DropDownList;
			m_exportedTextureGamma.Name = "Exported texture gamma".Localize();
			m_exportedTextureGamma.ComboBox.Width = m_exportedTextureGamma.ComboBox.Width / 3;
			m_exportedTextureGamma.ComboBox.Items.Add( "1,0" );
			m_exportedTextureGamma.ComboBox.Items.Add( "2,2" );
			m_exportedTextureGamma.ComboBox.Items.Add( "2,4" );
			m_exportedTextureGamma.ComboBox.SelectedIndex = 0;
			m_exportedTextureGamma.ToolTipText = "Exported texture gamma".Localize();
			m_exportedTextureGamma.ComboBox.SelectedIndexChanged += exportedTexture_ComboBox_SelectedIndexChanged;


			// Register comboBoxes for visible mip and visible slice, in the edit menu's toolbar
			//
			m_visibleMip = new ToolStripComboBox();
			m_visibleMip.DropDownStyle = ComboBoxStyle.DropDownList;
			m_visibleMip.Name = "Visible Mip".Localize();
			//m_visibleMip.Text = "Visible Mip".Localize();
			m_visibleMip.ComboBox.Width = m_visibleMip.ComboBox.Width / 2;
			//m_visibleMip.ComboBox.DataSource = Enum.GetValues( typeof( SnapFromMode ) );
			m_visibleMip.ComboBox.Items.Add( "Mip 0" );
			m_visibleMip.ComboBox.SelectedIndex = 0;
			m_visibleMip.ToolTipText = "Selects mip for preview".Localize();
			m_visibleMip.ComboBox.SelectedIndexChanged += m_visibleMip_SelectedIndexChanged;


			m_visibleSlice = new ToolStripComboBox();
			m_visibleSlice.DropDownStyle = ComboBoxStyle.DropDownList;
			m_visibleSlice.Name = "Visible Slice".Localize();
			m_visibleSlice.ComboBox.Width = m_visibleSlice.ComboBox.Width / 2;
			m_visibleSlice.ComboBox.Items.Add( "Slice 0" );
			m_visibleSlice.ComboBox.SelectedIndex = 0;
			m_visibleSlice.ToolTipText = "Selects slice for preview".Localize();
			m_visibleSlice.ComboBox.SelectedIndexChanged += m_visibleSlice_SelectedIndexChanged;

			MenuInfo editMenuInfo = MenuInfo.Edit;
			editMenuInfo.GetToolStrip().Items.Add( m_sourceTextureGamma );
			editMenuInfo.GetToolStrip().Items.Add( m_exportedTextureGamma );
			editMenuInfo.GetToolStrip().Items.Add( m_visibleMip );
			editMenuInfo.GetToolStrip().Items.Add( m_visibleSlice );
		}

		void m_visibleMip_SelectedIndexChanged( object sender, EventArgs e )
		{
			if (m_previewWindow == null)
				return;

			ComboBox cb = sender as ComboBox;
			m_previewWindow.VisibleMip = cb.SelectedIndex;
			m_previewWindow.Invalidate();
		}

		void m_visibleSlice_SelectedIndexChanged( object sender, EventArgs e )
		{
			if (m_previewWindow == null)
				return;

			ComboBox cb = sender as ComboBox;
			m_previewWindow.VisibleSlice = cb.SelectedIndex;
			m_previewWindow.Invalidate();
		}

		void sourceTexture_ComboBox_SelectedIndexChanged( object sender, EventArgs e )
		{
			if (m_previewWindow == null)
				return;

			ComboBox cb = sender as ComboBox;
			float fval = 2.2f;
			if (float.TryParse( cb.Text, out fval ))
			{
				m_previewWindow.SourceTextureGamma = fval;
				m_previewWindow.Invalidate();
			}
		}

		void exportedTexture_ComboBox_SelectedIndexChanged( object sender, EventArgs e )
		{
			if (m_previewWindow == null)
				return;

			ComboBox cb = sender as ComboBox;
			float fval = 2.2f;
			if (float.TryParse( cb.Text, out fval ))
			{
				m_previewWindow.ExportedTextureGamma = fval;
				m_previewWindow.Invalidate();
			}
		}

		//void ComboBox_Validating( object sender, System.ComponentModel.CancelEventArgs e )
		//{
		//	ComboBox tscb = sender as ComboBox;
		//	float fval = 2.2f;
		//	if ( ! float.TryParse( tscb.Text, out fval ) )
		//	{
		//		e.Cancel = true;
		//	}
		//}

		//void ComboBox_KeyPress( object sender, KeyPressEventArgs e )
		//{
		//	if ( e.KeyChar == 13 )
		//	{
		//		ComboBox cb = sender as ComboBox;
		//		if (cb != null)
		//			cb.FindForm().Validate();
		//	}
		//}

		public void onShowResource( TextureProperties tp )
		{
			int nMips = tp.MipLevels;
			if ( nMips != m_visibleMip.ComboBox.Items.Count )
			{
				int oldSelection = m_visibleMip.ComboBox.SelectedIndex;
				m_visibleMip.ComboBox.Items.Clear();
				for (int i = 0; i < nMips; ++i)
					m_visibleMip.ComboBox.Items.Add( "Mip " + i );

				if (oldSelection < nMips)
					m_visibleMip.ComboBox.SelectedIndex = oldSelection;
				else
					m_visibleMip.ComboBox.SelectedIndex = nMips - 1;
			}

			int nSlices = tp.ArraySize;
			if (nSlices != m_visibleSlice.ComboBox.Items.Count)
			{
				int oldSelection = m_visibleSlice.ComboBox.SelectedIndex;
				m_visibleSlice.ComboBox.Items.Clear();
				for (int i = 0; i < nSlices; ++i)
					m_visibleSlice.ComboBox.Items.Add( "Slice " + i );

				if (oldSelection < nSlices)
					m_visibleSlice.ComboBox.SelectedIndex = oldSelection;
				else
					m_visibleSlice.ComboBox.SelectedIndex = nSlices - 1;
			}
		}

        /// <summary>
        /// Rendering modes</summary>
        protected enum Command
        {
            FitInWindow,
			FitSize,
			ShowSource,
			ShowExported,
			ShowDiff,

			ShowRedChannel,
			ShowGreenChannel,
			ShowBlueChannel,
			ShowAlphaChannel,

			ExportOne,
			ExpartAll
        }

        #region ICommandClient Members

        /// <summary>
        /// Can the client do the command?</summary>
        /// <param name="commandTag">Command</param>
        /// <returns>True iff client can do the command</returns>
        public bool CanDoCommand(object commandTag)
        {
            if (!(commandTag is Command))
                return false;

            if (m_previewWindow == null)
                return false;
            
            switch ((Command)commandTag)
            {
                case Command.FitInWindow:
                    return true;
				case Command.FitSize:
					return true;
				case Command.ShowSource:
					return true;
				case Command.ShowExported:
				case Command.ShowDiff:
					{
						if ( m_previewWindow.SelectedTexture != null )
							return m_previewWindow.SelectedTexture.ExportedTexture != null;
						return false;
					}
				case Command.ExportOne:
					return m_previewWindow.SelectedTexture != null;
				case Command.ExpartAll:
					return true;

				case Command.ShowRedChannel:
				case Command.ShowGreenChannel:
				case Command.ShowBlueChannel:
				case Command.ShowAlphaChannel:
					return true;
			}

            return false;
        }

        /// <summary>
        /// Do a command</summary>
        /// <param name="commandTag">Command</param>
        public void DoCommand(object commandTag)
        {
            if (m_previewWindow == null)
                return; 
            
            if (commandTag is Command)
            {
                switch ((Command)commandTag)
                {
                    case Command.FitInWindow:
                        m_previewWindow.fitInWindow();
                        break;
					case Command.FitSize:
						m_previewWindow.fitSize();
						break;
					case Command.ShowSource:
						m_previewWindow.DisplayMode = TexturePreviewWindowSharpDX.TextureDisplayMode.Source;
						m_previewWindow.Invalidate();
						break;
					case Command.ShowExported:
						m_previewWindow.DisplayMode = TexturePreviewWindowSharpDX.TextureDisplayMode.Exported;
						m_previewWindow.Invalidate();
						break;
					case Command.ShowDiff:
						m_previewWindow.DisplayMode = TexturePreviewWindowSharpDX.TextureDisplayMode.Difference;
						m_previewWindow.Invalidate();
						break;


					case Command.ShowRedChannel:
						m_previewWindow.ShowRedChannel = !m_previewWindow.ShowRedChannel;
						m_previewWindow.Invalidate();
						break;
					case Command.ShowGreenChannel:
						m_previewWindow.ShowGreenChannel = !m_previewWindow.ShowGreenChannel;
						m_previewWindow.Invalidate();
						break;
					case Command.ShowBlueChannel:
						m_previewWindow.ShowBlueChannel = !m_previewWindow.ShowBlueChannel;
						m_previewWindow.Invalidate();
						break;
					case Command.ShowAlphaChannel:
						m_previewWindow.ShowAlphaChannel = !m_previewWindow.ShowAlphaChannel;
						m_previewWindow.Invalidate();
						break;


					case Command.ExportOne:
						ExportOne();
						break;
					case Command.ExpartAll:
						ExportAll( m_mainForm, false );
						break;
				}
            }
        }

        /// <summary>
        /// Updates command state for given command</summary>
        /// <param name="commandTag">Command</param>
        /// <param name="state">Command state to update</param>
        public void UpdateCommand(object commandTag, Sce.Atf.Applications.CommandState state)
        {
            if (m_previewWindow == null)
                return;

			if ( commandTag is Command )
			{
				switch ( (Command)commandTag )
				{
					//case Command.RenderSmooth:
					//	state.Check = ( activeControl.RenderState.RenderMode & RenderMode.Smooth ) != 0;
					//	break;
					case Command.ShowSource:
						state.Check = m_previewWindow.DisplayMode == TexturePreviewWindowSharpDX.TextureDisplayMode.Source;
						break;
					case Command.ShowExported:
						state.Check = m_previewWindow.DisplayMode == TexturePreviewWindowSharpDX.TextureDisplayMode.Exported;
						break;
					case Command.ShowDiff:
						state.Check = m_previewWindow.DisplayMode == TexturePreviewWindowSharpDX.TextureDisplayMode.Difference;
						break;

					case Command.ShowRedChannel:
						state.Check = m_previewWindow.ShowRedChannel;
						break;
					case Command.ShowGreenChannel:
						state.Check = m_previewWindow.ShowGreenChannel;
						break;
					case Command.ShowBlueChannel:
						state.Check = m_previewWindow.ShowBlueChannel;
						break;
					case Command.ShowAlphaChannel:
						state.Check = m_previewWindow.ShowAlphaChannel;
						break;
				}
			}
        }

        #endregion

		void ExportOne()
		{
			TextureProperties tp = m_previewWindow.SelectedTexture;
			TextureExporter te = new TextureExporter( m_mainForm, m_schemaLoader );
			te.ExportOne( new Uri(tp.FileUri.LocalPath + ".metadata") );
		}

		public static void ExportAll( MainForm mainForm, bool batchExport )
		{
			TextureExporter te = new TextureExporter( mainForm, m_schemaLoader );
			te.ExportAll( batchExport );
		}

		private MainForm m_mainForm;
		private static SchemaLoader m_schemaLoader = null;
        private TexturePreviewWindowSharpDX m_previewWindow;

		private ToolStripComboBox m_sourceTextureGamma;
		private ToolStripComboBox m_exportedTextureGamma;
		private ToolStripComboBox m_visibleMip;
		private ToolStripComboBox m_visibleSlice;

		private static string CommandGroup = "TexturePreviewCommands";
    }
}
