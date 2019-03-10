using System;
using System.Collections;
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
                // TODO This is a kluge for not getting a valid typeName here.
                this.typeName = "BLOB";
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
            if (!typeName.Equals("BLOB")) {
                info.Append(tab).Append(value);
                info.Append(NL);
            } else {
                byte[] iconBytes = null;
                iconBytes = (byte[])value;
                info.Append(tab).Append(iconBytes.GetLength(0)).AppendLine(" bytes");
                string dump = HexDump.HexDump.Dump(iconBytes);
                dump = HexDump.HexDump.indentLines(dump, tab);
                info.Append(dump);
            }
            return info.ToString();
        }

        /// <summary>
        /// Returns an information string for the value for this CSPBrushParam.
        /// </summary>
        /// <param name="tab">Prefix for each line, typically "  " or similar.</param>
        /// <returns></returns>
        public string getValueAsString(string tab) {
            if (!typeName.Equals("BLOB")) {
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
            if (!typeName.Equals("BLOB")) {
                if (value.Equals(param.value)) {
                    return true;
                } else {
                    return false;
                }
            } else {
                byte[] iconBytes = (byte[])value; ;
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
