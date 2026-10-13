using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using SnapLine.Services;
using Point = System.Windows.Point;

namespace SnapLine.Windows
{
    public partial class SnipperOverlayWindow : Window
    {
        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        private readonly Bitmap _fullScreenBitmap;
        private readonly Action<string> _onCaptured;
        private readonly StorageService _storageService;

        private bool _isSelecting = false;
        private Point _startPoint;
        private System.Windows.Rect _currentSelectionRect;

        public SnipperOverlayWindow(Bitmap fullScreenBitmap, StorageService storageService, Action<string> onCaptured)
        {
            InitializeComponent();

            _fullScreenBitmap = fullScreenBitmap;
            _storageService = storageService;
            _onCaptured = onCaptured;

            SetupWindowDimensions();
            DisplayCapturedImage();
        }

        private void SetupWindowDimensions()
        {
            Left = SystemParameters.VirtualScreenLeft;
            Top = SystemParameters.VirtualScreenTop;
            Width = SystemParameters.VirtualScreenWidth;
            Height = SystemParameters.VirtualScreenHeight;

            ScreenImage.Width = Width;
            ScreenImage.Height = Height;

            FullMaskGeometry.Rect = new System.Windows.Rect(0, 0, Width, Height);
        }

        private void DisplayCapturedImage()
        {
            if (_fullScreenBitmap == null) return;

            IntPtr hBitmap = _fullScreenBitmap.GetHbitmap();
            try
            {
                BitmapSource source = Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
                source.Freeze();
                ScreenImage.Source = source;
            }
            finally
            {
                DeleteObject(hBitmap);
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _isSelecting = true;
                _startPoint = e.GetPosition(this);
                CaptureMouse();

                SelectionBorder.Visibility = Visibility.Visible;
                DimensionBadge.Visibility = Visibility.Visible;
                UpdateSelection(e.GetPosition(this));
            }
        }

        private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_isSelecting)
            {
                UpdateSelection(e.GetPosition(this));
            }
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isSelecting)
            {
                _isSelecting = false;
                ReleaseMouseCapture();

                if (_currentSelectionRect.Width >= 10 && _currentSelectionRect.Height >= 10)
                {
                    FinalizeCapture();
                }
                else
                {
                    Close();
                }
            }
        }

        private void UpdateSelection(Point currentPos)
        {
            double x = Math.Min(_startPoint.X, currentPos.X);
            double y = Math.Min(_startPoint.Y, currentPos.Y);
            double w = Math.Abs(_startPoint.X - currentPos.X);
            double h = Math.Abs(_startPoint.Y - currentPos.Y);

            _currentSelectionRect = new System.Windows.Rect(x, y, w, h);

            SelectionMaskGeometry.Rect = _currentSelectionRect;

            SelectionBorder.Margin = new Thickness(x, y, 0, 0);
            SelectionBorder.Width = w;
            SelectionBorder.Height = h;

            double scaleX = Width > 0 ? ((double)_fullScreenBitmap.Width / Width) : 1.0;
            double scaleY = Height > 0 ? ((double)_fullScreenBitmap.Height / Height) : 1.0;

            DimensionText.Text = string.Format("{0} × {1} px", (int)Math.Round(w * scaleX), (int)Math.Round(h * scaleY));

            // Posicionar el badge debajo de la seleccion, o arriba si esta cerca del borde inferior
            double badgeY = (y + h + 8 + 30 > Height) ? (y - 30) : (y + h + 8);
            DimensionBadge.Margin = new Thickness(x, Math.Max(4, badgeY), 0, 0);
        }

        private void FinalizeCapture()
        {
            try
            {
                double scaleX = Width > 0 ? ((double)_fullScreenBitmap.Width / Width) : 1.0;
                double scaleY = Height > 0 ? ((double)_fullScreenBitmap.Height / Height) : 1.0;

                int cropX = (int)Math.Round(_currentSelectionRect.X * scaleX);
                int cropY = (int)Math.Round(_currentSelectionRect.Y * scaleY);
                int cropW = (int)Math.Round(_currentSelectionRect.Width * scaleX);
                int cropH = (int)Math.Round(_currentSelectionRect.Height * scaleY);

                string filePath = _storageService.GenerateNewCaptureFilePath();
                Rectangle cropRect = new Rectangle(cropX, cropY, cropW, cropH);

                CaptureService.SaveCroppedRegion(_fullScreenBitmap, cropRect, filePath);

                Close();
                if (_onCaptured != null)
                {
                    _onCaptured(filePath);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error al guardar captura: " + ex.Message, "SnapLine", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                _isSelecting = false;
                ReleaseMouseCapture();
                Close();
            }
            else if (e.Key == Key.Space || e.Key == Key.Tab)
            {
                _isSelecting = false;
                ReleaseMouseCapture();
                Close();
                if (_onCaptured != null)
                {
                    _onCaptured(null);
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (_fullScreenBitmap != null)
            {
                _fullScreenBitmap.Dispose();
            }
        }
    }
}
