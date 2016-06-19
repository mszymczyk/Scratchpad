using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;

using Sce.Atf;
using Sce.Atf.Controls.PropertyEditing;

namespace pico.Controls.PropertyEditing
{
    /// <summary>
    /// UITypeEditor class for editing file paths (stored ONLY as strings)
	/// This class is for selecting file only within PICO_DEMO file system hierarchy
	/// It's mainly a copy of ATF's FileUriEditor with modifications to handle relative path names
	/// </summary>
    public class picoFileUriEditor : FileNameEditor, IAnnotatedParams
    {
        /// <summary>
        /// Constructor</summary>
        public picoFileUriEditor()
        {
        }

        /// <summary>
        /// Constructor with file filter</summary>
        /// <param name="filter">Filter string for open file dialog</param>
        public picoFileUriEditor(string filter)
        {
            m_filter = filter;
        }

        /// <summary>
        /// Gets or sets the filter string for the open file dialog</summary>
        public string Filter
        {
            get { return m_filter; }
            set { m_filter = value; }
        }

        /// <summary>
        /// Gets or sets the full path of the text editor associated to view the file contents</summary>
        public string AssociatedTextEditor
        {
            get
            {
                return string.IsNullOrEmpty(m_associatedTextEditor) ? GlobalDefaultTextEditor : m_associatedTextEditor;
            }

            set { m_associatedTextEditor = value; }
        }

        /// <summary>
        /// Gets or sets the default text editor for viewng file contents</summary>
        public static string GlobalDefaultTextEditor
        {
            get { return s_globalDefaultTextEditor; }
            set { s_globalDefaultTextEditor = value; }
        }

        #region IAnnotatedParams Members

        /// <summary>
        /// Initializes the control</summary>
        /// <param name="parameters">Editor parameters</param>
        public void Initialize(string[] parameters)
        {
            if (parameters.Length == 1)
            {
                m_filter = parameters[0];
            }
            else if (parameters.Length > 1)
            {
                // This editor only accept on parameter.
                // If there are more than one, then combine them into one.
                StringBuilder filter = new StringBuilder();
                for (int i = 0; i < parameters.Length - 1; i++)
                {
                    filter.AppendFormat("{0},", parameters[i]);
                }
                filter.Append(parameters[parameters.Length - 1]);
                m_filter = filter.ToString();                
            }                        
        }

        #endregion

        /// <summary>
        /// Displays a file open dialog box to allow the user to edit the specified path (value).
        /// Sets the initial directory of the file open dialog box to be the path, if it's valid.</summary>
        /// <param name="context">Type descriptor context</param>
        /// <param name="provider">Service provider</param>
        /// <param name="value">Path to edit</param>
        /// <returns>Edited path</returns>
        public override object EditValue(System.ComponentModel.ITypeDescriptorContext context, System.IServiceProvider provider, object value)
        {
            // An exception thrown here will be unhandled by PropertyEditingControl and will bring down the app,
            //  so it seems prudent to not let that happen due to the feature of setting the initial directory
            //  or due to problems with the .Net OpenFileDialog. Different kinds of exceptions can be thrown:
            // System.ArgumentException -- Path.GetDirectoryName() if the path contains invalid characters.
            // System.IO.PathTooLongException -- Path.GetDirectoryName() if the path is too long.
            // InvalidOperationException -- System.Windows.Forms.OpenFileDialog if the path is badly formed or
            //  contains invalid characters, on Windows XP.
            try
            {
                // Try to set the initial directory to be the path that's in 'value'.
                // Also, fix up the path by removing forward slashes. This is critical for Windows XP.
                m_initialDirectory = null;

                // Can't use the property descriptor to convert to a string because we need the LocalPath
                //  if possible.
                //string path = context.PropertyDescriptor.Converter.ConvertToString(value);
                string path = value as string;

                if (!string.IsNullOrEmpty(path))
                {
                    string fixedPath = path.Replace('/', '\\');
                    if (fixedPath != path)
                        value = context.PropertyDescriptor.Converter.ConvertFromString(fixedPath);

					fixedPath = Paths.LocalPathToPicoDataAbsolutePath( fixedPath );

                    if (File.Exists(fixedPath))
                    {
                        string directory = Path.GetDirectoryName(fixedPath);
                        if (!string.IsNullOrEmpty(directory))
                            m_initialDirectory = directory;

						string filename = Path.GetFileName( fixedPath );
						value = filename;
                    }
                }

                if (m_dialog != null &&
                    !string.IsNullOrEmpty(m_initialDirectory))
                    m_dialog.InitialDirectory = m_initialDirectory;

                object newValue = base.EditValue(context, provider, value);

				string newPath = newValue as string;

				if ( !string.IsNullOrEmpty( newPath ) )
				{
					if ( m_initialDirectory != null )
						newPath = Path.Combine( m_initialDirectory, newPath );

					string picoDemoPath = Paths.PathToPicoDemoPath( newPath );
					return picoDemoPath;
				}

				return string.Empty;
            }
            catch (Exception e)
            {
                Outputs.WriteLine(OutputMessageType.Error, e.Message);
                return value;
            }
        }

        /// <summary>
        /// Initializes the open file dialog</summary>
        /// <param name="dialog">The open file dialog.</param>
        protected override void InitializeDialog(OpenFileDialog dialog)
        {
            base.InitializeDialog(dialog);
            
            if (m_filter != null)
                dialog.Filter = m_filter;

            m_dialog = dialog;
            if (!string.IsNullOrEmpty(m_initialDirectory))
                m_dialog.InitialDirectory = m_initialDirectory;

			m_dialog.CustomPlaces.Add( pico.Paths.PICO_DEMO_data );
			m_dialog.CustomPlaces.Add( pico.Paths.PICO_DEMO_data + "assets" );
		}

        private static string s_globalDefaultTextEditor = @"notepad.exe";

        private string m_filter;
        private string m_initialDirectory;
        private OpenFileDialog m_dialog;
        private string m_associatedTextEditor;
     }
}
