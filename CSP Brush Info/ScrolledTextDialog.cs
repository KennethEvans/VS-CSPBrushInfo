using System;
using System.Drawing;
using System.Windows.Forms;

namespace CSPBrushInfo {
    public partial class ScrolledTextDialog : Form {
        public ScrolledTextDialog(Size size, string text) {
            InitializeComponent();

            // Resize the Form
            if (size != null) {
                this.Size = size;
            }
            if(!String.IsNullOrEmpty(text)) {
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

        private void OnButtonCancelClick(object sender, EventArgs e) {
            this.Visible = false;
        }

    }
}
