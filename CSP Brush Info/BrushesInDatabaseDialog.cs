using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Windows.Forms;

namespace CSPBrushInfo {
    public partial class BrushesInDatabaseDialog : Form {
        private string selected;
        private string database;
        private List<string> items;
        private string origName;

        public string SelectedBrush { get => selected; set => selected = value; }
        public string Database { get => database; set => database = value; }

        public BrushesInDatabaseDialog(string bundleName, string origName) {
            InitializeComponent();

            this.origName = origName;
            textBoxDatabase.Text = bundleName;
            textBoxDatabase.Select(0, 0);
            find();
        }

        private void OnBrowseClick(object sender, EventArgs e) {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "Select a Database";
            string fileName = textBoxDatabase.Text;
            // Set initial directory
            if (File.Exists(fileName)) {
                dlg.FileName = fileName;
                dlg.InitialDirectory = Path.GetDirectoryName(fileName);
            }
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                textBoxDatabase.Text = dlg.FileName;
                find();
            }
        }

        private List<string> getFilteredItems() {
            if (items == null) {
                return null;
            }
            string filter = textBoxFilter.Text;
            if (String.IsNullOrEmpty(filter)) {
                return items;
            }
            List<String> filteredItems = new List<String>();
            foreach (string item in items) {
                if (item.Contains(filter)) {
                    filteredItems.Add(item);
                }
            }
            return filteredItems;
        }

        private void find() {
            listBoxBrushes.DataSource = null;
            string name = textBoxDatabase.Text;
            if (name == null || name.Length == 0) {
                Utils.Utils.errMsg("Database is not defined");
                return;
            }
            if (!File.Exists(name)) {
                Utils.Utils.errMsg(name + " does not exist");
                return;
            }
            // Get the brushes from the database
            SQLiteConnection conn = null;
            try {
                items = new List<string>();
                conn = new SQLiteConnection("Data Source=" + name + ";Version=3;Read Only=True;");
                conn.Open();
                SQLiteDataReader dataReader;
                SQLiteCommand command;
                command = conn.CreateCommand();
                command.CommandText = "SELECT NodeName, NodeVariantId, NodeInitVariantId FROM Node";

                dataReader = command.ExecuteReader();
                string nodeName;
                int nodeVariantId, nodeInitVariantId;
                if (!dataReader.HasRows) {
                    Utils.Utils.errMsg("No matching rows found");
                    return;
                }
                while (dataReader.Read()) {
                    nodeName = dataReader.GetString(0);
                    nodeVariantId = dataReader.GetInt32(1);
                    nodeInitVariantId = dataReader.GetInt32(2);
                    if (nodeVariantId != 0 || nodeInitVariantId != 0) {
                        items.Add(nodeName);
                    }
                }
                items.Sort();
                List<String> filteredItems = getFilteredItems();
                if (filteredItems != null && filteredItems != items && filteredItems.Count > 0) {
                    listBoxBrushes.DataSource = filteredItems;
                } else {
                    listBoxBrushes.DataSource = items;
                }
                // Select the original item
                if (!String.IsNullOrEmpty(origName)) {
                    int index = listBoxBrushes.FindString(origName);
                    if (index >= 0) {
                        listBoxBrushes.SelectedIndex = index;
                    }
                }
            } catch (Exception ex) {
                Utils.Utils.excMsg("Failed to get brushes", ex);
                return;
            } finally {
                if (conn != null) {
                    conn.Close();
                    conn = null;
                }
            }
        }

        private void OnFindClick(object sender, EventArgs e) {
            find();
        }

        private void OnCancelClick(object sender, EventArgs e) {
            selected = null;
            database = textBoxDatabase.Text;
            this.DialogResult = DialogResult.Cancel;
            this.Visible = false;
        }

        private void OnOkClick(object sender, EventArgs e) {
            selected = listBoxBrushes.GetItemText(listBoxBrushes.SelectedItem);
            database = textBoxDatabase.Text;
            this.DialogResult = DialogResult.OK;
            this.Visible = false;
        }

        private void onTextBoxFilterKeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) {
                // Keep it from dinging because is is not multi-line
                e.SuppressKeyPress = true;
                List<String> filteredItems = getFilteredItems();
                if (filteredItems != null) { }
                listBoxBrushes.DataSource = filteredItems;
                // Select the original item
                if (!String.IsNullOrEmpty(origName)) {
                    int index = listBoxBrushes.FindString(origName);
                    if (index >= 0) {
                        listBoxBrushes.SelectedIndex = index;
                    }
                }
            }
        }
    }
}
