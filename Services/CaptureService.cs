using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SnapLine.Services
{
    public static class CaptureService
    {
        [DllImport("user32.dll")]
        public static extern bool SetProcessDPIAware();

        static CaptureService()
        {
            try
            {
                SetProcessDPIAware();
            }
            catch
            {
                // Fallback si la API no esta disponible
            }
        }

        /// <summary>
        /// Captura la pantalla completa (incluyendo configuraciones multimonitor).
        /// </summary>
        public static Bitmap CaptureEntireScreen()
        {
            Rectangle bounds = SystemInformation.VirtualScreen;
            Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(bounds.Left, bounds.Top, 0, 0, bounds.Size, CopyPixelOperation.SourceCopy);
            }

            return bitmap;
        }

        /// <summary>
        /// Recorta una region rectangular del bitmap completo y la guarda en disco como PNG.
        /// </summary>
        public static void SaveCroppedRegion(Bitmap fullScreenBitmap, Rectangle cropRect, string destinationPath)
        {
            if (cropRect.Width <= 0 || cropRect.Height <= 0)
                throw new ArgumentException("El area seleccionada no es valida.");

            // Asegurar que el rectangulo no sobrepase los limites del bitmap
            int x = Math.Max(0, cropRect.X);
            int y = Math.Max(0, cropRect.Y);
            int width = Math.Min(cropRect.Width, fullScreenBitmap.Width - x);
            int height = Math.Min(cropRect.Height, fullScreenBitmap.Height - y);

            if (width <= 0 || height <= 0) return;

            Rectangle safeRect = new Rectangle(x, y, width, height);

            using (Bitmap cropped = fullScreenBitmap.Clone(safeRect, fullScreenBitmap.PixelFormat))
            {
                cropped.Save(destinationPath, ImageFormat.Png);
            }
        }
    }
}
