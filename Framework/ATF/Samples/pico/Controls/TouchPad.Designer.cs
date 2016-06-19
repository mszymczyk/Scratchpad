namespace pico.Controls
{
	partial class TouchPad
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
			if ( disposing && (components != null) )
			{
				components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.labelHint = new System.Windows.Forms.Label();
			this.splitButtonFrameSelection = new Sce.Atf.Controls.SplitButton();
			this.checkBoxFollowSelection = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// labelHint
			// 
			this.labelHint.AutoSize = true;
			this.labelHint.Enabled = false;
			this.labelHint.Location = new System.Drawing.Point(0, 25);
			this.labelHint.Name = "labelHint";
			this.labelHint.Size = new System.Drawing.Size(100, 23);
			this.labelHint.TabIndex = 0;
			this.labelHint.Text = "label1";
			// 
			// splitButtonFrameSelection
			// 
			this.splitButtonFrameSelection.AutoSize = true;
			this.splitButtonFrameSelection.Location = new System.Drawing.Point(0, 0);
			this.splitButtonFrameSelection.Name = "splitButtonFrameSelection";
			this.splitButtonFrameSelection.Size = new System.Drawing.Size(96, 23);
			this.splitButtonFrameSelection.TabIndex = 0;
			this.splitButtonFrameSelection.Text = "splitButton1";
			this.splitButtonFrameSelection.UseVisualStyleBackColor = true;
			// 
			// checkBoxFollowSelection
			// 
			this.checkBoxFollowSelection.AutoSize = true;
			this.checkBoxFollowSelection.Location = new System.Drawing.Point(100, 4);
			this.checkBoxFollowSelection.Name = "checkBoxFollowSelection";
			this.checkBoxFollowSelection.Size = new System.Drawing.Size(104, 24);
			this.checkBoxFollowSelection.TabIndex = 0;
			this.checkBoxFollowSelection.Text = "Follow Selection";
			this.checkBoxFollowSelection.UseVisualStyleBackColor = true;
			// 
			// TouchPad
			// 
			this.BackColor = System.Drawing.SystemColors.ActiveCaption;
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label labelHint;
		private Sce.Atf.Controls.SplitButton splitButtonFrameSelection;
		private System.Windows.Forms.CheckBox checkBoxFollowSelection;

	}
}
