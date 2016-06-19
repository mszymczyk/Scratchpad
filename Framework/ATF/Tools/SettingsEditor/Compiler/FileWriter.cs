using System;
using System.Collections.Generic;

namespace SettingsEditor
{
    /// <summary>
    /// Utility class for writing files line by line
    /// </summary>
    public class FileWriter
    {
        public FileWriter()
        {
        }

        public void AddLine( string line )
        {
            m_lines.Add( m_indentStr + line );
        }

        public void EmptyLine()
        {
            m_lines.Add( "" );
        }

        public void IncIndent()
        {
            m_indent += 1;
            m_indentStr += "\t";
        }

        public void DecIndent()
        {
            if (m_indent == 0)
                throw new InvalidOperationException( "Indent is 0 and can't be further decreased!" );

            m_indent -= 1;
            m_indentStr = m_indentStr.Substring( 0, m_indentStr.Length - 1 );
        }

        public List<string> Lines { get { return m_lines; } }

        private List<string> m_lines = new List<string>();
        private int m_indent = 0;
        private string m_indentStr = "";
    }

}