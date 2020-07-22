namespace CSPBrushInfo {
    partial class MultiChoiceListDialog {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MultiChoiceListDialog));
            this.tableLayoutPanelTop = new System.Windows.Forms.TableLayoutPanel();
            this.labelMsg = new System.Windows.Forms.Label();
            this.listBoxFiles = new System.Windows.Forms.ListBox();
            this.flowLayoutPanelButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOk = new System.Windows.Forms.Button();
            this.tableLayoutPanelTop.SuspendLayout();
            this.flowLayoutPanelButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanelTop
            // 
            this.tableLayoutPanelTop.AutoSize = true;
            this.tableLayoutPanelTop.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanelTop.ColumnCount = 1;
            this.tableLayoutPanelTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelTop.Controls.Add(this.labelMsg, 0, 0);
            this.tableLayoutPanelTop.Controls.Add(this.listBoxFiles, 0, 1);
            this.tableLayoutPanelTop.Controls.Add(this.flowLayoutPanelButtons, 0, 2);
            this.tableLayoutPanelTop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelTop.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanelTop.Name = "tableLayoutPanelTop";
            this.tableLayoutPanelTop.RowCount = 2;
            this.tableLayoutPanelTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelTop.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelTop.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelTop.Size = new System.Drawing.Size(1041, 450);
            this.tableLayoutPanelTop.TabIndex = 1;
            // 
            // labelMsg
            // 
            this.labelMsg.AutoSize = true;
            this.labelMsg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelMsg.Location = new System.Drawing.Point(3, 0);
            this.labelMsg.Name = "labelMsg";
            this.labelMsg.Padding = new System.Windows.Forms.Padding(25);
            this.labelMsg.Size = new System.Drawing.Size(1035, 114);
            this.labelMsg.TabIndex = 3;
            this.labelMsg.Text = "There is more than one brush with the same name.\r\nSelect the one to use.";
            // 
            // listBoxFiles
            // 
            this.listBoxFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxFiles.FormattingEnabled = true;
            this.listBoxFiles.ItemHeight = 31;
            this.listBoxFiles.Location = new System.Drawing.Point(3, 117);
            this.listBoxFiles.Name = "listBoxFiles";
            this.listBoxFiles.Size = new System.Drawing.Size(1035, 276);
            this.listBoxFiles.TabIndex = 2;
            // 
            // flowLayoutPanelButtons
            // 
            this.flowLayoutPanelButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.flowLayoutPanelButtons.AutoSize = true;
            this.flowLayoutPanelButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanelButtons.BackColor = System.Drawing.SystemColors.Control;
            this.flowLayoutPanelButtons.Controls.Add(this.buttonCancel);
            this.flowLayoutPanelButtons.Controls.Add(this.buttonOk);
            this.flowLayoutPanelButtons.Location = new System.Drawing.Point(424, 399);
            this.flowLayoutPanelButtons.Name = "flowLayoutPanelButtons";
            this.flowLayoutPanelButtons.Size = new System.Drawing.Size(192, 48);
            this.flowLayoutPanelButtons.TabIndex = 0;
            this.flowLayoutPanelButtons.WrapContents = false;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.buttonCancel.AutoSize = true;
            this.buttonCancel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.buttonCancel.Location = new System.Drawing.Point(3, 3);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(114, 42);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.onCancelClick);
            // 
            // buttonOk
            // 
            this.buttonOk.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.buttonOk.AutoSize = true;
            this.buttonOk.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.buttonOk.Location = new System.Drawing.Point(123, 3);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(66, 42);
            this.buttonOk.TabIndex = 0;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.onOkClick);
            // 
            // MultiChoiceListDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1041, 450);
            this.Controls.Add(this.tableLayoutPanelTop);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MultiChoiceListDialog";
            this.Text = "MultiChoiceListDialog";
            this.tableLayoutPanelTop.ResumeLayout(false);
            this.tableLayoutPanelTop.PerformLayout();
            this.flowLayoutPanelButtons.ResumeLayout(false);
            this.flowLayoutPanelButtons.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelTop;
        private System.Windows.Forms.ListBox listBoxFiles;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelButtons;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Label labelMsg;
    }
}