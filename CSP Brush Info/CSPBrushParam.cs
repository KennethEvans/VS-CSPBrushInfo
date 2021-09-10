using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace CSPBrushInfo {
    class CSPBrushParam : IComparer {
        public static readonly String NL = Environment.NewLine;
        private string name;
        private string typeName;
        private object value;
        private bool err;
        private string errorMessage;

        /// <summary>
        /// Returns the value as a string.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="typeName"></param>
        /// <param name="value"></param>
        public CSPBrushParam(string name, string typeName, object value) {
            err = false;
            errorMessage = "No error";
            this.name = name;
            if (typeName != null && typeName.Length > 0) {
                this.typeName = typeName;
            } else {
                // TODO This is a kludge for not getting a valid typeName here
                if (value.GetType() == typeof(byte[])) {
                    this.typeName = "(BLOB)";
                } else {
                    this.typeName = "(" + value.GetType().ToString() + ")";
                }
            }
            this.value = value;
        }

        /// <summary>
        /// Returns an information string for this CSPBrushParam. This consists
        /// of the Name and the result of getValueByString().
        /// </summary>
        /// <param name="tab">Prefix for each line, typically "  " or similar.</param>
        /// <returns></returns>
        public String info(string tab) {
            StringBuilder info;
            info = new StringBuilder();
            info.Append(name);
            info.Append(" [").Append(typeName).Append("]");
            info.Append(NL);
            info.Append(getValueAsString(tab));
            return info.ToString();
        }

        /// <summary>
        /// Interprets the blob.  Returns a blank string if the
        /// name does not contain one of the items it can handle.  Converts to 
        /// BigEndian.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="tab">Prefix for each line, typically "  " or similar.</param>
        /// <returns></returns>
        string interpretBlob(byte[] bytes, string tab) {
            string info = "";
            if (name.ToLower().Contains("effector")) {
                // Effectors
                info = interpretEffector(bytes, tab);
            } else if (name.Contains("PatternImageArray")) {
                info = interpretImage(bytes, tab);
            } else if (name.Contains("TextureImage")) {
                info = interpretImage(bytes, tab);
            }
            // Could possibly be usd for BrushSprayFixedPointArray
            // But don't have any brushes with that or see it in the interface
            return info;
        }

        /// <summary>
        /// Interprets the blob as an Effector. Converts to BigEndian.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="tab">Prefix for each line, typically "  " or similar.</param>
        /// <returns></returns>
        string interpretEffector(byte[] bytes, string tab) {
            string info = "";
            info = tab + "Interpreted:" + NL;
            int nBytes = bytes.Length;
            int nBytesRead = 0;
            int nControlPoints;
            int nHeader;
            int iVal;
            double pointX, pointY;
            bool pressureUsed = false, tiltUsed = false, velocityUsed = false, randomUsed = false;
            try {
                using (BinaryReader reader = new BinaryReader(new MemoryStream(bytes))) {
                    // Get first 10 or 11 integers, depending on nHeader
                    if (nBytesRead == nBytes) return info + NL;
                    info += tab + "  ";
                    nHeader = iVal = readInteger(reader);
                    nBytesRead += 4;
                    info += String.Format("nHeader={0} ", iVal);

                    if (nBytesRead == nBytes) return info + NL;
                    iVal = readInteger(reader);
                    nBytesRead += 4;
                    info += String.Format("undetermined2={0} ", iVal);

                    if (nBytesRead == nBytes) return info + NL;
                    iVal = readInteger(reader);
                    nBytesRead += 4;
                    info += String.Format("usedFlag={0} ", iVal);
                    if ((iVal & 0x10) != 0) pressureUsed = true;
                    if ((iVal & 0x20) != 0) tiltUsed = true;
                    if ((iVal & 0x40) != 0) velocityUsed = true;
                    if ((iVal & 0x80) != 0) randomUsed = true;

                    if (nBytesRead == nBytes) return info + NL;
                    iVal = readInteger(reader);
                    nBytesRead += 4;
                    info += String.Format("pmin={0} ", iVal);
                    // Write warning if unexpected nHeader
                    if (nHeader < 40 || nHeader > 44) {
                        info += "        [!!! Can only interpret nHeader = 40 or 44. Expect errors]";
                    }
                    info += NL + tab + "    (pressureUsed=" + pressureUsed
                        + " tiltUsed=" + tiltUsed
                        + " velocityUsed=" + velocityUsed
                        + " randomUsed=" + randomUsed
                        + ")";

                    if (nBytesRead == nBytes) return info + NL;
                    info += NL + tab + "  ";
                    iVal = readInteger(reader);
                    nBytesRead += 4;
                    info += String.Format("tMin={0} ", iVal);

                    if (nBytesRead == nBytes) return info + NL;
                    iVal = readInteger(reader);
                    nBytesRead += 4;
                    info += String.Format("vMax={0} ", iVal);

                    if (nBytesRead == nBytes) return info + NL;
                    iVal = readInteger(reader);
                    nBytesRead += 4;
                    info += String.Format("rMin={0} ", iVal);

                    if (nBytesRead == nBytes) return info + NL;
                    iVal = readInteger(reader);
                    nBytesRead += 4;
                    info += String.Format("undetermined8={0} ", iVal);

                    if (nBytesRead == nBytes) return info + NL;
                    info += NL + tab + "  ";
                    iVal = readInteger(reader);
                    nBytesRead += 4;
                    info += String.Format("undetermined9={0} ", iVal);

                    if (nBytesRead == nBytes) return info + NL;
                    iVal = readInteger(reader);
                    nBytesRead += 4;
                    info += String.Format("undetermined10={0} ", iVal);

                    if (nBytesRead == nBytes) return info+ NL;
                    if (nHeader > 40) {
                        // Older files with 40-byte header don't have this
                        iVal = readInteger(reader);
                        nBytesRead += 4;
                        info += String.Format("tMax={0} ", iVal);
                    } else {
                        info += String.Format("tMax=unspecified");
                    }
                    if (nBytesRead == nBytes) return info + NL;
                    info += NL;

                    // Read 3 control point integers
                    nControlPoints = 0;
                    info += tab + "Control Points 1: ";
                    iVal = readInteger(reader);
                    nBytesRead += 4;
                    info += String.Format("undetermined1={0} ", iVal);

                    if (nBytesRead == nBytes) return info + NL;
                    nControlPoints = iVal = readInteger(reader);
                    nBytesRead += 4;
                    info += String.Format("nPoints={0} ", iVal);

                    if (nBytesRead == nBytes) return info + NL;
                    iVal = readInteger(reader);
                    nBytesRead += 4;
                    info += String.Format("undetermined3={0} ", iVal);
                    if (nBytesRead == nBytes) return info + NL;
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
                    info += tab + "Control Points 2: ";
                    iVal = readInteger(reader);
                    nBytesRead += 4;
                    info += String.Format("undetermined1={0} ", iVal);

                    if (nBytesRead == nBytes) return info + NL;
                    nControlPoints = iVal = readInteger(reader);
                    nBytesRead += 4;
                    info += String.Format("nPoints={0} ", iVal);

                    if (nBytesRead == nBytes) return info + NL;
                    iVal = readInteger(reader);
                    nBytesRead += 4;
                    info += String.Format("undetermined3={0} ", iVal);
                    if (nBytesRead == nBytes) return info + NL;
                    info += NL;

                    // Get control Points
                    PointF[] controlPoints = new PointF[nControlPoints];
                    for (int i = 0; i < nControlPoints; i++) {
                        pointX = readDouble(reader);
                        nBytesRead += 8;
                        pointY = readDouble(reader);
                        controlPoints[i] = new PointF((float)pointX, (float)pointY);
                        nBytesRead += 8;
                        info += tab + "  " + String.Format(
                            "Point {0}: {1,8:#0.0000} {2,8:#0.0000}",
                            i + 1, pointX, pointY) + NL;
                    }
                    return info;
                }
            } catch (Exception ex) {
                info += "Error interpreting binary data for " + name + NL;
                info += ex + NL;
                return info;
            }
        }

        /// <summary>
        /// Get an array of Images for the effectors
        /// </summary>
        /// <param name="tab">Prefix for each line, typically "  " or similar.</param>
        /// <returns></returns>
        public List<Bitmap> getEffectorImages(string tab) {
            if (!typeName.Contains("BLOB") ||
                !name.ToLower().Contains("effector")) {
                return null;
            }
            List<Bitmap> imageArray = new List<Bitmap>();
            Bitmap image;
            byte[] bytes = null;
            bytes = (byte[])value;

            int nBytes = bytes.Length;
            int nBytesRead = 0;
            int nControlPoints;
            int nHeader;
            int iVal;
            double pointX, pointY;
            try {
                using (BinaryReader reader = new BinaryReader(new MemoryStream(bytes))) {
                    // Get first 10 or 11 integers, depending on nHeader
                    nHeader = iVal = readInteger(reader); // 1
                    nBytesRead += 4;

                    // Read the rest of the header (9 or 10 values depending on nHeader)
                    int nVals = nHeader / 4;
                    for (int i = 0; i < nVals - 1; i++) {
                        if (nBytesRead == nBytes) return imageArray;
                        iVal = readInteger(reader);
                        nBytesRead += 4;
                    }

                    // Read 3 control point integers
                    nControlPoints = 0;
                    if (nBytesRead == nBytes) return imageArray;
                    iVal = readInteger(reader);
                    nBytesRead += 4;

                    if (nBytesRead == nBytes) return imageArray;
                    nControlPoints = iVal = readInteger(reader);
                    nBytesRead += 4;

                    if (nBytesRead == nBytes) return imageArray;
                    iVal = readInteger(reader);
                    nBytesRead += 4;

                    // Get control Points
                    PointF[] controlPoints = new PointF[nControlPoints];
                    for (int i = 0; i < nControlPoints; i++) {
                        pointX = readDouble(reader);
                        nBytesRead += 8;
                        pointY = readDouble(reader);
                        nBytesRead += 8;
                        controlPoints[i] = new PointF((float)pointX, (float)pointY);
                    }
                    // Create the image
                    image = getEffectorImage(controlPoints);
                    if (image != null) {
                        imageArray.Add(image);
                    }

                    if (nBytesRead == nBytes) return imageArray;
                    // Read 3 control point integers
                    nControlPoints = 0;
                    if (nBytesRead == nBytes) return imageArray;
                    iVal = readInteger(reader);
                    nBytesRead += 4;

                    if (nBytesRead == nBytes) return imageArray;
                    nControlPoints = iVal = readInteger(reader);
                    nBytesRead += 4;

                    if (nBytesRead == nBytes) return imageArray;
                    iVal = readInteger(reader);
                    nBytesRead += 4;

                    // Get control Points
                    controlPoints = new PointF[nControlPoints];
                    for (int i = 0; i < nControlPoints; i++) {
                        pointX = readDouble(reader);
                        nBytesRead += 8;
                        pointY = readDouble(reader);
                        nBytesRead += 8;
                        controlPoints[i] = new PointF((float)pointX, (float)pointY);
                    }
                    // Create the image
                    image = getEffectorImage(controlPoints);
                    if (image != null) {
                        imageArray.Add(image);
                    }
                }
            } catch (Exception ex) {
                Utils.Utils.excMsg("Error creating effector images for " + name,
                    ex);
            }
            return imageArray;
        }

        /// <summary>
        /// Interprets the blob as a material. Useful for BrushPatternImageArray,
        /// TextureImage, possibly BrushSprayFixedPointArray.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="tab">Prefix for each line, typically "  " or similar.</param>
        /// <returns></returns>
        string interpretImage(byte[] bytes, string tab) {
            string info = "";
            info = tab + "Interpreted:" + NL;
            int nBytes = bytes.Length;
            int nBytesRead = 0;
            int iVal, nItems;
            string identPlus, imageName, ident;
            try {
                using (BinaryReader reader = new BinaryReader(new MemoryStream(bytes))) {
                    // Get first 2 integers
                    if (nBytesRead == nBytes) return info;
                    info += tab + tab;
                    iVal = readInteger(reader);  // Seems to always be 8
                    nBytesRead += 4;
                    //info += String.Format("int1={0} ", iVal);
                    if (nBytesRead == nBytes) return info;
                    nItems = iVal = readInteger(reader);
                    nBytesRead += 4;
                    info += String.Format("nItems={0} ", iVal) + NL;

                    // Loop over items
                    int iVal1, nName, nSize;
                    long length = reader.BaseStream.Length;
                    long position;
                    for (int i = 0; i < nItems; i++) {
                        identPlus = ident = imageName = "<Not found>";
                        // This iVal1 is the size of the BLOB minus the first two integers
                        nSize = iVal1 = readInteger(reader);
                        nName = readInteger(reader);
                        position = reader.BaseStream.Position;
                        identPlus = readName(reader, nName);
                        //info += "After identPlus [" + iVal1 + "," + nName + "] "
                        //    + "position=" + position + "/" + length + NL;
                        while (true) {
                            nName = -1;
                            iVal1 = readInteger(reader);
                            position = reader.BaseStream.Position;
                            //info += "After iVal1 [" + iVal1 + "," + nName + "] " 
                            //    + "position=" + position + "/" + length + NL;
                            if (iVal1 == 0) break;
                            if (position == length) break;
                            nName = readInteger(reader);
                            position = reader.BaseStream.Position;
                            if (position == length) break;
                            if (iVal1 == 1) {
                                ident = readName(reader, nName);
                            } else {
                                // In most cases iVal1 = 2, but have also seen 3
                                imageName = readName(reader, nName);
                            }
                            position = reader.BaseStream.Position;
                            //info += "After readName [" + iVal1 + "," + nName + "] " 
                            //    + "position=" + position + "/" + length + NL;
                        }
                        position = reader.BaseStream.Position;
                        //info += "At end [" + iVal1 + "," + nName + "] "
                        //    + "position=" + position + "/" + length + NL;
                        info += tab + tab + imageName + NL
                            + tab + tab + tab + "Original Path=" + identPlus + NL
                            + tab + tab + tab + "Catalog Path=" + ident + NL;
                    }
                    return info;
                }
            } catch (Exception ex) {
                info += "Error interpreting binary data for " + name + NL;
                info += ex + NL;
                return info;
            }
        }

        int readInteger(BinaryReader reader) {
            byte[] charData = reader.ReadBytes(4);
            Array.Reverse(charData);
            return BitConverter.ToInt32(charData, 0);
        }

        int readShort(BinaryReader reader) {
            byte[] charData = reader.ReadBytes(2);
            Array.Reverse(charData);
            return BitConverter.ToInt16(charData, 0);
        }

        double readDouble(BinaryReader reader) {
            byte[] charData = reader.ReadBytes(8);
            Array.Reverse(charData);
            return BitConverter.ToDouble(charData, 0);
        }

        string readName(BinaryReader reader, int nChars) {
            string sVal;
            sVal = readChars(reader, nChars);
            return sVal;
        }

        string readChars(BinaryReader reader, int nChars) {
            byte[] bytes = reader.ReadBytes(nChars);
            return Encoding.Unicode.GetString(bytes);
        }

        byte readByte(BinaryReader reader) {
            return reader.ReadByte();
        }

        string byteInfo(byte bVal) {
            char cVal = bVal < 32 ? '·' : (char)bVal;
            return String.Format("{0} [{1:X2}h] {2}", bVal, bVal, cVal);
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
                byte[] bytes = (byte[])value;
                StringBuilder info;
                info = new StringBuilder();
                info.AppendLine(bytes.GetLength(0) + " bytes");
                string dump = HexDump.HexDump.Dump(bytes);
                dump = HexDump.HexDump.indentLines(dump, tab);
                info.Append(dump);
                string interpret = interpretBlob(bytes, tab);
                info.Append(interpret);
                return info.ToString();
            }
        }

#if GENERATE_RTF
        /// <summary>
        /// Creates an RTF string for an image that shows the effector graph.
        /// </summary>
        /// <param name="controlPoints">The control points for the effector.</param>
        /// <returns></returns>
        public string createEffectorImage(PointF[] controlPoints) {
            string imageString = null;
            string rtfString = null;
            // !!!! Replace with getEffectorImage !!!
            int margin = 10;
            int w = 256, h = 256;
            using (Bitmap bm = new Bitmap(w, h)) {
                using (Graphics g = Graphics.FromImage(bm)) {
                    g.Clear(Color.Red);
                    g.Clear(Color.FromArgb(255, 191, 191, 191));
                    GraphicsPath gp = new GraphicsPath();
                    gp.AddLine(0, 0, 1, 0);
                    gp.AddLine(0, 1, 1, 1);
                    gp.AddLine(1, 1, 0, 1);
                    gp.AddLine(0, 0, 0, 0);
                    // Scale it
                    Matrix m = new System.Drawing.Drawing2D.Matrix();
                    m.Scale(w - 2 * margin, h - 2 * margin);
                    m.Translate(margin, margin);
                    gp.Transform(m);
                    // Draw it
                    using (Pen pen = new Pen(Color.Black)) {
                        g.DrawPath(pen, gp);
                    }
                }
                // Convert to bytes
                using (MemoryStream ms = new MemoryStream()) {
                    bm.Save(ms, ImageFormat.Png);
                    byte[] bytes = ms.ToArray();
                    string byteString = BitConverter.ToString(bytes, 0);
                    if (!String.IsNullOrEmpty(byteString)) {
                        imageString = byteString.Replace("-", string.Empty);
                    }
                }
            }
#if false
            // Test
            string fileName = @"C:\Users\evans\Pictures\Icon Images\BlueMouse.96x96.png";
            byte[] bytes1 = System.IO.File.ReadAllBytes(fileName);
            string byteString1 = BitConverter.ToString(bytes1, 0);
            if (!String.IsNullOrEmpty(byteString1)) {
                imageString = byteString1.Replace("-", string.Empty);
            }
#endif

            // Convert to an RTF string
            if (!String.IsNullOrEmpty(imageString)) {
                rtfString = @"{\pict\pngblip"
                    + @"\picw" + w.ToString() + @"\pich" + h.ToString()
                    + @"\picwgoal" + w.ToString() + @"\pichgoal" + h.ToString()
                    + @" " + imageString + "}";
            }
            return rtfString;
        }
#endif

        /// <summary>
        /// Creates an image that shows the effector graph for the given control points.
        /// </summary>
        /// <param name="controlPoints">The control points for the effector.</param>
        /// <returns></returns>
        public Bitmap getEffectorImage(PointF[] controlPoints) {
            // DEBUG
            //return new Bitmap(@"C:\Users\evans\Pictures\Icon Images\BlueMouse.256x256.png");
            int margin = 10;
            int w = 256, h = w;
            float scale = w - 2 * margin;
            Bitmap bm = new Bitmap(w, h);
            // RTfUtils uses this to set the size
            float sizeInches = 1.0f;
            bm.SetResolution(w / sizeInches, h / sizeInches);
            using (Graphics g = Graphics.FromImage(bm)) {
                // Scale it so we can work in [0,1] coordinate axes,
                // with y increasing up
                Matrix m = new System.Drawing.Drawing2D.Matrix();
                m.Scale(1f, -1f);
                m.Translate(0, -h);
                m.Translate(margin, margin);
                m.Scale(scale, scale);
                g.Transform = m;
                g.Clear(Color.FromArgb(191, 191, 191));
                // Grid lines
                using (Pen pen = new Pen(Color.White, 1 / scale)) {
                    for (int i = 1; i < 4; i++) {
                        g.DrawLine(pen, .25f * i, 0, .25f * i, 1);
                        g.DrawLine(pen, 0, .25f * i, 1, .25f * i);
                    }
                }
                // Axes
                using (Pen pen = new Pen(Color.Black, 1 / scale)) {
                    g.DrawLine(pen, 0, 0, 1, 0);
                    g.DrawLine(pen, 1, 0, 1, 1);
                    g.DrawLine(pen, 1, 1, 0, 1);
                    g.DrawLine(pen, 0, 1, 0, 0);
                }
                // Control points
                using (Brush brush = new SolidBrush(Color.FromArgb(140, 152, 252))) {
                    int width = 9;  // Should be odd
                    int off = (width - 1) / 2;
                    foreach (PointF point in controlPoints) {
                        g.FillRectangle(brush,
                            new RectangleF(point.X - off / scale, point.Y - off / scale,
                            width / scale, width / scale));
                    }
                }
                // Control point lines
                using (Pen pen = new Pen(Color.FromArgb(140, 152, 252), 2 / scale)) {
                    float xPrev = controlPoints[0].X;
                    float yPrev = controlPoints[0].Y;
                    for (int i = 1; i < controlPoints.Length; i++) {
                        g.DrawLine(pen, xPrev, yPrev, controlPoints[i].X, controlPoints[i].Y);
                        xPrev = controlPoints[i].X;
                        yPrev = controlPoints[i].Y;
                    }
                }
                // Curves
                PointF[] ccp;
                using (Pen pen = new Pen(Color.Black, 2 / scale)) {
                    int nPoints = controlPoints.Length;
                    if (nPoints == 2) {
                        g.DrawLine(pen, controlPoints[0], controlPoints[1]);
                    } else if (nPoints == 3) {
                        ccp = cubicBezier(controlPoints[0], controlPoints[1], controlPoints[2]);
                        g.DrawBezier(pen, ccp[0], ccp[1], ccp[2], ccp[3]);
                    } else if (nPoints > 3) {
                        // First
                        ccp = cubicBezier(controlPoints[0], controlPoints[1],
                            midpoint(controlPoints[1], controlPoints[2]));
                        g.DrawBezier(pen, ccp[0], ccp[1], ccp[2], ccp[3]);
                        // Middle
                        for (int i = 2; i < nPoints - 2; i++) {
                            ccp = cubicBezier(midpoint(controlPoints[i - 1], controlPoints[i]),
                                controlPoints[i],
                                midpoint(controlPoints[i], controlPoints[i + 1]));
                            g.DrawBezier(pen, ccp[0], ccp[1], ccp[2], ccp[3]);
                        }
                        // Last
                        ccp = cubicBezier(midpoint(controlPoints[nPoints - 3], controlPoints[nPoints - 2]),
                            controlPoints[nPoints - 2],
                           controlPoints[nPoints - 1]);
                        g.DrawBezier(pen, ccp[0], ccp[1], ccp[2], ccp[3]);
                    }
                }
            }
            bm.Save(@"C:\Scratch\AAA\" + name + "TestDocument.png", ImageFormat.Png);
            return bm;
        }

        /// <summary>
        /// Calculates the cubic Bezier points corresponding to a quadratic Bezier.
        /// </summary>
        /// <param name="qp0">Quadratic point.</param>
        /// <param name="qp1">Quadratic point.</param>
        /// <param name="qp2">Quadratic point.</param>
        /// <returns>Cubic points as float {cp0, cp1, cp2, cp3}.</returns>
        PointF[] cubicBezier(PointF qp0, PointF qp1, PointF qp2) {
            float fract = 2.0f / 3.0f;
            PointF cp0 = qp0;
            PointF cp1 = new PointF(qp0.X + fract * (qp1.X - qp0.X),
                qp0.Y + fract * (qp1.Y - qp0.Y));
            PointF cp2 = new PointF(qp2.X + fract * (qp1.X - qp2.X),
                qp2.Y + fract * (qp1.Y - qp2.Y));
            PointF cp3 = qp2;
            return new PointF[] { cp0, cp1, cp2, cp3 };
        }

        PointF midpoint(PointF p0, PointF p1) {
            return new PointF(.5f * (p0.X + p1.X), .5f * (p0.Y + p1.Y));
        }


        /// <summary>
        /// Separates an info string in the text part and the RTF for a pict,
        /// which is of the form {\pict....}.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static string[] splitInfo(string info) {
            Regex RTF_PICT_REGX = new Regex("\\{\\\\pict(.*)\\}");
            Match match = RTF_PICT_REGX.Match(info);
            string rtf = "";
            string newInfo = info;
            if (match.Success) {
                int nGroups = match.Groups.Count;
                string value;
                foreach (Group group in match.Groups) {
                    value = group.Value;
                    rtf += value;
                }
                // Remove all the patterns from info
                newInfo = RTF_PICT_REGX.Replace(newInfo, "");
            }
            return new string[] { newInfo, rtf };
        }

        /// <summary>
        /// Returns if the given CSPBrushParam is equal to this one.
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public bool equals(CSPBrushParam param) {
            if (!this.name.Equals(param.name)) {
                return false;
            }
            if (!this.typeName.Contains("BLOB")) {
                if (!this.typeName.Equals(param.typeName)) {
                    return false;
                }
                return this.value.Equals(param.value);
            }
            byte[] bytes = (byte[])this.value;
            byte[] bytesParam = (byte[])param.value;
            int len = bytes.GetLength(0);
            int lenParam = bytesParam.GetLength(0);
            if (len != lenParam) {
                return false;
            }
            for (int i = 0; i < len; i++) {
                if (bytes[i] != bytesParam[i]) {
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
