using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Utils {
    /// <summary>
    /// Based on code from https://www.codeproject.com/Articles/4544/Insert-Plain-Text-and-Images-into-RichTextBox-at-R
    /// </summary>
    public static class RTFUtils {
        /* RTF HEADER
		 * ----------
		 * 
		 * \rtf[N]		- For text to be considered to be RTF, it must be enclosed in this tag.
		 *				  rtf1 is used because the RichTextBox conforms to RTF Specification
		 *				  version 1.
		 * \ansi		- The character set.
		 * \ansicpg[N]	- Specifies that unicode characters might be embedded. ansicpg1252
		 *				  is the default used by Windows.
		 * \deff[N]		- The default font. \deff0 means the default font is the first font
		 *				  found.
		 * \deflang[N]	- The default language. \deflang1033 specifies US English.
		 * */
        private const string RTF_HEADER = @"{\rtf1\ansi\ansicpg1252\deff0\deflang1033";

        /* RTF DOCUMENT AREA
		 * -----------------
		 * 
		 * \viewkind[N]	- The type of view or zoom level.  \viewkind4 specifies normal view.
		 * \uc[N]		- The number of bytes corresponding to a Unicode character.
		 * \pard		- Resets to default paragraph properties
		 * \cf[N]		- Foreground color.  \cf1 refers to the color at index 1 in
		 *				  the color table
		 * \f[N]		- Font number. \f0 refers to the font at index 0 in the font
		 *				  table.
		 * \fs[N]		- Font size in half-points.
		 * */
        private const string RTF_DOCUMENT_PRE = @"\viewkind4\uc1\pard\cf1\f0\fs20";
        private const string RTF_DOCUMENT_POST = @"\cf0\fs17}";
        private const string RTF_IMAGE_POST = @"}";

        // Ensures that the metafile maintains a 1:1 aspect ratio
        private const int MM_ISOTROPIC = 7;

        // Allows the x-coordinates and y-coordinates of the metafile to be adjusted
        // independently
        private const int MM_ANISOTROPIC = 8;

        // Represents an unknown font family
        private const string FF_UNKNOWN = "UNKNOWN";

        // The number of hundredths of millimeters (0.01 mm) in an inch
        // For more information, see GetImagePrefix() method.
        private const int HMM_PER_INCH = 2540;

        // The number of twips in an inch
        // For more information, see GetImagePrefix() method.
        private const int TWIPS_PER_INCH = 1440;

        // Specifies the flags/options for the unmanaged call to the GDI+ method
        // Metafile.EmfToWmfBits().
        private enum EmfToWmfBitsFlags {
            // Use the default conversion
            EmfToWmfBitsFlagsDefault = 0x00000000,
            // Embedded the source of the EMF metafiel within the resulting WMF
            // metafile
            EmfToWmfBitsFlagsEmbedEmf = 0x00000001,
            // Place a 22-byte header in the resulting WMF file.  The header is
            // required for the metafile to be considered placeable.
            EmfToWmfBitsFlagsIncludePlaceable = 0x00000002,
            // Don't simulate clipping by using the XOR operator.
            EmfToWmfBitsFlagsNoXORClip = 0x00000004
        };

        /// <summary>
        /// Returns the RTF string representing the given Image wrapped in a Windows
        /// Format Metafile. Even though Microsoft discourages the use of a WMF,
        /// the RichTextBox (and even MS Word), wraps an image in a WMF before inserting
        /// the image into a document.  The WMF is attached in HEX format (a string of
        /// HEX numbers).
        /// 
        /// The RTF Specification v1.6 says that you should be able to insert bitmaps,
        /// .jpegs, .gifs, .pngs, and Enhanced Metafiles (.emf) directly into an RTF
        /// document without the WMF wrapper. This works fine with MS Word,
        /// however, when you don't wrap images in a WMF, WordPad and
        /// RichTextBoxes simply ignore them.  Both use the riched20.dll or msfted.dll.
        /// </summary>
        /// <param name="control">The control used to get the Graphics.</param>
        /// <param name="image"></param>
        public static string imageRtf(Control control, Image image) {
            StringBuilder rtf = new StringBuilder();
            // Append the RTF header
            rtf.Append(RTF_HEADER);
            // Create the font table using the RichTextBox's current font and append
            // it to the RTF string
            rtf.Append(getFontTable(control.Font));
            // Create the image control string and append it to the RTF string
            rtf.Append(getImagePrefix(control, image));
            // Create the Windows Metafile and append its bytes in HEX format
            rtf.Append(getRtfImage(control, image));
            // Close the RTF image control string
            rtf.Append(RTF_IMAGE_POST);
            // Close the RTF control string
            // (KE: Was missing in original and leads to unmatched braces)
            rtf.Append(RTF_IMAGE_POST);

            return rtf.ToString();
        }

        /// <summary>
        /// Creates the RTF control string that describes the image being inserted.
        /// This description (in this case) specifies that the image is an
        /// MM_ANISOTROPIC metafile, meaning that both X and Y axes can be scaled
        /// independently.  The control string also gives the images current dimensions,
        /// and its target dimensions, so if you want to control the size of the
        /// image being inserted, this would be the place to do it. The prefix should
        /// have the form ...
        /// 
        /// {\pict\wmetafile8\picw[A]\pich[B]\picwgoal[C]\pichgoal[D]
        /// 
        /// where ...
        /// 
        /// A	= current width of the metafile in hundredths of millimeters (0.01mm)
        ///		= Image Width in Inches * Number of (0.01mm) per inch
        ///		= (Image Width in Pixels / Graphics Context's Horizontal Resolution) * 2540
        ///		= (Image Width in Pixels / Graphics.DpiX) * 2540
        /// 
        /// B	= current height of the metafile in hundredths of millimeters (0.01mm)
        ///		= Image Height in Inches * Number of (0.01mm) per inch
        ///		= (Image Height in Pixels / Graphics Context's Vertical Resolution) * 2540
        ///		= (Image Height in Pixels / Graphics.DpiX) * 2540
        /// 
        /// C	= target width of the metafile in twips
        ///		= Image Width in Inches * Number of twips per inch
        ///		= (Image Width in Pixels / Graphics Context's Horizontal Resolution) * 1440
        ///		= (Image Width in Pixels / Graphics.DpiX) * 1440
        /// 
        /// D	= target height of the metafile in twips
        ///		= Image Height in Inches * Number of twips per inch
        ///		= (Image Height in Pixels / Graphics Context's Horizontal Resolution) * 1440
        ///		= (Image Height in Pixels / Graphics.DpiX) * 1440
        ///	
        /// </summary>
        /// <remarks>
        /// The Graphics Context's resolution is simply the current resolution at which
        /// windows is being displayed.  Normally it's 96 dpi, but instead of assuming
        /// I just added the code.
        /// 
        /// According to Ken Howe at pbdr.com, "Twips are screen-independent units
        /// used to ensure that the placement and proportion of screen elements in
        /// your screen application are the same on all display systems."
        /// 
        /// Units Used
        /// ----------
        /// 1 Twip = 1/20 Point
        /// 1 Point = 1/72 Inch
        /// 1 Twip = 1/1440 Inch
        /// 
        /// 1 Inch = 2.54 cm
        /// 1 Inch = 25.4 mm
        /// 1 Inch = 2540 (0.01)mm
        /// </remarks>
        /// <param name="control">The control used to get the Graphics.</param>
        /// <param name="image"></param>
        /// <returns></returns>
        private static string getImagePrefix(Control control, Image image) {
            StringBuilder rtf = new StringBuilder();
            // Get the DPI from the Control
            float[] dpi = getDpi(control);
            float xDpi = dpi[0];
            float yDpi = dpi[1];
            // Get the DPI from the image DPI
            //float xDpi = image.HorizontalResolution;
            //float yDpi = image.VerticalResolution;
            // Calculate the current width of the image in (0.01)mm
            int picw = (int)Math.Round((image.Width / xDpi) * HMM_PER_INCH);
            // Calculate the current height of the image in (0.01)mm
            int pich = (int)Math.Round((image.Height / yDpi) * HMM_PER_INCH);
            // Calculate the target width of the image in twips
            int picwgoal = (int)Math.Round((image.Width / xDpi) * TWIPS_PER_INCH);
            // Calculate the target height of the image in twips
            int pichgoal = (int)Math.Round((image.Height / yDpi) * TWIPS_PER_INCH);
            // Append values to RTF string
            rtf.Append(@"{\pict\wmetafile8");
            rtf.Append(@"\picw");
            rtf.Append(picw);
            rtf.Append(@"\pich");
            rtf.Append(pich);
            rtf.Append(@"\picwgoal");
            rtf.Append(picwgoal);
            rtf.Append(@"\pichgoal");
            rtf.Append(pichgoal);
            rtf.Append(" ");
            return rtf.ToString();
        }

        /// <summary>
        /// Get the horizonal and vertcal DPI for the given control.
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
        /// Use the EmfToWmfBits function in the GDI+ specification to convert a 
        /// Enhanced Metafile to a Windows Metafile
        /// </summary>
        /// <param name="hEmf">
        /// A handle to the Enhanced Metafile to be converted
        /// </param>
        /// <param name="bufferSize">
        /// The size of the buffer used to store the Windows Metafile bits returned
        /// </param>
        /// <param name="buffer">
        /// An array of bytes used to hold the Windows Metafile bits returned
        /// </param>
        /// <param name="mappingMode">
        /// The mapping mode of the image.  This control uses MM_ANISOTROPIC.
        /// </param>
        /// <param name="_flags">
        /// Flags used to specify the format of the Windows Metafile returned
        /// </param>
        [DllImportAttribute("gdiplus.dll")]
        private static extern uint GdipEmfToWmfBits(IntPtr hEmf, uint bufferSize,
                byte[] buffer, int mappingMode, EmfToWmfBitsFlags
            flags);

        /// <summary>
        /// Wraps the image in an Enhanced Metafile by drawing the image onto the
        /// graphics context, then converts the Enhanced Metafile to a Windows
        /// Metafile, and finally appends the bits of the Windows Metafile in HEX
        /// to a string and returns the string.
        /// </summary>
        /// <param name="control">The control used to get the Graphics.</param>
        /// <param name="image"></param>
        /// <returns>
        /// A string containing the bits of a Windows Metafile in HEX
        /// </returns>
        private static string getRtfImage(Control control, Image image) {

            StringBuilder rtf = null;

            // Used to store the enhanced metafile
            MemoryStream ms = null;

            // Used to create the metafile and draw the image
            Graphics graphics = null;

            // The enhanced metafile
            Metafile metaFile = null;

            // Handle to the device context used to create the metafile
            IntPtr hdc;

            try {
                rtf = new StringBuilder();
                ms = new MemoryStream();

                // Get a graphics context from the RichTextBox
                using (graphics = control.CreateGraphics()) {

                    // Get the device context from the graphics context
                    hdc = graphics.GetHdc();

                    // Create a new Enhanced Metafile from the device context
                    metaFile = new Metafile(ms, hdc);

                    // Release the device context
                    graphics.ReleaseHdc(hdc);
                }

                // Get a graphics context from the Enhanced Metafile
                using (graphics = Graphics.FromImage(metaFile)) {

                    // Draw the image on the Enhanced Metafile
                    graphics.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height));

                }

                // Get the handle of the Enhanced Metafile
                IntPtr hEmf = metaFile.GetHenhmetafile();

                // A call to EmfToWmfBits with a null buffer return the size of the
                // buffer need to store the WMF bits.  Use this to get the buffer
                // size.
                uint bufferSize = GdipEmfToWmfBits(hEmf, 0, null, MM_ANISOTROPIC,
                    EmfToWmfBitsFlags.EmfToWmfBitsFlagsDefault);

                // Create an array to hold the bits
                byte[] buffer = new byte[bufferSize];

                // A call to EmfToWmfBits with a valid buffer copies the bits into the
                // buffer an returns the number of bits in the WMF.  
                uint convertedSize = GdipEmfToWmfBits(hEmf, bufferSize, buffer, MM_ANISOTROPIC,
                    EmfToWmfBitsFlags.EmfToWmfBitsFlagsDefault);

                // Append the bits to the RTF string
                for (int i = 0; i < buffer.Length; ++i) {
                    rtf.Append(String.Format("{0:X2}", buffer[i]));
                }

                return rtf.ToString();
            } finally {
                if (graphics != null)
                    graphics.Dispose();
                if (metaFile != null)
                    metaFile.Dispose();
                if (ms != null)
                    ms.Close();
            }
        }

        /// <summary>
        /// Appends the text to the RichTextBox.  Moves the caret to the end
        /// of the RichTextBox's text then sets rtb.selectedRtf.
        /// </summary>
        /// <remarks>
        /// NOTE: The image is inserted wherever the caret is at the time of the call,
        /// and if any text is selected, that text is replaced.
        /// </remarks>
        /// <param name="rtb">The RichTextBox.</param>
        /// <param name="text">The string to insert.</param>
        public static void appendRtb(RichTextBox rtb, string text) {
            // Move carret to the end of the text
            rtb.Select(rtb.TextLength, 0);
            rtb.SelectedRtf = text;
        }

        /// <summary>
        /// Inserts the text into the RichTextBox. The image is inserted
        /// wherever the caret is at the time of the call,
        /// and if any text is selected, that text is replaced.
        /// </summary>
        /// <param name="rtb">The RichTextBox.</param>
        /// <param name="text">The string to insert.</param>
        public static void insertRtb(RichTextBox rtb, string text) {
            rtb.SelectedRtf = text;
        }

