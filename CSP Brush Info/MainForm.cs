//#define debugging
//#define replaceDoctype

using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace CSPBrushInfo {

    public partial class MainForm : Form {
        enum FileType { Database1, Database2, Brush1, Brush2 };
        public readonly int PROCESS_TIMEOUT = 5000; // ms
        public readonly String NL = Environment.NewLine;
        private static ScrolledHTMLDialog overviewDlg;

        private List<CSPBrushParam> params1 = new List<CSPBrushParam>();
        private List<CSPBrushParam> params2 = new List<CSPBrushParam>();
        private string info1;
        private string info2;
        //private List<string> attributes1 = new List<string>();
        //private List<string> attributes2 = new List<string>();
        //private List<KritaPresetParam> paramsCur;
        //private List<string> attributesCur;
        //private FileType fileTypeCur;

        public MainForm() {
            InitializeComponent();

            textBoxDatabase1.Text = Properties.Settings.Default.DatabaseName1;
            textBoxDatabase2.Text = Properties.Settings.Default.DatabaseName2;
            textBoxBrush1.Text = Properties.Settings.Default.BrushName1;
            textBoxBrush2.Text = Properties.Settings.Default.BrushName2;
        }

        /// <summary>
        /// Process a database.
        /// </summary>
        /// <param name="fileType">Determines if databse 1 or database 2</param>
        /// <param name="print">Whether to write to textBoxInfo.</param>
        private void processDatabase(FileType fileType, bool print) {
            int nDatabase = 1;
            TextBox textBoxDatabase = null;
            List<CSPBrushParam> paramsList = null;
            CSPBrushParam param = null;
            string info = null;
            switch (fileType) {
                case FileType.Database1:
                    nDatabase = 1;
                    textBoxDatabase = textBoxDatabase1;
                    break;
                case FileType.Database2:
                    nDatabase = 2;
                    textBoxDatabase = textBoxDatabase2;
                    break;
                default:
                    Utils.Utils.errMsg("Invalid fileType (" + fileType + ") for processDatabase");
                    return;
            }
            textBoxInfo.Clear();
            paramsList = new List<CSPBrushParam>();
            info = "";
            String name = "";
            string nodeName;
            int nodeVariantId, nodeInitVariantId;
            int nCols = 0;
            int nNull = 0;
            name = textBoxDatabase.Text;
            if (name == null || name.Length == 0) {
                registerOutput(fileType, info, paramsList);
                Utils.Utils.errMsg("Database " + nDatabase + " is not defined");
                return;
            }
            if (!File.Exists(name)) {
                registerOutput(fileType, info, paramsList);
                Utils.Utils.errMsg(name + " does not exist");
                return;
            }
            // Get the selected brush name
            string brushName = textBoxBrush1.Text;
            if (brushName == null | brushName.Length == 0) {
                registerOutput(fileType, info, paramsList);
                Utils.Utils.errMsg(brushName + " not specified");
                return;
            }

            SQLiteConnection conn = null;
            DateTime modTime = File.GetLastWriteTime(name);
            info += name + NL;
            info += "Modified: " + modTime + NL;
            // Find the node
            try {
                conn = new SQLiteConnection("Data Source=" + name + ";Version=3;Read Only=True;");
                conn.Open();
                SQLiteDataReader dataReader;
                SQLiteCommand command;
                command = conn.CreateCommand();
                command.CommandText = "SELECT NodeName, NodeVariantId, " +
                    "NodeInitVariantId FROM Node WHERE NodeName='"
                    + brushName + "'";
                dataReader = command.ExecuteReader();
                if (!dataReader.HasRows) {
                    Utils.Utils.errMsg("No matching rows looking for " + brushName);
                    registerOutput(fileType, info, paramsList);
                    return;
                }
                dataReader.Read();
                nodeName = dataReader.GetString(0);
                nodeVariantId = dataReader.GetInt32(1);
                nodeInitVariantId = dataReader.GetInt32(2);
                if (!nodeName.Equals(brushName)) {
                    Utils.Utils.errMsg("Looking for " + brushName + ", found " + nodeName);
                    registerOutput(fileType, info, paramsList);
                    return;
                }
                if (nodeVariantId == 0) {
                    Utils.Utils.errMsg(brushName + " is not a brush (No Nodevariant Id)");
                    registerOutput(fileType, info, paramsList);
                    return;
                }
            } catch (Exception ex) {
                Utils.Utils.excMsg("Failed to find " + brushName, ex);
                registerOutput(fileType, info, paramsList);
                return;
            } finally {
                if (conn != null) {
                    conn.Close();
                    conn = null;
                }
            }

            // Find the variant
            conn = null;
            try {
                conn = new SQLiteConnection("Data Source=" + name + ";Version=3;Read Only=True;");
                conn.Open();
                SQLiteDataReader dataReader;
                SQLiteCommand command;
                command = conn.CreateCommand();
                command.CommandText = "SELECT * FROM Variant WHERE VariantID=" + nodeVariantId;
                dataReader = command.ExecuteReader();
                if (!dataReader.HasRows) {
                    registerOutput(fileType, info, paramsList);
                    Utils.Utils.errMsg("No matching rows forLooking for VariantID = " + nodeVariantId);
                    return;
                }
                dataReader.Read();
                nCols = dataReader.FieldCount;
                if (print) {
                    textBoxInfo.AppendText(info + NL);
                }
                for (int i = 0; i < nCols; i++) {
                    if (dataReader.IsDBNull(i)) {
                        nNull++;
                        continue;
                    }
                    param = new CSPBrushParam(dataReader.GetName(i),
                        dataReader.GetDataTypeName(i), dataReader.GetValue(i));
                    param.Value = dataReader.GetValue(i);
                    //    textBoxInfo.AppendText(param.info());
                    //}
                    paramsList.Add(param);
                }
            } catch (Exception ex) {
                Utils.Utils.excMsg("Failed to find VariantID=" + nodeVariantId, ex);
                registerOutput(fileType, info, paramsList);
                return;
            } finally {
                if (conn != null) {
                    conn.Close();
                    conn = null;
                }
            }
            info += "Columns: " + nCols + " Null: " + nNull + " Non-Null: "
                + (nCols - nNull) + NL;
            registerOutput(fileType, info, paramsList);
        }

        /// <summary>
        /// Compares the two files and displays the output.
        /// </summary>
        private void compare() {
            // Process 1
            processDatabase(FileType.Database1, false);
            if (params1.Count == 0) {
                Utils.Utils.errMsg("Did not get params for Brush 1");
                return;
            }
            // Process 2
            processDatabase(FileType.Database2, false);
            if (params2.Count == 0) {
                Utils.Utils.errMsg("Did not get params for Brush 2");
                return;
            }

            // Write heading to textBoxInfo
            textBoxInfo.Text = "1: ";
            printHeading(FileType.Database1);
            textBoxInfo.AppendText("2: ");
            printHeading(FileType.Database2);
            // Look for items in 2 that are in 1
            bool found;
            CSPBrushParam foundParam = null;
            foreach (CSPBrushParam param1 in params1) {
                found = false;
                // Look for the same name
                foreach (CSPBrushParam param2 in params2) {
                    if (param1.Name.Equals(param2.Name)) {
                        found = true;
                        foundParam = param2;
                        break;
                    }
                }
                if (!found) {
                    textBoxInfo.AppendText(param1.Name + NL);
                    textBoxInfo.AppendText("  1: " + param1.getValueAsString("  "));
                    textBoxInfo.AppendText("  2: Not found in 2" + NL);
                    continue;
                }
                if (found && !param1.equals(foundParam)) {
                    textBoxInfo.AppendText(param1.Name + NL);
                    textBoxInfo.AppendText("  1: " + param1.getValueAsString("  "));
                    textBoxInfo.AppendText("  2: " + foundParam.getValueAsString("  "));
                }
            }

            // Look for items in 2 that are not in 1
            textBoxInfo.AppendText(NL);
            foreach (CSPBrushParam param2 in params2) {
                found = false;
                // Look for the same name
                foreach (CSPBrushParam param1 in params1) {
                    if (param1.Name.Equals(param2.Name)) {
                        found = true;
                        foundParam = param2;
                        break;
                    }
                }
                if (!found) {
                    textBoxInfo.AppendText(param2.Name + NL);
                    textBoxInfo.AppendText("  1: Not found in 1" + NL);
                    textBoxInfo.AppendText("  2: " + foundParam.getValueAsString("  "));
                    break;
                }
            }
        }

        private void getFileName(FileType type) {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "Select a File" + " (" + type + ")";
            string fileName = "";
            // Set initial directory
            switch (type) {
                case FileType.Database1:
                    //dlg.Filter = "Krita Presets|*.kpp";
                    fileName = textBoxDatabase1.Text;
                    break;
                case FileType.Database2:
                    //dlg.Filter = "Krita Presets|*.kpp";
                    fileName = textBoxDatabase2.Text;
                    break;
            }
            if (File.Exists(fileName)) {
                dlg.FileName = fileName;
                dlg.InitialDirectory = Path.GetDirectoryName(fileName);
            }
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                resetFileName(type, dlg.FileName);
            }
        }

        private void resetFileName(FileType type, string name) {
            switch (type) {
                case FileType.Database1:
                    textBoxDatabase1.Text = name;
                    Properties.Settings.Default.DatabaseName1 = name;
                    break;
                case FileType.Database2:
                    textBoxDatabase2.Text = name;
                    Properties.Settings.Default.DatabaseName2 = name;
                    break;
                case FileType.Brush1:
                    textBoxBrush1.Text = name;
                    Properties.Settings.Default.BrushName1 = name;
                    break;
                case FileType.Brush2:
                    textBoxBrush2.Text = name;
                    Properties.Settings.Default.BrushName2 = name;
                    break;
            }
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Sets the global values for info and paramsList.
        /// </summary>
        /// <param name="fileType"></param>
        /// <param name="info"></param>
        /// <param name="paramsList"></param>
        private void registerOutput(FileType fileType, string info,
            List<CSPBrushParam> paramsList) {
            switch (fileType) {
                case FileType.Database1:
                    info1 = info;
                    params1 = paramsList; ;
                    break;
                case FileType.Database2:
                    info2 = info;
                    params2 = paramsList; ;
                    break;
            }
        }

        /// <summary>
        /// Outputs a heading in testBoxInfo according to the type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private void printHeading(FileType type) {
            switch (type) {
                case FileType.Database1:
                    textBoxInfo.AppendText(info1 + NL);
                    break;
                case FileType.Database2:
                    textBoxInfo.AppendText(info2 + NL);
                    break;
            }
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e) {
            Properties.Settings.Default.DatabaseName1 = textBoxDatabase1.Text;
            Properties.Settings.Default.DatabaseName2 = textBoxDatabase2.Text;
            Properties.Settings.Default.BrushName1 = textBoxBrush1.Text;
            Properties.Settings.Default.BrushName2 = textBoxBrush2.Text;
            Properties.Settings.Default.Save();
        }

        private void OnProcess1Click(object sender, EventArgs e) {
            processDatabase(FileType.Database1, true);
            textBoxInfo.Clear();
            printHeading(FileType.Database1);
            foreach (CSPBrushParam param in params1) {
                textBoxInfo.AppendText(param.info("  "));
            }
        }

        private void OnProcess2Click(object sender, EventArgs e) {
            processDatabase(FileType.Database2, true);
            textBoxInfo.Clear();
            printHeading(FileType.Database2);
            foreach (CSPBrushParam param in params2) {
                textBoxInfo.AppendText(param.info("  "));
            }
        }

        private void OnCompareClick(object sender, EventArgs e) {
            compare();
        }

        private void OnQuitCick(object sender, EventArgs e) {
            Close();
        }

        private void onOverviewClick(object sender, EventArgs e) {
            // Create, show, or set visible the overview dialog as appropriate
            if (overviewDlg == null) {
                MainForm app = (MainForm)FindForm().FindForm();
                overviewDlg = new ScrolledHTMLDialog(
                    Utils.Utils.getDpiAdjustedSize(app, new Size(800, 600)));
                overviewDlg.Show();
            } else {
                overviewDlg.Visible = true;
            }
        }

        private void OnAboutClick(object sender, EventArgs e) {
            AboutBox dlg = new AboutBox();
            dlg.ShowDialog();
        }

        private void OnBrowseDatabase1Click(object sender, EventArgs e) {
            getFileName(FileType.Database1);
        }

        private void OnBrowseDatabase2Click(object sender, EventArgs e) {
            getFileName(FileType.Database2);
        }

        private void OnBrowseBrush1Click(object sender, EventArgs e) {
            string databaseName = textBoxDatabase1.Text;
            if (databaseName == null || databaseName.Length == 0) {
                Utils.Utils.errMsg("Database 1 is not defined");
                return;
            }
            if (!File.Exists(databaseName)) {
                Utils.Utils.errMsg("Database 1 does not exist");
                return;
            }
            BrushesInDatabaseDialog dlg = new BrushesInDatabaseDialog(databaseName);
            // Create, show, or set visible the preferences dialog as appropriate
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                if (dlg.SelectedBrush != null) {
                    textBoxBrush1.Text = dlg.SelectedBrush;
                    textBoxDatabase1.Text = dlg.Database;
                } else {
                    Utils.Utils.errMsg("No items selected");
                }
            }
        }

        private void OnBrowseBrush2Click(object sender, EventArgs e) {
            string databaseName = textBoxDatabase2.Text;
            if (databaseName == null || databaseName.Length == 0) {
                Utils.Utils.errMsg("Database 2 is not defined");
                return;
            }
            if (!File.Exists(databaseName)) {
                Utils.Utils.errMsg("Database 2 does not exist");
                return;
            }
            BrushesInDatabaseDialog dlg = new BrushesInDatabaseDialog(databaseName);
            // Create, show, or set visible the preferences dialog as appropriate
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                if (dlg.SelectedBrush != null) {
                    textBoxBrush2.Text = dlg.SelectedBrush;
                    textBoxDatabase2.Text = dlg.Database;
                } else {
                    Utils.Utils.errMsg("No items selected");
                }
            }
        }

    }
}