namespace TextureEditor
{
	partial class ProgressOutputWindow
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing )
		{
			if ( disposing && ( components != null ) )
			{
				components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.richTextBox1 = new System.Windows.Forms.RichTextBox();
			this.CancelButtonObj = new System.Windows.Forms.Button();
			this.CloseButtonObj = new System.Windows.Forms.Button();
			this.progressLabel = new System.Windows.Forms.Label();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.progressLabel);
			this.groupBox1.Controls.Add(this.progressBar1);
			this.groupBox1.Controls.Add(this.richTextBox1);
			this.groupBox1.Location = new System.Drawing.Point(12, 6);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(1140, 446);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			// 
			// progressBar1
			// 
			this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.progressBar1.Location = new System.Drawing.Point(6, 404);
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new System.Drawing.Size(1128, 36);
			this.progressBar1.TabIndex = 1;
			// 
			// richTextBox1
			// 
			this.richTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.richTextBox1.Location = new System.Drawing.Point(6, 12);
			this.richTextBox1.Name = "richTextBox1";
			this.richTextBox1.Size = new System.Drawing.Size(1128, 373);
			this.richTextBox1.TabIndex = 0;
			this.richTextBox1.Text = "";
			// 
			// CancelButtonObj
			// 
			this.CancelButtonObj.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButtonObj.Location = new System.Drawing.Point(1077, 463);
			this.CancelButtonObj.Name = "CancelButtonObj";
			this.CancelButtonObj.Size = new System.Drawing.Size(75, 23);
			this.CancelButtonObj.TabIndex = 1;
			this.CancelButtonObj.Text = "Cancel";
			this.CancelButtonObj.UseVisualStyleBackColor = true;
			this.CancelButtonObj.Click += new System.EventHandler(this.CancelButtonObj_Click);
			// 
			// CloseButtonObj
			// 
			this.CloseButtonObj.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButtonObj.Enabled = false;
			this.CloseButtonObj.Location = new System.Drawing.Point(996, 463);
			this.CloseButtonObj.Name = "CloseButtonObj";
			this.CloseButtonObj.Size = new System.Drawing.Size(75, 23);
			this.CloseButtonObj.TabIndex = 2;
			this.CloseButtonObj.Text = "Close";
			this.CloseButtonObj.UseVisualStyleBackColor = true;
			this.CloseButtonObj.Click += new System.EventHandler(this.CloseButtonObj_Click);
			// 
			// progressLabel
			// 
			this.progressLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.progressLabel.AutoSize = true;
			this.progressLabel.Location = new System.Drawing.Point(6, 388);
			this.progressLabel.Name = "progressLabel";
			this.progressLabel.Size = new System.Drawing.Size(0, 13);
			this.progressLabel.TabIndex = 2;
			// 
			// ProgressOutputWindow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1164, 498);
			this.ControlBox = false;
			this.Controls.Add(this.CloseButtonObj);
			this.Controls.Add(this.CancelButtonObj);
			this.Controls.Add(this.groupBox1);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(500, 300);
			this.Name = "ProgressOutputWindow";
			this.ShowIcon = false;
			this.Text = "ProgressOutputWindow";
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.RichTextBox richTextBox1;
		private System.Windows.Forms.ProgressBar progressBar1;
		private System.Windows.Forms.Button CancelButtonObj;
		private System.Windows.Forms.Button CloseButtonObj;
		private System.Windows.Forms.Label progressLabel;


	}
}