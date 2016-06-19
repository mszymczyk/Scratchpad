namespace pico.LogOutput
{
	partial class picoLogOutputForm3
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
			this.dataGridView1 = new System.Windows.Forms.DataGridView();
			this.columnType = new System.Windows.Forms.DataGridViewImageColumn();
			this.columnOrdinal = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.columnTag = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.columnDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.columnFile = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.columnLine = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.buttonClear = new System.Windows.Forms.Button();
			this.checkBoxErrors = new System.Windows.Forms.CheckBox();
			this.labelSeparator = new System.Windows.Forms.Label();
			this.checkBoxWarnings = new System.Windows.Forms.CheckBox();
			this.labelSeparator2 = new System.Windows.Forms.Label();
			this.checkBoxInfos = new System.Windows.Forms.CheckBox();
			this.labelSeparator3 = new System.Windows.Forms.Label();
			this.checkBoxDebug = new System.Windows.Forms.CheckBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.filterTextBox = new System.Windows.Forms.TextBox();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
			this.label2 = new System.Windows.Forms.Label();
			this.includeTagsTextBox = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.excludeTagsTextBox = new System.Windows.Forms.TextBox();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
			this.flowLayoutPanel1.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.flowLayoutPanel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// dataGridView1
			// 
			this.dataGridView1.AllowUserToAddRows = false;
			this.dataGridView1.AllowUserToDeleteRows = false;
			this.dataGridView1.AllowUserToResizeRows = false;
			dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
			this.dataGridView1.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
			this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.dataGridView1.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
			this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
			this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.columnType,
            this.columnOrdinal,
            this.columnTag,
            this.columnDescription,
            this.columnFile,
            this.columnLine});
			this.dataGridView1.Location = new System.Drawing.Point(3, 70);
			this.dataGridView1.Name = "dataGridView1";
			this.dataGridView1.ReadOnly = true;
			this.dataGridView1.RowHeadersVisible = false;
			this.dataGridView1.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
			this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dataGridView1.Size = new System.Drawing.Size(1038, 592);
			this.dataGridView1.TabIndex = 0;
			this.dataGridView1.TabStop = false;
			// 
			// columnType
			// 
			this.columnType.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
			this.columnType.DataPropertyName = "Type";
			this.columnType.HeaderText = "";
			this.columnType.MinimumWidth = 22;
			this.columnType.Name = "columnType";
			this.columnType.ReadOnly = true;
			this.columnType.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.columnType.Width = 22;
			// 
			// columnOrdinal
			// 
			this.columnOrdinal.DataPropertyName = "Ordinal";
			this.columnOrdinal.HeaderText = "";
			this.columnOrdinal.MinimumWidth = 35;
			this.columnOrdinal.Name = "columnOrdinal";
			this.columnOrdinal.ReadOnly = true;
			this.columnOrdinal.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.columnOrdinal.Width = 35;
			// 
			// columnTag
			// 
			this.columnTag.DataPropertyName = "Tag";
			this.columnTag.HeaderText = "Tag";
			this.columnTag.MinimumWidth = 20;
			this.columnTag.Name = "columnTag";
			this.columnTag.ReadOnly = true;
			// 
			// columnDescription
			// 
			this.columnDescription.DataPropertyName = "Description";
			dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.columnDescription.DefaultCellStyle = dataGridViewCellStyle2;
			this.columnDescription.HeaderText = "Description";
			this.columnDescription.MinimumWidth = 50;
			this.columnDescription.Name = "columnDescription";
			this.columnDescription.ReadOnly = true;
			this.columnDescription.Width = 510;
			// 
			// columnFile
			// 
			this.columnFile.DataPropertyName = "File";
			this.columnFile.HeaderText = "File";
			this.columnFile.MinimumWidth = 20;
			this.columnFile.Name = "columnFile";
			this.columnFile.ReadOnly = true;
			this.columnFile.Width = 150;
			// 
			// columnLine
			// 
			this.columnLine.DataPropertyName = "Line";
			this.columnLine.HeaderText = "Line";
			this.columnLine.MinimumWidth = 40;
			this.columnLine.Name = "columnLine";
			this.columnLine.ReadOnly = true;
			this.columnLine.Width = 40;
			// 
			// flowLayoutPanel1
			// 
			this.flowLayoutPanel1.AutoSize = true;
			this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.flowLayoutPanel1.Controls.Add(this.buttonClear);
			this.flowLayoutPanel1.Controls.Add(this.checkBoxErrors);
			this.flowLayoutPanel1.Controls.Add(this.labelSeparator);
			this.flowLayoutPanel1.Controls.Add(this.checkBoxWarnings);
			this.flowLayoutPanel1.Controls.Add(this.labelSeparator2);
			this.flowLayoutPanel1.Controls.Add(this.checkBoxInfos);
			this.flowLayoutPanel1.Controls.Add(this.labelSeparator3);
			this.flowLayoutPanel1.Controls.Add(this.checkBoxDebug);
			this.flowLayoutPanel1.Controls.Add(this.label3);
			this.flowLayoutPanel1.Controls.Add(this.label1);
			this.flowLayoutPanel1.Controls.Add(this.filterTextBox);
			this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 3);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new System.Drawing.Size(1038, 29);
			this.flowLayoutPanel1.TabIndex = 1;
			// 
			// buttonClear
			// 
			this.buttonClear.Location = new System.Drawing.Point(3, 3);
			this.buttonClear.Name = "buttonClear";
			this.buttonClear.Size = new System.Drawing.Size(75, 23);
			this.buttonClear.TabIndex = 8;
			this.buttonClear.Text = "Clear";
			this.buttonClear.UseVisualStyleBackColor = true;
			this.buttonClear.Click += new System.EventHandler(this.buttonClear_Click);
			// 
			// checkBoxErrors
			// 
			this.checkBoxErrors.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.checkBoxErrors.AutoSize = true;
			this.checkBoxErrors.Checked = true;
			this.checkBoxErrors.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxErrors.Location = new System.Drawing.Point(84, 7);
			this.checkBoxErrors.MinimumSize = new System.Drawing.Size(50, 0);
			this.checkBoxErrors.Name = "checkBoxErrors";
			this.checkBoxErrors.Size = new System.Drawing.Size(50, 14);
			this.checkBoxErrors.TabIndex = 0;
			this.checkBoxErrors.UseVisualStyleBackColor = true;
			// 
			// labelSeparator
			// 
			this.labelSeparator.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.labelSeparator.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.labelSeparator.Enabled = false;
			this.labelSeparator.Location = new System.Drawing.Point(140, 4);
			this.labelSeparator.Name = "labelSeparator";
			this.labelSeparator.Size = new System.Drawing.Size(2, 20);
			this.labelSeparator.TabIndex = 5;
			// 
			// checkBoxWarnings
			// 
			this.checkBoxWarnings.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.checkBoxWarnings.AutoSize = true;
			this.checkBoxWarnings.Checked = true;
			this.checkBoxWarnings.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxWarnings.Location = new System.Drawing.Point(148, 7);
			this.checkBoxWarnings.MinimumSize = new System.Drawing.Size(50, 0);
			this.checkBoxWarnings.Name = "checkBoxWarnings";
			this.checkBoxWarnings.Size = new System.Drawing.Size(50, 14);
			this.checkBoxWarnings.TabIndex = 1;
			this.checkBoxWarnings.UseVisualStyleBackColor = true;
			// 
			// labelSeparator2
			// 
			this.labelSeparator2.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.labelSeparator2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.labelSeparator2.Enabled = false;
			this.labelSeparator2.Location = new System.Drawing.Point(204, 4);
			this.labelSeparator2.Name = "labelSeparator2";
			this.labelSeparator2.Size = new System.Drawing.Size(2, 20);
			this.labelSeparator2.TabIndex = 6;
			// 
			// checkBoxInfos
			// 
			this.checkBoxInfos.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.checkBoxInfos.AutoSize = true;
			this.checkBoxInfos.Checked = true;
			this.checkBoxInfos.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxInfos.Location = new System.Drawing.Point(212, 7);
			this.checkBoxInfos.MinimumSize = new System.Drawing.Size(50, 0);
			this.checkBoxInfos.Name = "checkBoxInfos";
			this.checkBoxInfos.Size = new System.Drawing.Size(50, 14);
			this.checkBoxInfos.TabIndex = 2;
			this.checkBoxInfos.UseVisualStyleBackColor = true;
			// 
			// labelSeparator3
			// 
			this.labelSeparator3.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.labelSeparator3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.labelSeparator3.Enabled = false;
			this.labelSeparator3.Location = new System.Drawing.Point(268, 4);
			this.labelSeparator3.Name = "labelSeparator3";
			this.labelSeparator3.Size = new System.Drawing.Size(2, 20);
			this.labelSeparator3.TabIndex = 7;
			// 
			// checkBoxDebug
			// 
			this.checkBoxDebug.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.checkBoxDebug.AutoSize = true;
			this.checkBoxDebug.Location = new System.Drawing.Point(276, 7);
			this.checkBoxDebug.MinimumSize = new System.Drawing.Size(50, 0);
			this.checkBoxDebug.Name = "checkBoxDebug";
			this.checkBoxDebug.Size = new System.Drawing.Size(50, 14);
			this.checkBoxDebug.TabIndex = 12;
			this.checkBoxDebug.UseVisualStyleBackColor = true;
			// 
			// label3
			// 
			this.label3.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.label3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.label3.Enabled = false;
			this.label3.Location = new System.Drawing.Point(332, 4);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(2, 20);
			this.label3.TabIndex = 13;
			// 
			// label1
			// 
			this.label1.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(340, 8);
			this.label1.Margin = new System.Windows.Forms.Padding(3);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(29, 13);
			this.label1.TabIndex = 3;
			this.label1.Text = "Filter";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// filterTextBox
			// 
			this.filterTextBox.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.filterTextBox.Location = new System.Drawing.Point(375, 4);
			this.filterTextBox.Name = "filterTextBox";
			this.filterTextBox.Size = new System.Drawing.Size(270, 20);
			this.filterTextBox.TabIndex = 4;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.AutoSize = true;
			this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.dataGridView1, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel2, 0, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(1044, 665);
			this.tableLayoutPanel1.TabIndex = 2;
			// 
			// flowLayoutPanel2
			// 
			this.flowLayoutPanel2.AutoSize = true;
			this.flowLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.flowLayoutPanel2.Controls.Add(this.label2);
			this.flowLayoutPanel2.Controls.Add(this.includeTagsTextBox);
			this.flowLayoutPanel2.Controls.Add(this.label5);
			this.flowLayoutPanel2.Controls.Add(this.excludeTagsTextBox);
			this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flowLayoutPanel2.Location = new System.Drawing.Point(3, 38);
			this.flowLayoutPanel2.Name = "flowLayoutPanel2";
			this.flowLayoutPanel2.Size = new System.Drawing.Size(1038, 26);
			this.flowLayoutPanel2.TabIndex = 2;
			// 
			// label2
			// 
			this.label2.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(3, 6);
			this.label2.Margin = new System.Windows.Forms.Padding(3);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(66, 13);
			this.label2.TabIndex = 17;
			this.label2.Text = "IncludeTags";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// includeTagsTextBox
			// 
			this.includeTagsTextBox.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.includeTagsTextBox.Location = new System.Drawing.Point(75, 3);
			this.includeTagsTextBox.Name = "includeTagsTextBox";
			this.includeTagsTextBox.Size = new System.Drawing.Size(300, 20);
			this.includeTagsTextBox.TabIndex = 18;
			// 
			// label5
			// 
			this.label5.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(381, 6);
			this.label5.Margin = new System.Windows.Forms.Padding(3);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(69, 13);
			this.label5.TabIndex = 20;
			this.label5.Text = "ExcludeTags";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// excludeTagsTextBox
			// 
			this.excludeTagsTextBox.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.excludeTagsTextBox.Location = new System.Drawing.Point(456, 3);
			this.excludeTagsTextBox.Name = "excludeTagsTextBox";
			this.excludeTagsTextBox.Size = new System.Drawing.Size(300, 20);
			this.excludeTagsTextBox.TabIndex = 21;
			// 
			// picoLogOutputForm3
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "picoLogOutputForm3";
			this.Size = new System.Drawing.Size(1044, 665);
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
			this.flowLayoutPanel1.ResumeLayout(false);
			this.flowLayoutPanel1.PerformLayout();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.flowLayoutPanel2.ResumeLayout(false);
			this.flowLayoutPanel2.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.DataGridView dataGridView1;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		private System.Windows.Forms.CheckBox checkBoxErrors;
		private System.Windows.Forms.CheckBox checkBoxWarnings;
		private System.Windows.Forms.CheckBox checkBoxInfos;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox filterTextBox;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label labelSeparator;
		private System.Windows.Forms.Label labelSeparator2;
		private System.Windows.Forms.Label labelSeparator3;
		private System.Windows.Forms.Button buttonClear;
		private System.Windows.Forms.DataGridViewImageColumn columnType;
		private System.Windows.Forms.DataGridViewTextBoxColumn columnOrdinal;
		private System.Windows.Forms.DataGridViewTextBoxColumn columnTag;
		private System.Windows.Forms.DataGridViewTextBoxColumn columnDescription;
		private System.Windows.Forms.DataGridViewTextBoxColumn columnFile;
		private System.Windows.Forms.DataGridViewTextBoxColumn columnLine;
		private System.Windows.Forms.CheckBox checkBoxDebug;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox includeTagsTextBox;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox excludeTagsTextBox;
	}
}
