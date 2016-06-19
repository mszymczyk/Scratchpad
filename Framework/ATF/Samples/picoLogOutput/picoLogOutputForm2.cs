using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace pico.LogOutput
{
	public partial class picoLogOutputForm2 : System.Windows.Forms.Panel
	{
		public picoLogOutputForm2()
		{
			InitializeComponent();

			m_logDt_ = new picoLogDataTable();
			this.olvData.DataSource = m_logDt_.Data;
		}

		private void listViewDataSet_FormatCell( object sender, BrightIdeasSoftware.FormatCellEventArgs e )
		{
			string[] colorNames = new string[] { "red", "green", "blue", "yellow", "black", "white" };
			string text = e.SubItem.Text.ToLowerInvariant();
			foreach (string name in colorNames)
			{
				if (text.Contains( name ))
				{
					if (text.Contains( "bk-" + name ))
						e.SubItem.BackColor = Color.FromName( name );
					else
						e.SubItem.ForeColor = Color.FromName( name );
				}
			}
			FontStyle style = FontStyle.Regular;
			if (text.Contains( "bold" ))
				style |= FontStyle.Bold;
			if (text.Contains( "italic" ))
				style |= FontStyle.Italic;
			if (text.Contains( "underline" ))
				style |= FontStyle.Underline;
			if (text.Contains( "strikeout" ))
				style |= FontStyle.Strikeout;

			if (style != FontStyle.Regular)
			{
				e.SubItem.Font = new Font( e.SubItem.Font, style );
			}
		}

		picoLogDataTable m_logDt_;
	}
}
