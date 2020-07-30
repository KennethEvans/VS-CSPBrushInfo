//#define debugging
//#define replaceDoctype

using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using About;

namespace CSPBrushInfo {

    public partial class MainForm : Form {
        enum FileType { Database1, Database2, Brush1, Brush2 };
        public static readonly int PROCESS_TIMEOUT = 5000; // ms
        public static readonly String NL = Environment.NewLine;
        private static ScrolledHTMLDialog overviewDlg;
        private static ScrolledTextDialog textDlg;

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
            radioButtonVariant1.Checked = Properties.Settings.Default.BrushVariant1;
            radioButtonVariant2.Checked = Properties.Settings.Default.BrushVariant2;
            radioButtonInitVariant1.Checked = !radioButtonVariant1.Checked;
            radioButtonInitVariant2.Checked = !radioButtonVariant2.Checked;
        }

        /// <summary>
        /// Process a database.
        /// </summary>
        /// <param name="fileType">Determines if databse 1 or database 2</param>
        /// <param name="print">Whether to write to textBoxInfo.</param>
        private void processDatabase(FileType fileType, bool print) {
            int nDatabase = 1;
            TextBox textBoxDatabase = null;
            TextBox textBoxBrush = null;
            RadioButton radioButtonVariant = null;
            List<CSPBrushParam> paramsList = null;
            CSPBrushParam param = null;
            string info = null;
            switch (fileType) {
                case FileType.Database1:
                    nDatabase = 1;
                    textBoxDatabase = textBoxDatabase1;
                    textBoxBrush = textBoxBrush1;
                    radioButtonVariant = radioButtonVariant1;
                    break;
                case FileType.Database2:
                    nDatabase = 2;
                    textBoxDatabase = textBoxDatabase2;
                    textBoxBrush = textBoxBrush2;
                    radioButtonVariant = radioButtonVariant2;
                    break;
                default:
                    Utils.Utils.errMsg("Invalid fileType ("
                        + fileType + ") for processDatabase");
                    return;
            }
            textBoxInfo.Clear();
            paramsList = new List<CSPBrushParam>();
            info = "";
            String name = "";
            string nodeName = null;
            int nodeVariantId = 0, nodeInitVariantId = 0;
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
            string brushName = textBoxBrush.Text;
            if (brushName == null | brushName.Length == 0) {
                registerOutput(fileType, info, paramsList);
                Utils.Utils.errMsg("Brush not specified");
                return;
            }

            SQLiteConnection conn = null;
            SQLiteDataReader dataReader;
            DateTime modTime = File.GetLastWriteTime(name);
            info += name + NL;
            info += "Brush: " + brushName + NL;
            info += "Using: "
               + (radioButtonVariant.Checked ?
               "NodeVariantID" : "NodeInitVariantID") + NL;
            info += "Modified: " + modTime + NL;
            // Find the node
            try {
                using (conn = new SQLiteConnection("Data Source=" + name
                    + ";Version=3;Read Only=True;")) {
                    conn.Open();
                    SQLiteCommand command;
                    command = conn.CreateCommand();
                    // Need to replace single quotes by double
                    command.CommandText = "SELECT NodeName, NodeVariantId, " +
                        "NodeInitVariantId FROM Node WHERE NodeName='"
                        + brushName.Replace("'", "''") + "'";
                    List<NodeInfo> items = new List<NodeInfo>();
                    using (dataReader = command.ExecuteReader()) {
                        if (!dataReader.HasRows) {
                            Utils.Utils.errMsg("No matching rows looking for "
                                + brushName);
                            registerOutput(fileType, info, paramsList);
                            return;
                        }
                        while (dataReader.Read()) {
                            nodeName = dataReader.GetString(0);
                            nodeVariantId = dataReader.GetInt32(1);
                            nodeInitVariantId = dataReader.GetInt32(2);
                            items.Add(new NodeInfo(nodeName, nodeVariantId, nodeInitVariantId));
                        }
                    }
                    if (items.Count == 0) {
                        Utils.Utils.errMsg("Did not find any matches for " + brushName);
                        registerOutput(fileType, info, paramsList);
                        return;
                    }
                    if (items.Count == 1) {
                        nodeName = items[0].NodeName;
                        nodeVariantId = items[0].NodeVariantId;
                        nodeInitVariantId = items[0].NodeInitVariantId;
                    } else {
                        // Found more than one matching item, prompt for which one
                        List<string> itemsList = new List<string>();
                        foreach (NodeInfo nodeInfo in items) {
                            itemsList.Add(nodeInfo.Info());
                        }
                        MultiChoiceListDialog dlg = new MultiChoiceListDialog(itemsList);
                        switch (fileType) {
                            case FileType.Database1:
                                dlg.Text = "Brush 1 Ambiguity";
                                break;
                            case FileType.Database2:
                                dlg.Text = "Brush 2 Ambiguity";
                                break;
                            default:
                                dlg.Text = "Brush Ambiguity";
                                break;
                        }
                        // Note that the ListBox has be set to have SelectionMode=One,
                        // not MultiExtended
                        if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                            List<string> selectedList = dlg.SelectedList;
                            if (selectedList == null || selectedList.Count == 0) {
                                Utils.Utils.errMsg("No items selected");
                                registerOutput(fileType, info, paramsList);
                                return;
                            }
                            // There should be only one entry
                            string item = selectedList[0];
                            nodeName = null;
                            foreach (NodeInfo nodeInfo in items) {
                                if (item.Equals(nodeInfo.Info())) {
                                    nodeName = nodeInfo.NodeName;
                                    nodeVariantId = nodeInfo.NodeVariantId;
                                    nodeInitVariantId = nodeInfo.NodeInitVariantId;
                                }
                            }
                        } else {
                            info += "Cancelled" + NL;
                            registerOutput(fileType, info, paramsList);
                            return;
                        }
                    }
                    // These should not happen
                    if (nodeName == null) {
                        Utils.Utils.errMsg("Failed to determine which brush");
                        registerOutput(fileType, info, paramsList);
                    }
                    if (!nodeName.Equals(brushName)) {
                        Utils.Utils.errMsg("Looking for " + brushName + ", found "
                            + nodeName);
                        registerOutput(fileType, info, paramsList);
                        return;
                    }
                    if (nodeVariantId == 0) {
                        Utils.Utils.errMsg(brushName
                            + " is not a brush (No NodeVariantId)");
                        registerOutput(fileType, info, paramsList);
                        return;
                    }
                }
            } catch (Exception ex) {
                Utils.Utils.excMsg("Error finding " + brushName, ex);
                registerOutput(fileType, info, paramsList);
                return;
            }

