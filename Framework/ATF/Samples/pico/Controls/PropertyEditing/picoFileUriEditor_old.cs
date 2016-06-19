//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

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
    /// UITypeEditor class for editing file paths (stored as strings or URIs)</summary>
	public class picoFileUriEditor : FileUriEditor
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
			: base( filter )
        {
        }

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
                m_initialDirectory2 = null;

                // Can't use the property descriptor to convert to a string because we need the LocalPath
                //  if possible.
                //string path = context.PropertyDescriptor.Converter.ConvertToString(value);
                string path = value as string;
                if (path == null)
                {
                    Uri uri = value as Uri;
                    if (uri != null)
                    {
                        if (uri.IsAbsoluteUri)
                            path = uri.LocalPath;
                        else
                            path = uri.OriginalString;
                    }
                }

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
                            m_initialDirectory2 = directory;

						//string filename = Path.GetFileName( fixedPath );
						//value = filename;
						value = fixedPath;
                    }
                }

                if (m_dialog2 != null &&
                    !string.IsNullOrEmpty(m_initialDirectory2))
                    m_dialog2.InitialDirectory = m_initialDirectory2;

				object newValue = base.EditValue(context, provider, value);
				//if ( newValue.Equals(value) )
				//	return newValue;

				string newPath = newValue as string;
				if (newPath == null)
				{
					Uri uri = newValue as Uri;
					if (uri != null)
					{
						if (uri.IsAbsoluteUri)
							newPath = uri.LocalPath;
						else
							newPath = uri.OriginalString;
					}
				}



				string picoDemoPath = Paths.PathToPicoDemoPath( newPath );
				return picoDemoPath;
				//Uri newUri = new Uri( picoDemoPath, UriKind.Relative );
				//return newUri;
				//return newPath;
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
		protected override void InitializeDialog( OpenFileDialog dialog )
		{
			base.InitializeDialog( dialog );

			m_dialog2 = dialog;
			if ( !string.IsNullOrEmpty( m_initialDirectory2 ) )
				m_dialog2.InitialDirectory = m_initialDirectory2;
		}

		private string m_initialDirectory2;
		private OpenFileDialog m_dialog2;
     }
}
