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
            bool pressureUsed = false, tiltUsed = false, velocityUsed = false, randomUsed = false;
            try {
                using (BinaryReader reader = new BinaryReader(new MemoryStream(bytes))) {
                    // Get first 11 integers
                    if (nBytesRead == nBytes) return info;
                    info += tab + "  ";
                    iVal = readInteger(reader);
                    nBytesRead += 4;
                    info += String.Format("undetermined1={0} ", iVal);

                    if (nBytesRead == nBytes) return info;
                    iVal = readInteger(reader);
                    nBytesRead += 4;
                    info += String.Format("undetermined2={0} ", iVal);

                    if (nBytesRead == nBytes) return info;
                    iVal = readInteger(reader);
                    nBytesRead += 4;
                    info += String.Format("usedFlag={0} ", iVal);
                    if ((iVal & 0x10) != 0) pressureUsed = true;
                    if ((iVal & 0x20) != 0) tiltUsed = true;
                    if ((iVal & 0x40) != 0) velocityUsed = true;
                    if ((iVal & 0x80) != 0) randomUsed = true;

                    if (nBytesRead == nBytes) return info;
                    iVal = readInteger(reader);
                    nBytesRead += 4;
                    info += String.Format("pmin={0} ", iVal);
                    info += NL + tab + "    (pressureUsed=" + pressureUsed
                        + " tiltUsed=" + tiltUsed
                        + " velocityUsed=" + velocityUsed
                        + " randomUsed=" + randomUsed
                        + ")";

                    if (nBytesRead == nBytes) return info;
                    info += NL + tab + "  ";
                    iVal = readInteger(reader);
                    nBytesRead += 4;
                    info += String.Format("tMin={0} ", iVal);

                    if (nBytesRead == nBytes) return info;
                    iVal = readInteger(reader);
                    nBytesRead += 4;
                    info += String.Format("vMax={0} ", iVal);

                    if (nBytesRead == nBytes) return info;
                    iVal = readInteger(reader);
                    nBytesRead += 4;
                    info += String.Format("rMax={0} ", iVal);

                    if (nBytesRead == nBytes) return info;
                    iVal = readInteger(reader);
                    nBytesRead += 4;
                    info += String.Format("undetermined8={0} ", iVal);

                    if (nBytesRead == nBytes) return info;
                    info += NL + tab + "  ";
                    iVal = readInteger(reader);
                    nBytesRead += 4;
                    info += String.Format("undetermined9={0} ", iVal);

                    if (nBytesRead == nBytes) return info;
                    iVal = readInteger(reader);
                    nBytesRead += 4;
                    info += String.Format("undetermined10={0} ", iVal);

                    if (nBytesRead == nBytes) return info;
                    iVal = readInteger(reader);
                    nBytesRead += 4;
                    info += String.Format("tMax={0} ", iVal);

                    // Read 3 control point integers
                    nControlPoints = 0;
                    if (nBytesRead == nBytes) {
                        info += NL;
                        return info;
                    }
                    info += NL + tab + "Control Points 1: ";
                    iVal = readInteger(reader);
                    nBytesRead += 4;
                    info += String.Format("undetermined1={0} ", iVal);

                    if (nBytesRead == nBytes) return info;
                    iVal = readInteger(reader);
                    nControlPoints = iVal;
                    nBytesRead += 4;
                    info += String.Format("nPoints={0} ", iVal);

                    if (nBytesRead == nBytes) return info;
                    iVal = readInteger(reader);
                    nBytesRead += 4;
                    info += String.Format("undetermined3={0} ", iVal);
                    info += NL;

                    // Get control Points
                    for (int i = 0; i < nControlPoints; i++) {
                        pointX = readDouble(reader);
                        nBytesRead += 8;
                        pointY = readDouble(reader);
                        nBytesRead += 8;
                        info += tab + "  " + String.Format(
                            "Point {0}: {1,8:#0.0000} {2,8:#0.0000}",
                            i + 1, pointX, pointY) + NL;
                    }
                    if (nBytesRead == nBytes) return info;
                    // Read 3 control point integers
                    nControlPoints = 0;
                    if (nBytesRead == nBytes) return info;
                    info += tab + "Control Points 2: ";
                    iVal = readInteger(reader);
                    nBytesRead += 4;
                    info += String.Format("undetermined1={0} ", iVal);

                    if (nBytesRead == nBytes) return info;
                    iVal = readInteger(reader);
                    nControlPoints = iVal;
                    nBytesRead += 4;
                    info += String.Format("nPoints={0} ", iVal);

                    if (nBytesRead == nBytes) return info;
                    iVal = readInteger(reader);
                    nBytesRead += 4;
                    info += String.Format("undetermined3={0} ", iVal);
                    info += NL;

                    // Get control Points
                    for (int i = 0; i < nControlPoints; i++) {
                        pointX = readDouble(reader);
                        nBytesRead += 8;
                        pointY = readDouble(reader);
                        nBytesRead += 8;
                        info += tab + "  " + String.Format(
                            "Point {0}: {1,8:#0.0000} {2,8:#0.0000}",
                            i + 1, pointX, pointY) + NL;
                    }
                    if (nBytesRead == nBytes) return info;
                    return info;
                }
            } catch (Exception ex) {
                info += "Error reading data for " + name + NL;
                info += ex + NL;
                return info;
            }
        }

        int readInteger(BinaryReader reader) {
            byte[] charData = reader.ReadBytes(4);
            Array.Reverse(charData);
            return BitConverter.ToInt32(charData, 0);
        }

        double readDouble(BinaryReader reader) {
            byte[] charData = reader.ReadBytes(8);
            Array.Reverse(charData);
            return BitConverter.ToDouble(charData, 0);
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
