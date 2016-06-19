namespace pico.LogOutput
{
	partial class picoLogOutputForm2
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
			BrightIdeasSoftware.HeaderStateStyle headerStateStyle1 = new BrightIdeasSoftware.HeaderStateStyle();
			BrightIdeasSoftware.HeaderStateStyle headerStateStyle2 = new BrightIdeasSoftware.HeaderStateStyle();
			BrightIdeasSoftware.HeaderStateStyle headerStateStyle3 = new BrightIdeasSoftware.HeaderStateStyle();
			this.highlightTextRenderer1 = new BrightIdeasSoftware.HighlightTextRenderer();
			this.headerFormatStyleData = new BrightIdeasSoftware.HeaderFormatStyle();
			this.olvColumn1 = ((BrightIdeasSoftware.OLVColumn) (new BrightIdeasSoftware.OLVColumn()));
			this.olvColumn2 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
			this.olvData = new BrightIdeasSoftware.DataListView();
			((System.ComponentModel.ISupportInitialize)(this.olvData)).BeginInit();
			this.SuspendLayout();
			// 
			// headerFormatStyleData
			// 
			headerStateStyle1.BackColor = System.Drawing.Color.FromArgb( ((int) (((byte) (32)))), ((int) (((byte) (32)))), ((int) (((byte) (32)))) );
			headerStateStyle1.ForeColor = System.Drawing.Color.White;
			this.headerFormatStyleData.Hot = headerStateStyle1;
			headerStateStyle2.BackColor = System.Drawing.Color.Black;
			headerStateStyle2.ForeColor = System.Drawing.Color.Gainsboro;
			this.headerFormatStyleData.Normal = headerStateStyle2;
			headerStateStyle3.BackColor = System.Drawing.Color.FromArgb( ((int) (((byte) (64)))), ((int) (((byte) (64)))), ((int) (((byte) (64)))) );
			headerStateStyle3.ForeColor = System.Drawing.Color.White;
			headerStateStyle3.FrameColor = System.Drawing.Color.WhiteSmoke;
			headerStateStyle3.FrameWidth = 2F;
			this.headerFormatStyleData.Pressed = headerStateStyle3;
			// 
			// highlightTextRenderer1
			// 
			this.highlightTextRenderer1.CanWrap = true;
			this.highlightTextRenderer1.UseGdiTextRendering = false;
			// 
			// olvColumn1
			// 
			this.olvColumn1.AspectName = "Name";
			this.olvColumn1.IsTileViewColumn = true;
			//this.olvColumn1.Renderer = this.highlightTextRenderer1;
			this.olvColumn1.Text = "Name";
			this.olvColumn1.UseInitialLetterForGroup = true;
			this.olvColumn1.Width = 112;
			this.olvColumn1.WordWrap = true;
			// 
			// olvColumn2
			// 
			this.olvColumn2.AspectName = "Value";
			this.olvColumn2.IsTileViewColumn = true;
			this.olvColumn2.Text = "Value";
			this.olvColumn2.Width = 73;
			// 
			// olvData
			// 
			this.olvData.AllColumns.Add(this.olvColumn1);
			this.olvData.AllColumns.Add(this.olvColumn2);
			this.olvData.AllowColumnReorder = true;
			this.olvData.AllowDrop = true;
			this.olvData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.olvData.CellEditActivation = BrightIdeasSoftware.ObjectListView.CellEditActivateMode.SingleClick;
			this.olvData.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvColumn1,
            this.olvColumn2});
			this.olvData.Cursor = System.Windows.Forms.Cursors.Default;
			this.olvData.DataSource = null;
			this.olvData.EmptyListMsg = "Add rows to the above table to see them here";
			this.olvData.EmptyListMsgFont = new System.Drawing.Font("Comic Sans MS", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.olvData.FullRowSelect = true;
			this.olvData.GridLines = true;
			this.olvData.GroupWithItemCountFormat = "{0} ({1} people)";
			this.olvData.GroupWithItemCountSingularFormat = "{0} (1 person)";
			this.olvData.HeaderFormatStyle = this.headerFormatStyleData;
			this.olvData.HideSelection = false;
			this.olvData.HighlightBackgroundColor = System.Drawing.Color.Crimson;
			this.olvData.HighlightForegroundColor = System.Drawing.Color.DarkGreen;
			this.olvData.Location = new System.Drawing.Point(6, 19);
			this.olvData.Name = "olvData";
			this.olvData.OwnerDraw = true;
			this.olvData.SelectColumnsOnRightClickBehaviour = BrightIdeasSoftware.ObjectListView.ColumnSelectBehaviour.Submenu;
			this.olvData.ShowCommandMenuOnRightClick = true;
			this.olvData.ShowGroups = false;
			this.olvData.ShowImagesOnSubItems = true;
			this.olvData.ShowItemToolTips = true;
			this.olvData.Size = new System.Drawing.Size(526, 231);
			this.olvData.TabIndex = 0;
			this.olvData.UseCellFormatEvents = true;
			this.olvData.UseCompatibleStateImageBehavior = false;
			this.olvData.UseFilterIndicator = true;
			this.olvData.UseFiltering = true;
			this.olvData.UseHotItem = true;
			this.olvData.UseTranslucentHotItem = true;
			this.olvData.View = System.Windows.Forms.View.Details;
			this.olvData.FormatCell += new System.EventHandler<BrightIdeasSoftware.FormatCellEventArgs>( this.listViewDataSet_FormatCell );
			// 
			// picoLogOutputForm2
			// 
			//this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			//this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(544, 262);
			this.Controls.Add(this.olvData);
			this.Name = "picoLogOutputForm2";
			this.Text = "picoLogOutputForm";
			((System.ComponentModel.ISupportInitialize)(this.olvData)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private BrightIdeasSoftware.HighlightTextRenderer highlightTextRenderer1;
		private BrightIdeasSoftware.HeaderFormatStyle headerFormatStyleData;
		private BrightIdeasSoftware.OLVColumn olvColumn1;
		private BrightIdeasSoftware.OLVColumn olvColumn2;
		private BrightIdeasSoftware.DataListView olvData;
	}
}