using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TextureEditor
{
	public partial class ProgressOutputWindow : Form
	{
		public ProgressOutputWindow()
		{
			InitializeComponent();
		}

		private void CancelButtonObj_Click( object sender, EventArgs e )
		{
			//Close();
			if ( CancelClicked != null )
				CancelClicked( sender, e );
		}

		private void CloseButtonObj_Click( object sender, EventArgs e )
		{
			Close();
		}

		public void AddInfo( string str )
		{
			//Color prevColor = richTextBox1.SelectionColor;
			//richTextBox1.SelectionColor = Color.Blue;
			richTextBox1.AppendText( str );
			//richTextBox1.SelectionColor = prevColor;
		}

		public void AddError( string str )
		{
			Color prevColor = richTextBox1.SelectionColor;
			richTextBox1.SelectionColor = Color.Red;
			richTextBox1.AppendText( str );
			richTextBox1.SelectionColor = prevColor;
		}

		public void EnableUserClose()
		{
			CancelButtonObj.Enabled = false;
			CloseButtonObj.Enabled = true;
		}

		public void SetProgress( int nDone, int nTotal )
		{
			float progress01 = (float)nDone / (float)nTotal;
			float p = Math.Max( Math.Min( progress01, 1.0f ), 0.0f );
			int prog = (int)( p * 100 );
			progressBar1.Value = prog;
			progressLabel.Text = "" + nDone + " / " + nTotal;
		}

		public event EventHandler CancelClicked;
	}
}
