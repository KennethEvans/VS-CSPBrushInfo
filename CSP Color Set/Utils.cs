using System;
using System.Drawing;
using System.Windows.Forms;

namespace Utils {
    public static class Utils {
        public static string LF = System.Environment.NewLine;

        /// <summary>
        /// Error message.
        /// </summary>
        /// <param name="msg"></param>
        public static void errMsg(string msg) {
            MessageBox.Show(msg, "Error");
        }

        /// <summary>
        /// Exception message.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="ex"></param>
        public static void excMsg(string msg, Exception ex) {
            MessageBox.Show(msg += LF + "Exception: " + ex + LF
            + ex.Message, "Exception");
        }

        /// <summary>
        /// Warning message.
        /// </summary>
        /// <param name="msg"></param>
        public static void warnMsg(string msg) {
            MessageBox.Show(msg, "Warning");
        }

        /// <summary>
        /// Information message.
        /// </summary>
        /// <param name="msg"></param>
        public static void infoMsg(string msg) {
            MessageBox.Show(msg, "Information");
        }

        /// <summary>
        /// Gets an adjjusted width and height for the given width and height
        /// by multiplying them by the current DPI for the given form divided
        /// by 96, the standard DPI.
        /// </summary>
        /// <param name="form"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static Size getDpiAdjustedSize(Form form, Size size) {
            Graphics g = form.CreateGraphics();
            float dpiX = g.DpiX;
            float dpiY = g.DpiY;
            g.Dispose();
            return new Size((int)(dpiX * size.Width / 96F),
                (int)(dpiY * size.Height / 96F));
        }
    }
}
