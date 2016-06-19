//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Dom;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.Timelines;
using Sce.Atf.Controls.SyntaxEditorControl;

using pico.Timeline;

//#pragma warning disable 0649 // suppress "field never set" warning

namespace picoTimelineEditor.DomNodeAdapters
{
    /// <summary>
    /// Adapts DomNode to a Key</summary>
	public class LuaScript : Key, ITimelineValidationCallback
    {
		///// <summary>
		///// Performs initialization when the adapter is connected to the editing context's DomNode.
		///// Raises the EditingContext NodeSet event and performs custom processing to adapt objects
		///// and subscribe to DomNode change and drag events.</summary>
		//protected override void OnNodeSet()
		//{
		//	base.OnNodeSet();
		//}

		/// <summary>
		/// Gets and sets the event's name</summary>
		public string SourceCode
		{
			get { return (string) DomNode.GetAttribute( Schema.keyLuaScriptType.sourceCodeAttribute ); }
			set { DomNode.SetAttribute( Schema.keyLuaScriptType.sourceCodeAttribute, value ); }
		}

		public Control LuaEditorControl
		{
			get
			{
				if ( m_luaEditor == null )
				{
					m_luaEditor = TextEditorFactory.CreateSyntaxHighlightingEditor();
					m_luaEditor.SetLanguage( Languages.Lua );
					m_luaEditor.Text = SourceCode;
					m_luaEditor.Dirty = true;
					m_luaEditor.Control.Dock = DockStyle.Fill;

					m_luaEditor.EditorTextChanged += delegate
					{
						if ( SourceCode != m_luaEditor.Text )
						{
							SourceCode = m_luaEditor.Text;

							Sce.Atf.Dom.DomNode root = DomNode.GetRoot();
							TimelineDocument document = root.Cast<TimelineDocument>();
							if ( document != null )
							{
								document.Dirty = true;
							}

							SourceCodeChanged.Raise( this, EventArgs.Empty );
						}
					};
				}

				return m_luaEditor.Control;
			}
		}

		public bool CanParentTo( DomNode parent )
		{
			return ValidateImpl( parent, 0 );
		}

		public bool Validate( DomNode parent )
		{
			return ValidateImpl( parent, 1 );
		}

		private bool ValidateImpl( DomNode parent, int validating )
		{
			if (parent.Type != Schema.trackType.Type)
				return false;

			return true;
		}

		private ISyntaxEditorControl m_luaEditor;

		public event EventHandler SourceCodeChanged;
	}
}




