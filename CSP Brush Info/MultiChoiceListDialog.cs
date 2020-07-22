using System.Collections.Generic;
using System.Windows.Forms;

namespace CSPBrushInfo {

    public partial class MultiChoiceListDialog : Form {
        private List<string> fileList;
        public List<string> SelectedList { get; set;  }

        public MultiChoiceListDialog(List<string> fileList) {
            this.fileList = fileList;

            InitializeComponent();

            populateList();
        }

        private void populateList() {
            listBoxFiles.DataSource = null;
            List<string> items = new List<string>();
            foreach (string fileName in fileList) {
                items.Add(fileName);
            }
            listBoxFiles.DataSource = items;
        }

        private void onOkClick(object sender, System.EventArgs e) {
            SelectedList = new List<string>();
            foreach (object item in listBoxFiles.SelectedItems) {
                SelectedList.Add(item.ToString());
            }
            this.DialogResult = DialogResult.OK;
            Close();
        }

        private void onCancelClick(object sender, System.EventArgs e) {
            SelectedList = null;
            this.DialogResult = DialogResult.Cancel;
            Close();

        }
    }
}