            // Find the variant
            conn = null;
            try {
                using (conn = new SQLiteConnection("Data Source=" + name
                    + ";Version=3;Read Only=True;")) {
                    conn.Open();
                    SQLiteCommand command;
                    command = conn.CreateCommand();
                    int variantId = (radioButtonVariant.Checked) ?
                        nodeVariantId : nodeInitVariantId;
                    command.CommandText = "SELECT * FROM Variant WHERE VariantID="
                                            + variantId;
                    using (dataReader = command.ExecuteReader()) {
                        if (!dataReader.HasRows) {
                            Utils.Utils.errMsg("No matching rows looking for VariantID = "
                                + variantId);
                            registerOutput(fileType, info, paramsList);
                            return;
                        }
                        dataReader.Read();
                        nCols = dataReader.FieldCount;
                        if (print) {
                            appendInfo(info + NL);
                        }
                        for (int i = 0; i < nCols; i++) {
                            if (dataReader.IsDBNull(i)) {
                                nNull++;
                                continue;
                            }
                            param = new CSPBrushParam(dataReader.GetName(i),
                                dataReader.GetDataTypeName(i), dataReader.GetValue(i));
                            param.Value = dataReader.GetValue(i);
                            //    appendInfo(param.info());
                            //}
                            paramsList.Add(param);
                        }
                    }
                }
            } catch (Exception ex) {
                Utils.Utils.excMsg("Error finding VariantID=" + nodeVariantId, ex);
                registerOutput(fileType, info, paramsList);
                return;
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
            appendInfo("2: ");
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
                    appendInfo(param1.Name + NL);
                    appendInfo("  1: " + param1.getValueAsString("  "));
                    appendImages(param1);
                    appendInfo("  2: Not found in 2" + NL);
                    continue;
                }
                if (found && !param1.equals(foundParam)) {
                    appendInfo(param1.Name + NL);
                    appendInfo("  1: " + param1.getValueAsString("  "));
                    appendImages(param1);
                    appendInfo("  2: " + foundParam.getValueAsString("  "));
                    appendImages(foundParam);
                }
            }

            // Look for items in 2 that are not in 1
            foreach (CSPBrushParam param2 in params2) {
                found = false;
                // Look for the same name
                foreach (CSPBrushParam param1 in params1) {
                    if (param1.Name.Equals(param2.Name)) {
                        found = true;
                        break;
                    }
                }
                if (!found) {
                    appendInfo(param2.Name + NL);
                    appendInfo("  1: Not found in 1" + NL);
                    appendInfo("  2: " + param2.getValueAsString("  "));
                    appendImages(param2);
                    continue;
                }
            }
        }

        /// <summary>
        /// Searchs textBoxInfo for "Error";
        /// </summary>
        /// <returns></returns>
        private bool checkForErrors() {
            int index = textBoxInfo.Find("Error ",
                RichTextBoxFinds.MatchCase & RichTextBoxFinds.NoHighlight);
            return index != -1;
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
                    appendInfo(info1 + NL);
                    break;
                case FileType.Database2:
                    appendInfo(info2 + NL);
                    break;
            }
        }

        /// <summary>
        /// Processes images to be included in the RTF.  Generates an RTF
        /// string and inserts it into textBoxInfo.
        /// </summary>
        /// <param name="images"></param>
        private void processImagesUsingRtf(List<Bitmap> images) {
            try {
                appendInfo("    ");
                String rtf;
                foreach (Bitmap bm in images) {
                    rtf = Utils.RTFUtils.imageRtf(textBoxInfo, bm);
                    if (!String.IsNullOrEmpty(rtf)) {
                        Utils.RTFUtils.appendRtb(textBoxInfo, rtf);
                        appendInfo("    ");
                    }
                }
                appendInfo(NL);
            } catch (Exception ex) {
                Utils.Utils.excMsg("Error processing effector images", ex);
            }
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e) {
            Properties.Settings.Default.DatabaseName1 = textBoxDatabase1.Text;
            Properties.Settings.Default.DatabaseName2 = textBoxDatabase2.Text;
            Properties.Settings.Default.BrushName1 = textBoxBrush1.Text;
            Properties.Settings.Default.BrushName2 = textBoxBrush2.Text;
            Properties.Settings.Default.BrushVariant1 = radioButtonVariant1.Checked;
            Properties.Settings.Default.BrushVariant2 = radioButtonVariant2.Checked;
            Properties.Settings.Default.Save();
        }

        private void appendInfo(string info) {
            textBoxInfo.AppendText(info);
        }

        private void appendImages(CSPBrushParam param) {
            if (param.Name.ToLower().Contains("effector")) {
                List<Bitmap> images = param.getEffectorImages("  ");
                if (images != null && images.Count > 0) {
                    // Insert RTF string
                    processImagesUsingRtf(images);
                }
            }
        }

        /// <summary>
        /// Inserts the given text at the start of textBoxInfo.
        /// </summary>
        /// <param name="text"></param>
        private void InsertAtInfoTop(string text) {
            if (String.IsNullOrEmpty(text)) return;
            textBoxInfo.SelectionStart = 0;
            textBoxInfo.SelectionLength = 0;
            textBoxInfo.SelectedText = text;
        }

        private void OnProcess1Click(object sender, EventArgs e) {
            processDatabase(FileType.Database1, true);
            textBoxInfo.Clear();
            printHeading(FileType.Database1);
            foreach (CSPBrushParam param in params1) {
                appendInfo(param.info("  "));
                appendImages(param);
            }
            // Check for errors
            if (checkForErrors()) {
                InsertAtInfoTop("!!! Note: There were errors during processing"
                    + NL + NL);
            }
        }

        private void OnProcess2Click(object sender, EventArgs e) {
            processDatabase(FileType.Database2, true);
            textBoxInfo.Clear();
            printHeading(FileType.Database2);
            foreach (CSPBrushParam param in params2) {
                appendInfo(param.info("  "));
                appendImages(param);
            }
            // Check for errors
            if (checkForErrors()) {
                InsertAtInfoTop("!!! There are errors" + NL + NL);
            }
        }

        private void OnCompareClick(object sender, EventArgs e) {
            compare();
            // Check for errors
            if (checkForErrors()) {
                InsertAtInfoTop("!!! There are errors" + NL + NL);
            }
        }

        private void OnQuitClick(object sender, EventArgs e) {
            Close();
        }

        private void OnOverviewClick(object sender, EventArgs e) {
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
            string brushName = textBoxBrush1.Text;
            if (databaseName == null || databaseName.Length == 0) {
                Utils.Utils.errMsg("Database 1 is not defined");
                return;
            }
            if (!File.Exists(databaseName)) {
                Utils.Utils.errMsg("Database 1 does not exist");
                return;
            }
            BrushesInDatabaseDialog dlg = new BrushesInDatabaseDialog(databaseName,
                brushName);
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
            string brushName = textBoxBrush2.Text;
            if (databaseName == null || databaseName.Length == 0) {
                Utils.Utils.errMsg("Database 2 is not defined");
                return;
            }
            if (!File.Exists(databaseName)) {
                Utils.Utils.errMsg("Database 2 does not exist");
                return;
            }
            BrushesInDatabaseDialog dlg = new BrushesInDatabaseDialog(databaseName,
              brushName);
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

        private void OnSaveRtfClick(object sender, EventArgs e) {
            if (textBoxInfo == null) {
                return;
            }
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "RTF Files|*.rtf";
            dlg.Title = "Save as RTF";
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                try {
                    textBoxInfo.SaveFile(dlg.FileName,
                        RichTextBoxStreamType.RichText);
                } catch (Exception ex) {
                    Utils.Utils.excMsg("Error saving RTF", ex);
                }
            }
        }

        private void OnShowToolHierarchy(object sender, EventArgs e) {
            string label = sender.ToString();
            string database;
            if (label.StartsWith("Database 2")) {
                database = textBoxDatabase2.Text;
            } else {
                database = textBoxDatabase1.Text;
            }
            string info = DatabaseUtils.getToolHierarchy(database);
            // Create, show, or set visible the overview dialog as appropriate
            if (textDlg == null) {
                MainForm app = (MainForm)FindForm().FindForm();
                textDlg = new ScrolledTextDialog(
                    Utils.Utils.getDpiAdjustedSize(app, new Size(600, 400)),
                    info);
                textDlg.Text = "Tool Hierarchy";
                textDlg.Show();
            } else {
                textDlg.Visible = true;
            }
        }

        // RichTextBox context menu
        private void OnCutClick(object sender, EventArgs e) {
            textBoxInfo.Cut();
        }

        private void OnCopyClick(object sender, EventArgs e) {
            textBoxInfo.Copy();
        }

        private void OnPasteClick(object sender, EventArgs e) {
            textBoxInfo.Paste();
        }

        private void OnSelectAllClick(object sender, EventArgs e) {
            textBoxInfo.SelectAll();
        }
    }

    public class NodeInfo {
        string nodeName;
        int nodeVariantId;
        int nodeInitVariantId;

        public string NodeName { get => nodeName; set => nodeName = value; }
        public int NodeVariantId { get => nodeVariantId; set => nodeVariantId = value; }
        public int NodeInitVariantId { get => nodeInitVariantId; set => nodeInitVariantId = value; }


        public NodeInfo(string nodeName, int nodeVariantId, int nodeInitVariantId) {
            NodeName = nodeName;
            NodeVariantId = nodeVariantId;
            NodeInitVariantId = nodeInitVariantId;
        }

        public string Info() {
            return nodeName + " NodeVariantId=" + nodeVariantId
                + " NodeInitVariantId=" + nodeInitVariantId;
        }
    }
}