using KEUtils.About;
using KEUtils.ScrolledHTML2;
using KEUtils.Utils;
using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace CSPUtils {
    public partial class ColorSetForm : Form {
        public static readonly String NL = Environment.NewLine;
        private static ScrolledRichTextDialog verboseDlg;
        private static ScrolledRichTextDialog statusDlg;
        private static ScrolledHTMLDialog2 overviewDlg;

        private Font defaultFont;
        private Font imageFont;
        private Font courierFont;

        // These dialogs are for debugging and will be added dynamically if true
        private bool useVerbose = false;
        private bool useStatus = false;
        private ToolStripMenuItem showVerboseOutputToolStripMenuItem;
        private ToolStripMenuItem showStatusOutputToolStripMenuItem;

        public ColorSetForm() {
            InitializeComponent();

            defaultFont = textBoxInfo.Font;
            imageFont = new System.Drawing.Font(defaultFont.Name, 10.0f);
            courierFont = new Font("Courier New", 10.0f, FontStyle.Bold);

            ColorSetForm app = (ColorSetForm)FindForm().FindForm();

            if (useVerbose) {
                showVerboseOutputToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                showVerboseOutputToolStripMenuItem.Name = "showVerboseOutputToolStripMenuItem";
                showVerboseOutputToolStripMenuItem.Size = new System.Drawing.Size(497, 54);
                showVerboseOutputToolStripMenuItem.Text = "Show Verbose Output...";
                showVerboseOutputToolStripMenuItem.Click += new System.EventHandler(this.OnShowVerboseClick);
                fileToolStripMenuItem.DropDownItems.Add(showVerboseOutputToolStripMenuItem);
                verboseDlg = new ScrolledRichTextDialog(
                    Utils.getDpiAdjustedSize(app, new Size(600, 400)), "");
                verboseDlg.Text = "Verbose Output" + NL;
            }
            if (useStatus) {
                showStatusOutputToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                showStatusOutputToolStripMenuItem.Name = "showStatusOutputToolStripMenuItem";
                showStatusOutputToolStripMenuItem.Size = new System.Drawing.Size(497, 54);
                showStatusOutputToolStripMenuItem.Text = "Show Status Output...";
                showStatusOutputToolStripMenuItem.Click += new System.EventHandler(this.OnShowStatusClick);
                fileToolStripMenuItem.DropDownItems.Add(showStatusOutputToolStripMenuItem);
                statusDlg = new ScrolledRichTextDialog(
                   Utils.getDpiAdjustedSize(app, new Size(600, 400)), "");
                statusDlg.Text = "Status Output" + NL;
                statusDlg.textBox.Font = courierFont;
                appendLineStatusInfo($"{"FileName",-32} {"c1",3} {"c2",3} {"c3",3} {"c4",3} {"c5",3} {"c6",5} {"c7",3}");
            }
        }

        /// <summary>
        /// Main method to process a file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="bytes"></param>
        private void processFile(string fileName, byte[] bytes) {
            //int nBytes = bytes.Length;
            string fileNameShort = Path.GetFileNameWithoutExtension(fileName);

            //setInfoCursorToEnd();
            //int caretPos = textDlg.textBox.Text.Length;
            //appendLineVerboseInfo($"caretPos={caretPos}");
            appendLineInfo(fileName);
            appendLineVerboseInfo(fileName);
            int c1 = -1;
            int c2 = -1;
            int c3 = -1;
            int c4 = -1;
            int c5 = -1;
            int c6 = -1;
            int c7 = -1;
            int c8;
            int c9;
            try {
                using (BinaryReader reader = new BinaryReader(new MemoryStream(bytes))) {
                    // Tag (SLCC)
                    string tag = readAscii(reader, 4);
                    if (!tag.Equals("SLCC")) {
                        appendLineVerboseInfo($"Unknown tag {tag}. Should be SLCC");
                        return;
                    } else {
                        appendLineVerboseInfo($"tag: {tag}");
                    }
                    // Next short
                    c1 = reader.ReadInt16();
                    string check = c1 != 256 ? "!!!" : "";
                    appendLineVerboseInfo($"c1={c1}{check}");
                    c2 = reader.ReadInt16();
                    appendLineVerboseInfo($"c2={c2}");
                    c3 = reader.ReadInt16();
                    check = c3 != 0 ? "!!!" : "";
                    appendLineVerboseInfo($"c3={c3}{check}");
                    // Number of characters in first name
                    int nChars1 = reader.ReadInt16();
                    appendLineVerboseInfo($"nChars1={nChars1}");
                    // First name
                    string name1 = readAscii(reader, nChars1);
                    appendLineVerboseInfo($"name1={name1}");
                    c4 = reader.ReadInt32();
                    check = c4 != 0 ? "!!!" : "";
                    appendLineVerboseInfo($"c4={c4}{check}");
                    // Number of characters in second name
                    int nChars2 = reader.ReadInt16();
                    appendLineVerboseInfo($"nChars2={nChars2}");
                    // Second name
                    string name2 = readAscii(reader, nChars2);
                    if (name2.Contains("\0")) {
                        name2 = null;
                        appendLineVerboseInfo($"name2=null");
                        // There was no name2, backtrack over reading it
                        reader.BaseStream.Position -= nChars2 + 2;
                    } else {
                        appendLineVerboseInfo($"name2={name2}");
                    }
                    c5 = reader.ReadInt32();
                    check = c5 != 4 ? "!!!" : "";
                    appendLineVerboseInfo($"c5={c5}{check}");
                    int nColors = reader.ReadInt32();
                    appendLineVerboseInfo($"nColors={nColors}");
                    c6 = reader.ReadInt16();
                    appendLineVerboseInfo($"c6={c6}");
                    c7 = reader.ReadInt16();
                    check = c7 != 0 ? "!!!" : "";
                    appendLineVerboseInfo($"c7={c7} {check}");
                    appendLineInfo($"{name1} nColors={nColors}");
                    for (int i = 0; i < nColors; i++) {
                        appendLineVerboseInfo($"Color {i}");
                        c8 = reader.ReadInt32();
                        appendLineVerboseInfo($"    c8={c8}");
                        int red = reader.ReadByte();
                        int green = reader.ReadByte();
                        int blue = reader.ReadByte();
                        int alpha = reader.ReadByte();
                        appendLineVerboseInfo($"    #{alpha:X2}{red:X2}{green:X2}{blue:X2} " +
                            $"{red} {blue} {green} {alpha}");
                        c9 = reader.ReadInt32();
                        check = c9 != 0 && c9 != 1 ? "!!!" : "";
                        appendLineVerboseInfo($"    c9={c9} {check}");
                        insertImage(red, green, blue, alpha, textBoxInfo, fileName);
                        // This is necessary to have the rest on the line after
                        // the image be correct
                        textBoxInfo.SelectionFont = defaultFont;
                        if (c9 != 0) {
                            // Process name
                            int nName = reader.ReadInt16();
                            appendLineVerboseInfo($"    nName={nName}");
                            String colorName = readUnicode(reader, nName);
                            // Remove possible null at end of colorName
                            int len = colorName.Length;
                            if (colorName[len - 1] == 0) {
                                colorName = colorName.Substring(0, len - 1);
                            }
                            appendLineVerboseInfo($"    colorName={colorName}");
                            appendLineInfo($" #{alpha:X2}{red:X2}{green:X2}{blue:X2} \t" +
                                $"{red:D3} {blue:D3} {green:D3} {alpha:D3} \t{colorName:-20}");
                        } else {
                            appendLineInfo($" #{alpha:X2}{red:X2}{green:X2}{blue:X2} \t" +
                                $"{red:D3} {blue:D3} {green:D3} {alpha:D3}");
                        }
                    }
                    appendLineStatusInfo($"{fileNameShort,-32} {c1:D3} {c2:D3} {c3:D3} {c4:D3} {c5:D3} {c6:D5} {c7:D3}");
                }
                appendLineInfo("");
                appendLineVerboseInfo("");
                //// Set to the start of this block
                //textDlg.textBox.Focus();
                //textDlg.textBox.Select(caretPos, 0);
            } catch (Exception ex) {
                appendLineStatusInfo($"{fileNameShort,-32} {c1:D3} {c2:D3} {c3:D3} {c4:D3} {c5:D3} {c6:D5} {c7:D3}");
                string msg = $"Process error for {fileName}";
                appendLineInfo(msg + NL + ex + NL);
                appendLineVerboseInfo(msg + NL + ex + NL);
                Utils.excMsg(msg, ex);
            }
        }

        #region BinaryReader Methods
        string readAscii(BinaryReader reader, int nChars) {
            byte[] bytes = reader.ReadBytes(nChars);
            return Encoding.ASCII.GetString(bytes);
        }

        string readUnicode(BinaryReader reader, int nChars) {
            byte[] bytes = reader.ReadBytes(nChars);
            return Encoding.Unicode.GetString(bytes);
        }

        byte readByte(BinaryReader reader) {
            return reader.ReadByte();
        }
        #endregion

        #region Insert Image Methods
        /// <summary>
        /// Get the horizonal and vertcal DPI for a given control.
        /// </summary>
        /// <param name="control">The control used to get the Graphics.</param>
        /// <returns>float {DpiX, DpiY}.</returns>
        private static float[] getDpi(Control control) {
            // Get the horizontal and vertical resolutions for the Control
            using (Graphics graphics = control.CreateGraphics()) {
                return new float[] { graphics.DpiX, graphics.DpiY };
            }
        }

        /// <summary>
        /// /// Inserts an image with a background corresponding to the given
        /// red, green, blue values using Unicode characters.
        /// </summary>
        /// <param name="red"></param>
        /// <param name="green"></param>
        /// <param name="blue"></param>
        /// <param name="alpha"></param>
        public void insertImage(int red, int green, int blue, int alpha,
            RichTextBox textBox, string fileName) {
            string symbol;
            Color color = Color.FromArgb(alpha, red, green, blue);
            if (alpha == 0) {
                //symbol = "\u25EA";  // Lower diagonal black
                //symbol = "\u1F67F";  // Reverse chekerboard (Not in default char set)
                symbol = "\u25A8";  // Right to left fill
            } else {
                symbol = "\u25A0";
            }
            textBox.SelectionStart = textBox.TextLength;
            textBox.SelectionLength = 0;
            textBox.SelectionColor = color;
            textBox.SelectionFont = imageFont;
            textBox.AppendText(symbol);
            textBox.SelectionColor = textBox.ForeColor;
        }

        /// <summary>
        /// /// Inserts an image with a background corresponding to the given
        /// red, green, blue values using RTFUtils. This means it inserts RTF
        /// control sequences containing a Windows metafile of the image.
        /// </summary>
        /// <param name="red"></param>
        /// <param name="green"></param>
        /// <param name="blue"></param>
        /// <param name="alpha"></param>
        public void insertImage1(int red, int green, int blue, int alpha,
            RichTextBox textBox, string fileName) {
            float sizeInches = .125f;
            float[] dpi = getDpi(textBox);
            int w = (int)(dpi[0] * sizeInches);
            int h = (int)(dpi[1] * sizeInches);
            Bitmap bm;
            if (alpha == 0) {
                bm = getTransparentBitmap(w, h);
            } else {
                bm = new Bitmap(w, h);
                using (Graphics g = Graphics.FromImage(bm)) {
                    g.Clear(Color.FromArgb(alpha, red, green, blue));
                }
            }
            // RTfUtils uses this to set the size (in = points/72)
            bm.SetResolution(w / sizeInches, h / sizeInches);
            try {
                String rtf = RTFUtils.imageRtf(textBox, bm);
                if (!String.IsNullOrEmpty(rtf)) {
                    RTFUtils.appendRtb(textBox, rtf);
                }
            } catch (Exception ex) {
                string msg = $"Error processing image for {fileName}";
                appendLineInfo(msg + NL + ex + NL);
                appendLineVerboseInfo(msg + NL + ex + NL);
                ////Utils.excMsg(msg, ex);
            }
        }

        /// <summary>
        /// /// Inserts an image with a background corresponding to the given
        /// red, green, blue values using the Clipboard.
        /// </summary>
        /// <param name="red"></param>
        /// <param name="green"></param>
        /// <param name="blue"></param>
        /// <param name="alpha"></param>
        public void insertImage2(int red, int green, int blue, int alpha,
            RichTextBox textBox, string fileName) {
            // Specify the size in inches (in = points/72)
            float sizeInches = .125f;
            float[] dpi = getDpi(textBox);
            int w = (int)(dpi[0] * sizeInches);
            int h = (int)(dpi[1] * sizeInches);
            Bitmap bm;
            if (alpha == 0) {
                bm = getTransparentBitmap(w, h);
            } else {
                bm = new Bitmap(w, h);
                using (Graphics g = Graphics.FromImage(bm)) {
                    g.Clear(Color.FromArgb(alpha, red, green, blue));
                }
            }

            // Copy the bitmap to the clipboard
            //Clipboard.SetData(DataFormats.Bitmap, bm);

            //bool done = false;
            //while (!done) {
            //    try {
            //        Clipboard.SetData(DataFormats.Bitmap, bm);
            //        done = true;
            //    } catch (Exception) {

            //    }
            //}

            Clipboard.SetDataObject(bm, false, 100, 100);
            DataFormats.Format format = DataFormats.GetFormat(DataFormats.Bitmap);
            if (textBox.CanPaste(format)) {
                textBox.Paste(format);
            }
            if (bm != null) bm.Dispose();
        }

        Bitmap getTransparentBitmap(int w, int h) {
            int squareSize = w / 4;
            int squareSize2 = 2 * squareSize;
            Bitmap bm = new Bitmap(w, h);
            using (Graphics g = Graphics.FromImage(bm)) {
                g.Clear(Color.White);
                for (int i = 0; i < w; i += squareSize2) {
                    for (int j = 0; j < h; j += squareSize2) {
                        g.FillRectangle(Brushes.Silver,
                            i, j, squareSize, squareSize);
                        g.FillRectangle(Brushes.LightSteelBlue,
                            i + squareSize, j + squareSize, squareSize, squareSize);
                    }
                }
            }
            return bm;
        }
        #endregion

        #region Append Methods
        private void appendStatusInfo(string info) {
            if (statusDlg == null) return;
            statusDlg.textBox.AppendText(info);
        }

        private void appendLineStatusInfo(string info) {
            appendStatusInfo(info + NL);
        }

        private void appendVerboseInfo(string info) {
            if (verboseDlg == null) return;
            verboseDlg.textBox.AppendText(info);
        }

        private void appendLineVerboseInfo(string info) {
            appendVerboseInfo(info + NL);
        }

        private void appendInfo(string info) {
            textBoxInfo.AppendText(info);
        }

        private void appendLineInfo(string info) {
            appendInfo(info + NL);
        }

        private void setInfoCursorToEnd() {
            // Set cursor to end
            textBoxInfo.Focus();
            textBoxInfo.Select(textBoxInfo.Text.Length, 0);
        }
        #endregion

        #region OnClick Methods
        private void OnQuitClick(object sender, EventArgs e) {
            Close();
        }

        private void OnAboutClick(object sender, EventArgs e) {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Image image = null;
            try {
                image = Image.FromFile(@".\Help\CSPColorSet.256x256.png");
            } catch (Exception ex) {
                Utils.excMsg("Failed to get AboutBox image", ex);
            }
            AboutBox dlg = new AboutBox(image, assembly);
            dlg.ShowDialog();
        }

        private void OnOverviewClick(object sender, EventArgs e) {
#if false
            // Create, show, or set visible the overview dialog as appropriate
            if (overviewDlg == null) {
                ColorSetForm app = (ColorSetForm)FindForm().FindForm();
                overviewDlg = new ScrolledHTMLDialog2(
                    Utils.getDpiAdjustedSize(app, new Size(800, 600)),
                    "Overview", @"Help\Overview.html");
                overviewDlg.Show();
            } else {
                overviewDlg.Visible = true;
            }
#else
            Utils.infoMsg("Not implemented yet");
#endif
        }

        private void OnClearClick(object sender, EventArgs e) {
            textBoxInfo.Text = "";
        }

        private void OnOpenClick(object sender, EventArgs e) {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Color Set|*.cls";
            dlg.Title = "Select color set to process";
            dlg.Multiselect = true;
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                if (dlg.FileNames == null) {
                    Utils.warnMsg("Failed to open files to process");
                    return;
                }
                Cursor.Current = Cursors.WaitCursor;
                string[] fileNames = dlg.FileNames;
                foreach (string fileName in fileNames) {
                    try {
                        byte[] bytes = File.ReadAllBytes(fileName);
                        processFile(fileName, bytes);
                    } catch (Exception ex) {
                        string msg = "Failed to open " + fileName;
                        Utils.excMsg(msg, ex);
                    }
                }
                Cursor.Current = Cursors.Default;
            }
        }

        private void OnShowVerboseClick(object sender, EventArgs e) {
            if (verboseDlg == null) {
                Utils.errMsg("textDlg is null");
                return;
            } else {
                verboseDlg.Visible = true;
            }
        }

        private void OnShowStatusClick(object sender, EventArgs e) {
            if (statusDlg == null) {
                Utils.errMsg("statusDlg is null");
                return;
            } else {
                statusDlg.Visible = true;
            }
        }
#endregion
    }
}
