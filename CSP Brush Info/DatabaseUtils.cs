using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;

namespace CSPBrushInfo {
    class DatabaseUtils {
        public static readonly String NL = Environment.NewLine;
        public static string networkString = "\\\\";

        /// <summary>
        /// Returns a name to use with new SQLiteConnection.
        /// Appends \\ if it is a network name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string getSqliteOpenName(string name) {
            if (name.StartsWith(networkString) &&
                !name.StartsWith(networkString + networkString)) {
                return networkString + name;
            } else {
                return name;
            }
        }

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
                string openName = DatabaseUtils.getSqliteOpenName(database);
                using (conn = new SQLiteConnection("Data Source=" + openName
                    + ";Version=3;Read Only=True;")) {
                    conn.Open();
                    SQLiteCommand command;
                    command = conn.CreateCommand();
                    command.CommandText = "SELECT _PW_ID," +
                        " NodeVariantID," + "NodeInitVariantId," + " NodeName," +
                        " hex(NodeUUid), hex(NodeFirstChildUuid)," +
                        " hex(NodeNextUuid), hex(NodeSelectedUuid)" +
                        " FROM Node";
                    long id, nodeVariantID, nodeInitVariantID;
                    string nodeName;
                    string nodeUuid, nodeFirstChildUuid, nodeNextUuid, nodeSelectedUuid;
                    using (dataReader = command.ExecuteReader()) {
                        while (dataReader.Read()) {
                            id = dataReader.GetInt64(0);
                            nodeVariantID = dataReader.GetInt64(1);
                            nodeInitVariantID = dataReader.GetInt64(2);
                            nodeName = dataReader.GetString(3);
                            nodeUuid = dataReader.GetString(4);
                            nodeFirstChildUuid = dataReader.GetString(5);
                            nodeNextUuid = dataReader.GetString(6);
                            nodeSelectedUuid = dataReader.GetString(7);
                            tool = new Tool(id, nodeVariantID, nodeInitVariantID,
                                nodeName, nodeUuid, nodeFirstChildUuid,
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
            // For each one found, set the nodeParentUUid.
            sb.AppendLine();
            Tool firstChild, firstChild1, firstChild2;
            int nTools = 0, nGroups = 0, nSubTools = 0;
            // Use this to sort the Dictionary (doesn't matter here)
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
                // Get the tools
                while (firstChild != null
                    && firstChild.nodeUuid.Length == 32) {
                    // sb.AppendLine("Tool: " + firstChild.nodeName
                    // + " nodeUuid=" + firstChild.nodeUuid
                    // + " nodeFirstChildUuid=" + firstChild.nodeFirstChildUuid);
                    nTools++;
                    sb.AppendLine("Tool: " + firstChild.nodeName);
                    firstChild.nodeParentUuid = tool.nodeUuid;

                    map.TryGetValue(firstChild.nodeFirstChildUuid, out firstChild1);
                    // Get the groups
                    while (firstChild1 != null
                        && firstChild1.nodeUuid.Length == 32) {
                        // sb.AppendLine(
                        // TAB + "Group: " + firstChild1.nodeName + " nodeUuid="
                        // + firstChild1.nodeUuid + " nodeFirstChildUuid="
                        // + firstChild1.nodeFirstChildUuid);
                        nGroups++;
                        sb.AppendLine(TAB + "Group: " + firstChild1.nodeName);
                        firstChild1.nodeParentUuid = firstChild.nodeUuid;

                        // Get the sub tools
                        map.TryGetValue(firstChild1.nodeFirstChildUuid, out firstChild2);
                        while (firstChild2 != null
                            && firstChild2.nodeUuid.Length == 32) {
                            // sb.AppendLine(TAB + TAB + "SubTool: "
                            // + firstChild2.nodeName + " nodeUuid="
                            // + firstChild2.nodeUuid + " nodeFirstChildUuid="
                            // + firstChild2.nodeFirstChildUuid);
                            nSubTools++;
                            sb.AppendLine(
                                TAB + TAB + "SubTool: " + firstChild2.nodeName);
                            firstChild2.nodeParentUuid = firstChild1.nodeUuid;
                            // Get the next subtool item
                            map.TryGetValue(firstChild2.nodeNextUuid, out firstChild2);
                        }
                        // Get the next group item
                        map.TryGetValue(firstChild1.nodeNextUuid, out firstChild1);
                    }
                    // Get the next tool item
                    map.TryGetValue(firstChild.nodeNextUuid, out firstChild);
                }
            }
            sb.AppendLine("nTools=" + nTools + " nGroups=" + nGroups
                + " nSubTools=" + nSubTools);

            // Check for orphans
            sb.AppendLine(NL + "Orphans");
            int nOrphans = 0;
            foreach (KeyValuePair<string, Tool> entry in
                map.OrderBy(entry => entry.Value.nodeName)) {
                tool = entry.Value;
                // Don't do the root
                if (tool.nodeName.Length == 0) continue;
                if (tool.nodeParentUuid == null || tool.nodeParentUuid.Length != 32) {
                    nOrphans++;
                    sb.AppendLine(
                        TAB + tool.nodeName + " _PW_ID(Node)=" + tool.id +
                        " nodeVariantId=" + tool.nodeVariantID);
                }
            }
            sb.AppendLine("nOrphans=" + nOrphans);

            // Check for duplicate brush names
            sb.AppendLine(NL + "Duplicate Brush Names");
            int nDuplicates = 0;
            Tool prev = null;
            bool first = true;
            foreach (KeyValuePair<string, Tool> entry in
                map.OrderBy(entry => entry.Value.nodeName)) {
                tool = entry.Value;
                // If nodeVariantID = 0, it's not a brush
                if (tool.nodeVariantID == 0) continue;
                // Can't have a duplicate for the first item
                if (prev == null) {
                    prev = tool;
                    continue;
                }
                if (tool.nodeName.Equals(prev.nodeName)) {
                    if (first) {
                        // Indicates the number of duplicate sets
                        nDuplicates++;
                        sb.AppendLine(
                            TAB + prev.nodeName
                            + " _PW_ID(Node)=" + prev.id
                            + " nodeVariantID=" + prev.nodeVariantID
                            + " nodeInitVariantID=" + prev.nodeInitVariantID);
                        first = false;
                    }
                    sb.AppendLine(
                       TAB + tool.nodeName + " _PW_ID(Node) =" + tool.id +
                        " nodeVariantID=" + tool.nodeVariantID
                        + " nodeInitVariantID=" + tool.nodeInitVariantID);
                    first = true;
                }
                prev = tool;
            }

            sb.AppendLine("nDuplicates=" + nDuplicates);

            return sb.ToString();
        }
    }

    public class Tool {
        public long id;
        public long nodeVariantID;
        public long nodeInitVariantID;
        public String nodeName;
        public String nodeUuid;
        public String nodeFirstChildUuid;
        public String nodeNextUuid;
        public String nodeSelectedUuid;
        // This is not a database column but is used for tracking orphans
        public String nodeParentUuid;

        public Tool(long id, long nodeVariantID, long nodeInitVariantID,
            String nodeName, String nodeUuid,
            String nodeFirstChildUuid, String nodeNextUuid,
            String nodeSelectedUuid) {
            this.id = id;
            this.nodeVariantID = nodeVariantID;
            this.nodeInitVariantID = nodeInitVariantID;
            this.nodeName = nodeName;
            this.nodeUuid = nodeUuid;
            this.nodeFirstChildUuid = nodeFirstChildUuid;
            this.nodeNextUuid = nodeNextUuid;
            this.nodeSelectedUuid = nodeSelectedUuid;
        }
    }

}
