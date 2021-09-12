using System;
using System.Drawing;
using System.Windows.Forms;

namespace CSPBrushInfo {
    public partial class ScrolledRichTextDialog : Form {
        private static FindDialog findDlg;

        public ScrolledRichTextDialog(Size size, string text) {
            InitializeComponent();

            // Resize the Form
            if (size != null) {
                this.Size = size;
            }
            if (!String.IsNullOrEmpty(text)) {
                this.textBox.Text = text;
            }
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e) {
            // Just hide rather than close if the user did it
            if (e.CloseReason == CloseReason.UserClosing) {
                e.Cancel = true;
                Visible = false;
            }
        }

        private void OnButtonOkClick(object sender, EventArgs e) {
            this.Visible = false;
        }

        private void OnSaveRtfClick(object sender, EventArgs e) {
            if (textBox == null) {
                return;
            }
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "RTF Files|*.rtf";
            dlg.Title = "Save as RTF";
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                try {
                    textBox.SaveFile(dlg.FileName,
                        RichTextBoxStreamType.RichText);
                } catch (Exception ex) {
                    Utils.Utils.excMsg("Error saving RTF", ex);
                }
            }
        }

        private void OnFindClick(object sender, EventArgs e) {
            if (textBox == null) {
                return;
            }
            if (findDlg == null) {
                findDlg = new FindDialog(textBox);
                findDlg.Text = "FindDialog in Tool Hierachy";
                // Keep it on top
                findDlg.Owner = this;
                findDlg.Show();
            } else {
                findDlg.Visible = true;
            }
        }

        // RichTextBox context menu
        private void OnCutClick(object sender, EventArgs e) {
            textBox.Cut();
        }

        private void OnCopyClick(object sender, EventArgs e) {
            textBox.Copy();
        }

        private void OnPasteClick(object sender, EventArgs e) {
            textBox.Paste();
        }

        private void OnSelectAllClick(object sender, EventArgs e) {
            textBox.SelectAll();
        }
    }


}
