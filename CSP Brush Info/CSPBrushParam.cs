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

        public CSPBrushParam(string name, string typeName) {
            err = false;
            errorMessage = "No error";
            this.name = name;
            this.typeName = typeName;
        }

        public String info() {
            StringBuilder info;
            info = new StringBuilder();
            info.Append(name);
            info.Append(" [").Append(typeName).Append("]");
            info.Append(NL);
            if (!typeName.Equals("BLOB")) {
                info.Append(" ").Append(value);
            } else {
                byte[] iconBytes = null;
                iconBytes = (byte[])value;
                info.Append(" ").Append(iconBytes.GetLength(0)).Append(" bytes");
            }
            info.Append(NL);
            return info.ToString();
        }

        public string getValueAsString() {
            if (!typeName.Equals("BLOB")) {
                return value.ToString();
            } else {
                byte[] iconBytes = null;
                iconBytes = (byte[])value;
                return iconBytes.GetLength(0) + " bytes";
            }
        }

        public bool equalsExceptType(CSPBrushParam param) {
            if (this.name.Equals(param.name) && this.value.Equals(param.value)) {
                return true;
            }
            return false;
        }

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
