using System;
using System.Collections;
using System.IO;
using System.Text;

namespace CSPBrushInfo {
    class CSPBrushParam : IComparer {
        public readonly String NL = Environment.NewLine;
        private string name;
        private string typeName;
        private object value;
        private bool err;
        private string errorMessage;

        public CSPBrushParam(string name, string typeName, object value) {
            err = false;
            errorMessage = "No error";
            this.name = name;
            if (typeName != null && typeName.Length > 0) {
                this.typeName = typeName;
            } else {
                // TODO This is a kluge for not getting a valid typeName here
                if (value.GetType() == typeof(byte[])) {
                    this.typeName = "(BLOB)";
                } else {
                    this.typeName = "(" + value.GetType().ToString() + ")";
                }
            }
            this.value = value;
        }

        /// <summary>
        /// Returns an information string for this CSPBrushParam.
        /// </summary>
        /// <param name="tab">Prefix for each line, typically "  " or similar.</param>
        /// <returns></returns>
        public String info(string tab) {
            StringBuilder info;
            info = new StringBuilder();
            info.Append(name);
            info.Append(" [").Append(typeName).Append("]");
            info.Append(NL);
            if (!typeName.Contains("BLOB")) {
                info.Append(tab).Append(value);
                info.Append(NL);
            } else {
                byte[] iconBytes = null;
                iconBytes = (byte[])value;
                info.Append(tab).Append(iconBytes.GetLength(0)).AppendLine(" bytes");
                string dump = HexDump.HexDump.Dump(iconBytes);
                dump = HexDump.HexDump.indentLines(dump, tab);
                info.Append(dump);
                string interpret = interpretBlob(iconBytes, tab);
                info.Append(interpret);
            }
            return info.ToString();
        }

        /// <summary>
        /// Interprets the blob as an Effector.  Returns a blank string if the
        /// name does not contain Effector.  Converts to BigEndian.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="tab">Prefix for each line, typically "  " or similar.</param>
        /// <returns></returns>
        string interpretBlob(byte[] bytes, string tab) {
            string info = "";
            // Only do Effectors
            if (!name.ToLower().Contains("effector")) return info;
            info = tab + "Interpreted:" + NL;
            int nBytes = bytes.Length;
            int nBytesRead = 0;
            int nControlPoints;
            int iVal;
            byte[] charData;
            double pointX, pointY;
            try {
                using (BinaryReader reader = new BinaryReader(new MemoryStream(bytes))) {
                    // Get first 11 integers
                    for (int i = 0; i < 11; i++) {
                        if (i == nBytes) return info;
                        charData = reader.ReadBytes(4);
                        nBytesRead += 4;
                        Array.Reverse(charData);
                        iVal = BitConverter.ToInt32(charData, 0);
                        if (i % 4 == 0) {
                            if (i != 0) info += NL;
                            info += tab;
                        }
                        info += String.Format("{0,6}", iVal);
                    }
                    if (!info.EndsWith(NL)) info += NL;
                    if (nBytesRead == nBytes) return info;
                    // Get next 3 bytes
                    nControlPoints = 0;
                    for (int i = 0; i < 3; i++) {
                        charData = reader.ReadBytes(4);
                        nBytesRead += 4;
                        Array.Reverse(charData);
                        iVal = BitConverter.ToInt32(charData, 0);
                        if (i == 1) nControlPoints = iVal;
                        if (i == 0) info += tab;
                        info += String.Format("{0,6}", iVal);
                    }
                    info += NL;
                    // Get control Points
                    for (int i = 0; i < nControlPoints; i++) {
                        charData = reader.ReadBytes(8);
                        nBytesRead += 8;
                        Array.Reverse(charData);
                        pointX = BitConverter.ToDouble(charData, 0);
                        charData = reader.ReadBytes(8);
                        nBytesRead += 8;
                        Array.Reverse(charData);
                        pointY = BitConverter.ToDouble(charData, 0);
                        info += tab + String.Format(
                            "Control Point {0}: {1,8:#0.0000} {2,8:#0.0000}",
                            i + 1, pointX, pointY) + NL;
                    }
                    if (nBytesRead == nBytes) return info;
                    // Get next 3 bytes
                    nControlPoints = 0;
                    for (int i = 0; i < 3; i++) {
                        charData = reader.ReadBytes(4);
                        nBytesRead += 4;
                        Array.Reverse(charData);
                        iVal = BitConverter.ToInt32(charData, 0);
                        if (i == 1) nControlPoints = iVal;
                        if (i == 0) info += tab;
                        info += String.Format("{0,6}", iVal);
                    }
                    info += NL;
                    // Get control Points
                    for (int i = 0; i < nControlPoints; i++) {
                        charData = reader.ReadBytes(8);
                        nBytesRead += 8;
                        Array.Reverse(charData);
                        pointX = BitConverter.ToDouble(charData, 0);
                        charData = reader.ReadBytes(8);
                        nBytesRead += 8;
                        Array.Reverse(charData);
                        pointY = BitConverter.ToDouble(charData, 0);
                        info += tab + String.Format(
                            "Control Point {0}: {1,8:#0.0000} {2,8:#0.0000}",
                            i + 1, pointX, pointY) + NL;
                    }
                    return info;
                }
            } catch (Exception ex) {
                info += "Error reading data for " + name + NL;
                info += ex + NL;
                return info;
            }
        }

        /// <summary>
        /// Returns an information string for the value for this CSPBrushParam.
        /// </summary>
        /// <param name="tab">Prefix for each line, typically "  " or similar.</param>
        /// <returns></returns>
        public string getValueAsString(string tab) {
            if (!typeName.Contains("BLOB")) {
                return value.ToString() + NL;
            } else {
                byte[] iconBytes = null;
                iconBytes = (byte[])value;
                StringBuilder info;
                info = new StringBuilder();
                info.AppendLine(iconBytes.GetLength(0) + " bytes");
                string dump = HexDump.HexDump.Dump(iconBytes);
                dump = HexDump.HexDump.indentLines(dump, tab);
                info.Append(dump);
                string interpret = interpretBlob(iconBytes, tab);
                info.Append(interpret);
                return info.ToString();
            }
        }

        /// <summary>
        /// Returns if the given CSPBrushParam is equal to this one.
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public bool equals(CSPBrushParam param) {
            if (!this.name.Equals(param.name) || !this.typeName.Equals(param.typeName)) {
                return false;
            }
            if (!this.typeName.Contains("BLOB")) {
                if (this.value.Equals(param.value)) {
                    return true;
                } else {
                    return false;
                }
            }
            byte[] iconBytes = (byte[])this.value; ;
            byte[] iconBytesParam = (byte[])param.value;
            int len = iconBytes.GetLength(0);
            int lenParam = iconBytesParam.GetLength(0);
            if (len != lenParam) {
                return false;
            }
            for (int i = 0; i < len; i++) {
                if (iconBytes[i] != iconBytesParam[i]) {
                    return false;
                }
            }
            return true;
        }

        public int Compare(CSPBrushParam x, CSPBrushParam y) {
            return ((CSPBrushParam)x).name.CompareTo(((CSPBrushParam)y).name);
        }

        public int Compare(object x, object y) {
            throw new NotImplementedException();
        }

        public string Name { get => name; set => name = value; }
        public string TypeName { get => typeName; set => typeName = value; }
        public object Value { get => value; set => this.value = value; }
        public bool Err { get => err; set => err = value; }
        public string ErrorMessage { get => errorMessage; set => errorMessage = value; }
    }
}
