using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text;

namespace CSPBrushInfo {
    class DatabaseUtils {
        public static readonly String NL = Environment.NewLine;

        public static string getToolHierarchy(string database) {
            string TAB = "   ";
            StringBuilder sb;
            sb = new StringBuilder();

            if (String.IsNullOrEmpty(database)) {
                sb.AppendLine("No database specified");
                return sb.ToString();
            } else {
                sb.AppendLine(database);
            }
            sb.AppendLine(DateTime.Now.ToString());
            string machineName = null;
            try {
                machineName = Environment.MachineName;
            } catch (Exception) {
                // Do nothing
            }
            if (!String.IsNullOrEmpty(machineName)) {
                sb.AppendLine("Computer: " + machineName);
            }

            if (!File.Exists(database)) {
                sb.AppendLine("Does not exist: " + database);
                return sb.ToString();
            }

            Dictionary<String, Tool> map = new Dictionary<String, Tool>();
            SQLiteConnection conn = null;
            SQLiteDataReader dataReader;
            DateTime modTime = File.GetLastWriteTime(database);
            Tool tool;
            try {
                using (conn = new SQLiteConnection("Data Source=" + database
                    + ";Version=3;Read Only=True;")) {
                    conn.Open();
                    SQLiteCommand command;
                    command = conn.CreateCommand();
                    command.CommandText = "SELECT _PW_ID, NodeVariantID, NodeName," +
                        " hex(NodeUUid), hex(NodeFirstChildUuid)," +
                        " hex(NodeNextUuid), hex(NodeSelectedUuid)" +
                        " FROM Node";
                    long id, nodeVariantID;
                    string nodeName;
                    string nodeUuid, nodeFirstChildUuid, nodeNextUuid, nodeSelectedUuid;
                    using (dataReader = command.ExecuteReader()) {
                        while (dataReader.Read()) {
                            id = dataReader.GetInt64(0);
                            nodeVariantID = dataReader.GetInt64(1);
                            nodeName = dataReader.GetString(2);
                            nodeUuid = dataReader.GetString(3);
                            nodeFirstChildUuid = dataReader.GetString(4);
                            nodeNextUuid = dataReader.GetString(5);
                            nodeSelectedUuid = dataReader.GetString(6);
                            tool = new Tool(id, nodeVariantID, nodeName,
                                nodeUuid, nodeFirstChildUuid,
                                nodeNextUuid, nodeSelectedUuid);
                            map.Add(nodeUuid, tool);
                        }
                    }
                    if (map.Count == 0) {
                        sb.AppendLine("Did not find any tools");
                        return sb.ToString();
                    }
                }
            } catch (Exception ex) {
                Utils.Utils.excMsg("Error reading " + database, ex);
                sb.AppendLine("Error reading " + database);
                return sb.ToString();
            }

            // Loop over the elements finding the top one (name is blank)
            // Then trace the first and next values for the groups and subtools
            sb.AppendLine();
            Tool firstChild, firstChild1, firstChild2;
            int nTools = 0, nGroups = 0, nSubTools = 0;
            // Use this to sort the Dictionary (doesn't matter here)
            //foreach (KeyValuePair<string, Tool> entry in
            //    map.OrderBy(entry => entry.Value.nodeName)) {
            foreach (KeyValuePair<string, Tool> entry in map) {
                tool = entry.Value;
                // Only process the top level which has a blank name and _PW_ID=1
                if (tool.nodeName.Length != 0) {
                    continue;
                }
                String nodeFirstChildUuid = tool.nodeFirstChildUuid;
                if (nodeFirstChildUuid == null
                    || nodeFirstChildUuid.Length != 32) {
                    continue;
                }
                map.TryGetValue(nodeFirstChildUuid, out firstChild);
                if (firstChild.nodeFirstChildUuid == null
                    || firstChild.nodeUuid.Length != 32) {
                    continue;
                }
                // Get the tools
                while (firstChild != null
                    && firstChild.nodeNextUuid.Length == 32) {
                    // sb.AppendLine("Tool: " + firstChild.nodeName
                    // + " nodeUuid=" + firstChild.nodeUuid
                    // + " nodeFirstChildUuid=" + firstChild.nodeFirstChildUuid);
                    nTools++;
                    sb.AppendLine("Tool: " + firstChild.nodeName);

                    map.TryGetValue(firstChild.nodeFirstChildUuid, out firstChild1);
                    if (firstChild1 == null || firstChild1.nodeFirstChildUuid == null
                        || firstChild1.nodeFirstChildUuid.Length != 32) {
                        continue;
                    }
                    while (firstChild1 != null
                        && firstChild1.nodeUuid.Length == 32) {
                        // sb.AppendLine(
                        // TAB + "Group: " + firstChild1.nodeName + " nodeUuid="
                        // + firstChild1.nodeUuid + " nodeFirstChildUuid="
                        // + firstChild1.nodeFirstChildUuid);
                        nGroups++;
                        sb.AppendLine(TAB + "Group: " + firstChild1.nodeName);

                        // Get the sub tools
                        map.TryGetValue(firstChild1.nodeFirstChildUuid, out firstChild2);
                        if (firstChild2 == null) {
                            continue;
                        }
                        while (firstChild2 != null
                            && firstChild2.nodeUuid.Length == 32) {
                            // sb.AppendLine(TAB + TAB + "SubTool: "
                            // + firstChild2.nodeName + " nodeUuid="
                            // + firstChild2.nodeUuid + " nodeFirstChildUuid="
                            // + firstChild2.nodeFirstChildUuid);
                            nSubTools++;
                            sb.AppendLine(
                                TAB + TAB + "SubTool: " + firstChild2.nodeName);
                            map.TryGetValue(firstChild2.nodeNextUuid, out firstChild2);
                        }

                        map.TryGetValue(firstChild1.nodeNextUuid, out firstChild1);
                    }
                    map.TryGetValue(firstChild.nodeNextUuid, out firstChild);
                }
            }
            sb.AppendLine(NL + "nTools=" + nTools + " nGroups=" + nGroups
                + " nSubTools=" + nSubTools);

            return sb.ToString();
        }

    }

    public class Tool {
        public long id;
        public long nodeVariantID;
        public String nodeName;
        public String nodeUuid;
        public String nodeFirstChildUuid;
        public String nodeNextUuid;
        public String nodeSelectedUuid;

        public Tool(long id, long nodeVariantID, String nodeName, String nodeUuid,
            String nodeFirstChildUuid, String nodeNextUuid,
            String nodeSelectedUuid) {
            this.id = id;
            this.nodeVariantID = nodeVariantID;
            this.nodeName = nodeName;
            this.nodeUuid = nodeUuid;
            this.nodeFirstChildUuid = nodeFirstChildUuid;
            this.nodeNextUuid = nodeNextUuid;
            this.nodeSelectedUuid = nodeSelectedUuid;
        }
    }

}
