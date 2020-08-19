namespace CSPBrushInfo {
    partial class BrushesInDatabaseDialog {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BrushesInDatabaseDialog));
            this.tableLayoutPanelTop = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanelDatabase = new System.Windows.Forms.TableLayoutPanel();
            this.labelDatabase = new System.Windows.Forms.Label();
            this.textBoxDatabase = new System.Windows.Forms.TextBox();
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.tableLayoutPanelFilter = new System.Windows.Forms.TableLayoutPanel();
            this.labelFilter = new System.Windows.Forms.Label();
            this.textBoxFilter = new System.Windows.Forms.TextBox();
            this.checkBoxCaseSensitive = new System.Windows.Forms.CheckBox();
            this.listBoxBrushes = new System.Windows.Forms.ListBox();
            this.flowLayoutPanelButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.buttonFind = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOk = new System.Windows.Forms.Button();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.tableLayoutPanelTop.SuspendLayout();
            this.tableLayoutPanelDatabase.SuspendLayout();
            this.tableLayoutPanelFilter.SuspendLayout();
            this.flowLayoutPanelButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanelTop
            // 
            this.tableLayoutPanelTop.AutoSize = true;
            this.tableLayoutPanelTop.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanelTop.ColumnCount = 1;
            this.tableLayoutPanelTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelTop.Controls.Add(this.tableLayoutPanelDatabase, 0, 0);
            this.tableLayoutPanelTop.Controls.Add(this.tableLayoutPanelFilter, 0, 1);
            this.tableLayoutPanelTop.Controls.Add(this.listBoxBrushes, 0, 2);
            this.tableLayoutPanelTop.Controls.Add(this.flowLayoutPanelButtons, 0, 3);
            this.tableLayoutPanelTop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelTop.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanelTop.Name = "tableLayoutPanelTop";
            this.tableLayoutPanelTop.RowCount = 4;
            this.tableLayoutPanelTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelTop.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelTop.Size = new System.Drawing.Size(1062, 450);
            this.tableLayoutPanelTop.TabIndex = 0;
            // 
            // tableLayoutPanelDatabase
            // 
            this.tableLayoutPanelDatabase.AutoSize = true;
            this.tableLayoutPanelDatabase.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanelDatabase.BackColor = System.Drawing.SystemColors.Control;
            this.tableLayoutPanelDatabase.ColumnCount = 3;
            this.tableLayoutPanelDatabase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelDatabase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelDatabase.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelDatabase.Controls.Add(this.labelDatabase, 0, 0);
            this.tableLayoutPanelDatabase.Controls.Add(this.textBoxDatabase, 1, 0);
            this.tableLayoutPanelDatabase.Controls.Add(this.buttonBrowse, 2, 0);
            this.tableLayoutPanelDatabase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelDatabase.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanelDatabase.Name = "tableLayoutPanelDatabase";
            this.tableLayoutPanelDatabase.RowCount = 1;
            this.tableLayoutPanelDatabase.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelDatabase.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelDatabase.Size = new System.Drawing.Size(1056, 48);
            this.tableLayoutPanelDatabase.TabIndex = 2;
            // 
            // labelDatabase
            // 
            this.labelDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.labelDatabase.AutoSize = true;
            this.labelDatabase.BackColor = System.Drawing.SystemColors.Control;
            this.labelDatabase.Location = new System.Drawing.Point(3, 0);
            this.labelDatabase.Name = "labelDatabase";
            this.labelDatabase.Size = new System.Drawing.Size(145, 48);
            this.labelDatabase.TabIndex = 0;
            this.labelDatabase.Text = "Database:";
            this.labelDatabase.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.toolTip.SetToolTip(this.labelDatabase, "Specify database or SUT file.");
            // 
            // textBoxDatabase
            // 
            this.textBoxDatabase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxDatabase.Location = new System.Drawing.Point(154, 3);
            this.textBoxDatabase.Name = "textBoxDatabase";
            this.textBoxDatabase.Size = new System.Drawing.Size(774, 38);
            this.textBoxDatabase.TabIndex = 1;
            this.toolTip.SetToolTip(this.textBoxDatabase, "Specify database or SUT file.");
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.buttonBrowse.AutoSize = true;
            this.buttonBrowse.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.buttonBrowse.BackColor = System.Drawing.SystemColors.Control;
            this.buttonBrowse.Location = new System.Drawing.Point(934, 3);
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Size = new System.Drawing.Size(119, 42);
            this.buttonBrowse.TabIndex = 2;
            this.buttonBrowse.Text = "Browse";
            this.toolTip.SetToolTip(this.buttonBrowse, "Browse for a database or SUT file.");
            this.buttonBrowse.UseVisualStyleBackColor = false;
            // 
            // tableLayoutPanelFilter
            // 
            this.tableLayoutPanelFilter.AutoSize = true;
            this.tableLayoutPanelFilter.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanelFilter.BackColor = System.Drawing.SystemColors.Control;
            this.tableLayoutPanelFilter.ColumnCount = 3;
            this.tableLayoutPanelFilter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelFilter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelFilter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelFilter.Controls.Add(this.labelFilter, 0, 1);
            this.tableLayoutPanelFilter.Controls.Add(this.textBoxFilter, 1, 1);
            this.tableLayoutPanelFilter.Controls.Add(this.checkBoxCaseSensitive, 2, 1);
            this.tableLayoutPanelFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelFilter.Location = new System.Drawing.Point(3, 57);
            this.tableLayoutPanelFilter.Name = "tableLayoutPanelFilter";
            this.tableLayoutPanelFilter.RowCount = 1;
            this.tableLayoutPanelFilter.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelFilter.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelFilter.Size = new System.Drawing.Size(1056, 44);
            this.tableLayoutPanelFilter.TabIndex = 0;
            this.toolTip.SetToolTip(this.tableLayoutPanelFilter, "Check if filter is case senstive.");
            // 
            // labelFilter
            // 
            this.labelFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.labelFilter.AutoSize = true;
            this.labelFilter.BackColor = System.Drawing.SystemColors.Control;
            this.labelFilter.Location = new System.Drawing.Point(3, 0);
            this.labelFilter.Name = "labelFilter";
            this.labelFilter.Size = new System.Drawing.Size(87, 44);
            this.labelFilter.TabIndex = 0;
            this.labelFilter.Text = "Filter:";
            this.labelFilter.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.toolTip.SetToolTip(this.labelFilter, "Filter the brushes.\r\nCR to execute.\r\nClear text, then CR to stop filtering.");
            // 
            // textBoxFilter
            // 
            this.textBoxFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxFilter.Location = new System.Drawing.Point(96, 3);
            this.textBoxFilter.Name = "textBoxFilter";
            this.textBoxFilter.Size = new System.Drawing.Size(708, 38);
            this.textBoxFilter.TabIndex = 1;
            this.toolTip.SetToolTip(this.textBoxFilter, "Filter the brushes.\r\nCR to execute.\r\nClear text, then CR to stop filtering.");
            this.textBoxFilter.KeyDown += new System.Windows.Forms.KeyEventHandler(this.onTextBoxFilterKeyDown);
            // 
            // checkBoxCaseSensitive
            // 
            this.checkBoxCaseSensitive.AutoSize = true;
            this.checkBoxCaseSensitive.Location = new System.Drawing.Point(810, 3);
            this.checkBoxCaseSensitive.Name = "checkBoxCaseSensitive";
            this.checkBoxCaseSensitive.Size = new System.Drawing.Size(243, 36);
            this.checkBoxCaseSensitive.TabIndex = 2;
            this.checkBoxCaseSensitive.Text = "Case Sensitive";
            this.toolTip.SetToolTip(this.checkBoxCaseSensitive, "Check to make filter case sensitive.");
            this.checkBoxCaseSensitive.UseVisualStyleBackColor = true;
            // 
            // listBoxBrushes
            // 
            this.listBoxBrushes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxBrushes.FormattingEnabled = true;
            this.listBoxBrushes.ItemHeight = 31;
            this.listBoxBrushes.Location = new System.Drawing.Point(3, 107);
            this.listBoxBrushes.Name = "listBoxBrushes";
            this.listBoxBrushes.Size = new System.Drawing.Size(1056, 286);
            this.listBoxBrushes.TabIndex = 1;
            this.toolTip.SetToolTip(this.listBoxBrushes, "Available brush names.");
            // 
            // flowLayoutPanelButtons
            // 
            this.flowLayoutPanelButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.flowLayoutPanelButtons.AutoSize = true;
            this.flowLayoutPanelButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanelButtons.BackColor = System.Drawing.SystemColors.Control;
            this.flowLayoutPanelButtons.Controls.Add(this.buttonFind);
            this.flowLayoutPanelButtons.Controls.Add(this.buttonCancel);
            this.flowLayoutPanelButtons.Controls.Add(this.buttonOk);
            this.flowLayoutPanelButtons.Location = new System.Drawing.Point(336, 399);
            this.flowLayoutPanelButtons.Name = "flowLayoutPanelButtons";
            this.flowLayoutPanelButtons.Size = new System.Drawing.Size(390, 48);
            this.flowLayoutPanelButtons.TabIndex = 0;
            this.flowLayoutPanelButtons.WrapContents = false;
            // 
            // buttonFind
            // 
            this.buttonFind.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.buttonFind.AutoSize = true;
            this.buttonFind.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.buttonFind.Location = new System.Drawing.Point(3, 3);
            this.buttonFind.Name = "buttonFind";
            this.buttonFind.Size = new System.Drawing.Size(192, 42);
            this.buttonFind.TabIndex = 2;
            this.buttonFind.Text = "Find Brushes";
            this.toolTip.SetToolTip(this.buttonFind, "Get or refresh the brushes in the database.");
            this.buttonFind.UseVisualStyleBackColor = true;
            this.buttonFind.Click += new System.EventHandler(this.OnFindClick);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.buttonCancel.AutoSize = true;
            this.buttonCancel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.buttonCancel.Location = new System.Drawing.Point(201, 3);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(114, 42);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "Cancel";
            this.toolTip.SetToolTip(this.buttonCancel, "Cancel without saving.");
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.OnCancelClick);
            // 
            // buttonOk
            // 
            this.buttonOk.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.buttonOk.AutoSize = true;
            this.buttonOk.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.buttonOk.Location = new System.Drawing.Point(321, 3);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(66, 42);
            this.buttonOk.TabIndex = 0;
            this.buttonOk.Text = "OK";
            this.toolTip.SetToolTip(this.buttonOk, "Quit and save the selected value.");
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.OnOkClick);
            // 
            // BrushesInDatabaseDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1062, 450);
            this.Controls.Add(this.tableLayoutPanelTop);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "BrushesInDatabaseDialog";
            this.Text = "Brushes in Database";
            this.tableLayoutPanelTop.ResumeLayout(false);
            this.tableLayoutPanelTop.PerformLayout();
            this.tableLayoutPanelDatabase.ResumeLayout(false);
            this.tableLayoutPanelDatabase.PerformLayout();
            this.tableLayoutPanelFilter.ResumeLayout(false);
            this.tableLayoutPanelFilter.PerformLayout();
            this.flowLayoutPanelButtons.ResumeLayout(false);
            this.flowLayoutPanelButtons.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelTop;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelFilter;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelButtons;
        private System.Windows.Forms.Button buttonFind;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.ListBox listBoxBrushes;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.Label labelFilter;
        private System.Windows.Forms.TextBox textBoxFilter;
        private System.Windows.Forms.CheckBox checkBoxCaseSensitive;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelDatabase;
        private System.Windows.Forms.Label labelDatabase;
        private System.Windows.Forms.TextBox textBoxDatabase;
        private System.Windows.Forms.Button buttonBrowse;
    }
}