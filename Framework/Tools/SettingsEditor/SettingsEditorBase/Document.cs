using System;
using System.IO;
using System.Xml;

using Sce.Atf;
using Sce.Atf.Dom;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using System.Collections.Generic;

namespace SettingsEditor
{
    /// <summary>
    /// Adapts root node to IDocument</summary>
    public class Document : DomDocument
    {
        /// <summary>
        /// Gets the document client's file type name</summary>
        public override string Type
        {
            get { return "SettingsFile".Localize(); }
        }

        /// <summary>
        /// Gets or sets whether the document is dirty (does it differ from its file)</summary>
        public override bool Dirty
        {
            get
            {
                //DocumentEditingContext editingContext = DomNode.Cast<DocumentEditingContext>();
                //return editingContext.History.Dirty;
                return false;
            }
            set
            {
                //DocumentEditingContext editingContext = DomNode.Cast<DocumentEditingContext>();
                //editingContext.History.Dirty = value;
            }
        }

        /// <summary>
        /// Raises the DirtyChanged event and performs custom processing</summary>
        /// <param name="e">EventArgs containing event data</param>
        protected override void OnDirtyChanged( EventArgs e )
        {
            UpdateControlInfo();

            base.OnDirtyChanged( e );
        }

        /// <summary>
        /// Raises the UriChanged event and performs custom processing</summary>
        /// <param name="e">UriChangedEventArgs containing event data</param>
        protected override void OnUriChanged( UriChangedEventArgs e )
        {
            UpdateControlInfo();

            // can't validate path here, if users saves as to directory outside SettingsRoot
            // throwing here will crash the app
            // moved it to Editor.Save

            base.OnUriChanged( e );
        }

        private void UpdateControlInfo()
        {
            string filePath;
            if ( Uri.IsAbsoluteUri )
                filePath = Uri.LocalPath;
            else
                filePath = Uri.OriginalString;

            string fileName = System.IO.Path.GetFileName( filePath );
            if ( Dirty )
                fileName += "*";

            ControlInfo.Name = fileName;
            ControlInfo.Description = filePath;
        }

        public void SaveImpl()
        {
            // save only when this setting file was explicitly saved by user
            // ie, don't save unnamed files
            //
            if ( !ExplicitlySavedByUser )
                return;

            string filePath = Uri.LocalPath;
            FileMode fileMode = File.Exists( filePath ) ? FileMode.Truncate : FileMode.OpenOrCreate;
            using ( FileStream stream = new FileStream( filePath, fileMode ) )
            {
                DomXmlWriter writer = new DomXmlWriter( SchemaLoader.s_schemaLoader.TypeCollection );
                //writer.PersistDefaultAttributes = true; // causes file size to grow considerably

                //XmlWriterSettings settings = new XmlWriterSettings();
                //settings.Indent = true;
                //settings.IndentChars = "\t";
                //settings.NewLineHandling = NewLineHandling.Replace;
                //settings.NewLineChars = "\r\n";
                //settings.NewLineOnAttributes = true;

                //writer.Write( DomNode.GetRoot(), stream, Uri, settings );
                writer.Write( DomNode.GetRoot(), stream, Uri );

                Outputs.WriteLine( OutputMessageType.Info, "Saving '" + filePath + "' to disk ( " + DateTime.Now.ToString() + " )" );
            }

            SaveTime = DateTime.Now;
        }

        /// <summary>
        /// Gets the list of all groups in this document</summary>
        public IList<Group> Groups
        {
            get { return GetChildList<Group>( Schema.settingsFileType.groupChild ); }
        }

        public DocumentControl Control { get; set; }
        public ControlInfo ControlInfo { get; set; }

        // turn on automatic saving only after user explicitly save the file to requested location
        //
        public bool ExplicitlySavedByUser { get; set; }

        public string DescFilePath { get; set; }
        public string DescFileRelativePath
        {
            get { return (string) DomNode.GetAttribute( Schema.settingsFileType.settingsDescFileAttribute ); }
            set { DomNode.SetAttribute( Schema.settingsFileType.settingsDescFileAttribute, value ); }
        }

        public string SettingsDescFile
        {
            get { return (string) DomNode.GetAttribute( Schema.settingsFileType.settingsDescFileAttribute ); }
            set { DomNode.SetAttribute( Schema.settingsFileType.settingsDescFileAttribute, value ); }
        }

        public string ShaderOutputFile
        {
            get { return (string) DomNode.GetAttribute( Schema.settingsFileType.shaderOutputFileAttribute ); }
            set { DomNode.SetAttribute( Schema.settingsFileType.shaderOutputFileAttribute, value ); }
        }

        public string PathRelativeToData { get; set; }

        public DateTime LoadTime { get; set; }
        public DateTime SaveTime { get; set; }
    }
}