#region RTF Helpers

        /// <summary>
        /// Creates a font table from a font object.  When an Insert or Append 
        /// operation is performed a font is either specified or the default font
        /// is used.  In any case, on any Insert or Append, only one font is used,
        /// thus the font table will always contain a single font.  The font table
        /// should have the form ...
        /// 
        /// {\fonttbl{\f0\[FAMILY]\fcharset0 [FONT_NAME];}
        /// </summary>
        /// <param name="font"></param>
        /// <returns></returns>
        private static string getFontTable(Font font) {

                StringBuilder sb = new StringBuilder();

                // Append table control string
                sb.Append(@"{\fonttbl{\f0");
                sb.Append(@"\");

#if false
            // KE: Not implementing this and apparently is not needed:
            // If the font's family corresponds to an RTF family, append the
            // RTF family name, else, append the RTF for unknown font family.
            if (rtfFontFamily.Contains(_font.FontFamily.Name))
                    sb.Append(rtfFontFamily[_font.FontFamily.Name]);
                else
                    sb.Append(rtfFontFamily[FF_UNKNOWN]);
#else
            sb.Append(font.FontFamily.Name);
#endif

            // \fcharset specifies the character set of a font in the font table.
            // 0 is for ANSI.
            sb.Append(@"\fcharset0 ");

                // Append the name of the font
                sb.Append(font.Name);

                // Close control string
                sb.Append(@";}}");

                return sb.ToString();
            }

#if false
            /// <summary>
            /// Creates a font table from the RtfColor structure.  When an Insert or Append
            /// operation is performed, _textColor and _backColor are either specified
            /// or the default is used.  In any case, on any Insert or Append, only three
            /// colors are used.  The default color of the RichTextBox (signified by a
            /// semicolon (;) without a definition), is always the first color (index 0) in
            /// the color table.  The second color is always the text color, and the third
            /// is always the highlight color (color behind the text).  The color table
            /// should have the form ...
            /// 
            /// {\colortbl ;[TEXT_COLOR];[HIGHLIGHT_COLOR];}
            /// 
            /// </summary>
            /// <param name="_textColor"></param>
            /// <param name="_backColor"></param>
            /// <returns></returns>
            private string GetColorTable(RtfColor _textColor, RtfColor _backColor) {

                StringBuilder _colorTable = new StringBuilder();

                // Append color table control string and default font (;)
                _colorTable.Append(@"{\colortbl ;");

                // Append the text color
                _colorTable.Append(rtfColor[_textColor]);
                _colorTable.Append(@";");

                // Append the highlight color
                _colorTable.Append(rtfColor[_backColor]);
                _colorTable.Append(@";}\n");

                return _colorTable.ToString();
            }

            /// <summary>
            /// Called by overrided RichTextBox.Rtf accessor.
            /// Removes the null character from the RTF.  This is residue from developing
            /// the control for a specific instant messaging protocol and can be ommitted.
            /// </summary>
            /// <param name="_originalRtf"></param>
            /// <returns>RTF without null character</returns>
            private string RemoveBadChars(string _originalRtf) {
                return _originalRtf.Replace("\0", "");
            }

#endif
#endregion
    }
}
