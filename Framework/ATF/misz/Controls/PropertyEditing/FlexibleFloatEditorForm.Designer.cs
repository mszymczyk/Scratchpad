namespace misz.Controls.PropertyEditing
{
    partial class FlexibleFloatEditorForm
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
			if (disposing && (components != null))
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
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxStep = new Sce.Atf.Controls.NumericTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxSoftMax = new Sce.Atf.Controls.NumericTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxSoftMin = new Sce.Atf.Controls.NumericTextBox();
            this.Cancel = new System.Windows.Forms.Button();
            this.OK = new System.Windows.Forms.Button();
            this.textBoxMin = new Sce.Atf.Controls.NumericTextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxMax = new Sce.Atf.Controls.NumericTextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.textBoxName = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.textBoxName);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.textBoxStep);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.textBoxSoftMax);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.textBoxSoftMin);
            this.groupBox1.Location = new System.Drawing.Point(12, 90);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(241, 131);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 75);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(55, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Step Size:";
            // 
            // textBoxStep
            // 
            this.textBoxStep.Location = new System.Drawing.Point(88, 72);
            this.textBoxStep.Name = "textBoxStep";
            this.textBoxStep.ScaleFactor = 1D;
            this.textBoxStep.Size = new System.Drawing.Size(147, 20);
            this.textBoxStep.TabIndex = 4;
            this.textBoxStep.Text = "0";
            this.textBoxStep.Value = 0F;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 47);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(76, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Soft Maximum:";
            // 
            // textBoxSoftMax
            // 
            this.textBoxSoftMax.Location = new System.Drawing.Point(88, 44);
            this.textBoxSoftMax.Name = "textBoxSoftMax";
            this.textBoxSoftMax.ScaleFactor = 1D;
            this.textBoxSoftMax.Size = new System.Drawing.Size(147, 20);
            this.textBoxSoftMax.TabIndex = 2;
            this.textBoxSoftMax.Text = "0";
            this.textBoxSoftMax.Value = 0F;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Soft Minimum:";
            // 
            // textBoxSoftMin
            // 
            this.textBoxSoftMin.Location = new System.Drawing.Point(88, 16);
            this.textBoxSoftMin.Name = "textBoxSoftMin";
            this.textBoxSoftMin.ScaleFactor = 1D;
            this.textBoxSoftMin.Size = new System.Drawing.Size(147, 20);
            this.textBoxSoftMin.TabIndex = 0;
            this.textBoxSoftMin.Text = "0";
            this.textBoxSoftMin.Value = 0F;
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(178, 236);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 23);
            this.Cancel.TabIndex = 1;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // OK
            // 
            this.OK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OK.Location = new System.Drawing.Point(100, 236);
            this.OK.Name = "OK";
            this.OK.Size = new System.Drawing.Size(75, 23);
            this.OK.TabIndex = 2;
            this.OK.Text = "OK";
            this.OK.UseVisualStyleBackColor = true;
            // 
            // textBoxMin
            // 
            this.textBoxMin.Enabled = false;
            this.textBoxMin.Location = new System.Drawing.Point(88, 13);
            this.textBoxMin.Name = "textBoxMin";
            this.textBoxMin.ScaleFactor = 1D;
            this.textBoxMin.Size = new System.Drawing.Size(147, 20);
            this.textBoxMin.TabIndex = 2;
            this.textBoxMin.Text = "0";
            this.textBoxMin.Value = 0F;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 16);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(51, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Minimum:";
            // 
            // textBoxMax
            // 
            this.textBoxMax.Enabled = false;
            this.textBoxMax.Location = new System.Drawing.Point(88, 43);
            this.textBoxMax.Name = "textBoxMax";
            this.textBoxMax.ScaleFactor = 1D;
            this.textBoxMax.Size = new System.Drawing.Size(147, 20);
            this.textBoxMax.TabIndex = 4;
            this.textBoxMax.Text = "0";
            this.textBoxMax.Value = 0F;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 46);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(54, 13);
            this.label5.TabIndex = 5;
            this.label5.Text = "Maximum:";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.textBoxMax);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.textBoxMin);
            this.groupBox2.Location = new System.Drawing.Point(12, 13);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(241, 71);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            // 
            // textBoxName
            // 
            this.textBoxName.Location = new System.Drawing.Point(88, 100);
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new System.Drawing.Size(147, 20);
            this.textBoxName.TabIndex = 4;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 103);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(38, 13);
            this.label6.TabIndex = 6;
            this.label6.Text = "Name:";
            // 
            // FlexibleFloatEditorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(265, 273);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.OK);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.groupBox1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FlexibleFloatEditorForm";
            this.ShowIcon = false;
            this.Text = "Customize - ";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button Cancel;
		private System.Windows.Forms.Button OK;
		private System.Windows.Forms.Label label3;
		private Sce.Atf.Controls.NumericTextBox textBoxStep;
		private System.Windows.Forms.Label label2;
		private Sce.Atf.Controls.NumericTextBox textBoxSoftMax;
		private System.Windows.Forms.Label label1;
		private Sce.Atf.Controls.NumericTextBox textBoxSoftMin;
        private Sce.Atf.Controls.NumericTextBox textBoxMin;
        private System.Windows.Forms.Label label4;
        private Sce.Atf.Controls.NumericTextBox textBoxMax;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBoxName;
    }
}