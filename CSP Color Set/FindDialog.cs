using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSPUtils {
    public partial class FindDialog : Form {
        private RichTextBox richTextBox;
        private string lastSelectedText = "";
        public FindDialog(RichTextBox richTextBox) {
            InitializeComponent();
            this.richTextBox = richTextBox;
            textBoxSearchString.Text = lastSelectedText;
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e) {
            // Just hide rather than close if the user did it
            if (e.CloseReason == CloseReason.UserClosing) {
                e.Cancel = true;
                Visible = false;
            }
        }

        private void OnFindNextClick(object sender, EventArgs e) {
            int startIndex, endIndex;
            int foundIndex;
            RichTextBoxFinds options = RichTextBoxFinds.None;
            if (checkBoxCaseSensitive.Checked) options |= RichTextBoxFinds.MatchCase;
            if (checkBoxWholeWord.Checked) options |= RichTextBoxFinds.WholeWord;
            if (checkBoxReverse.Checked) {
                // Reverse
                options |= RichTextBoxFinds.Reverse;
                startIndex = 0;
                endIndex = richTextBox.SelectionStart;
            } else {
                // Forward
                startIndex = this.richTextBox.SelectionStart + this.richTextBox.SelectionLength;
                endIndex = this.richTextBox.Text.Length;
            }
            richTextBox.Focus();
            foundIndex = richTextBox.Find(textBoxSearchString.Text, startIndex,
                endIndex, options);
            if (foundIndex < 0) {
                // Nothing found
                SystemSounds.Beep.Play();
            }
        }

        private void OnFindAllClick(object sender, EventArgs e) {
            int index = 0, nFound = 0;
            int foundIndex;
            richTextBox.SelectionStart = index;
            richTextBox.SelectionLength = 0;
            RichTextBoxFinds options = RichTextBoxFinds.None;
            if (checkBoxCaseSensitive.Checked) options |= RichTextBoxFinds.MatchCase;
            if (checkBoxWholeWord.Checked) options |= RichTextBoxFinds.WholeWord;
            // Dont handle reverse
#if false
            richTextBox.Focus();
#endif
            while (index > -1) {
                // Searches the text in a RichTextBox control for a string within
                // a range of text within the control and with specific options
                // applied to the search.
                foundIndex = richTextBox.Find(textBoxSearchString.Text, index,
                    richTextBox.TextLength, options);
                if (foundIndex < 0) break;
                nFound++;
                // Set the SelectionBackColor, which remains until changed
                richTextBox.SelectionBackColor = Color.Yellow;
                // After a match is found the index is increased so the search
                // won't stop at the same match again. This makes possible to
                // highlight same words at the same time.
                index = foundIndex + 1;
            }
            if (nFound == 0) {
                // Nothing found
                SystemSounds.Beep.Play();
            }
#if false
            // Set the selection back to the first found
            richTextBox.Focus();
            richTextBox.Find(textBoxSearchString.Text, 0, richTextBox.TextLength, RichTextBoxFinds.None);
#endif
        }

        private void OnCancelClick(object sender, EventArgs e) {
            this.DialogResult = DialogResult.Cancel;
            lastSelectedText = textBoxSearchString.Text;
            Visible = false;
        }

        private void OnResetClick(object sender, EventArgs e) {
            richTextBox.SelectionBackColor = SystemColors.Window;
            richTextBox.SelectionStart = 0;
            richTextBox.DeselectAll();
            richTextBox.SelectionStart = 0;
        }
    }
}
