using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using Portafotos.Services;
using Portafotos.Windows;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace Portafotos
{
    public partial class App : Application
    {
        private Mutex _mutex;
        private StorageService _storageService;
        private ClotheslineWindow _clotheslineWindow;
        private HotKeyManager _hotKeyManager;
        private NotifyIcon _notifyIcon;
        private bool _isSnipping = false;
        private System.Windows.Threading.DispatcherTimer _topEdgeDetectorTimer;
        private DateTime? _topEdgeHoverStart = null;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Prevenir multiples instancias
            const string mutexName = "Portafotos_App_SingleInstance_Mutex";
            bool isNewInstance;
            _mutex = new Mutex(true, mutexName, out isNewInstance);
            if (!isNewInstance)
            {
                MessageBox.Show("Portafotos ya se está ejecutando en segundo plano.", "Portafotos", MessageBoxButton.OK, MessageBoxImage.Information);
                Shutdown();
                return;
            }

            _storageService = new StorageService();

            // La ventana de la cuerda se crea pero permanece OCULTA al inicio
            _clotheslineWindow = new ClotheslineWindow(_storageService);

            SetupTrayIcon();
            SetupHotKeys();
            SetupTopEdgeHoverDetection();
        }

        private void SetupHotKeys()
        {
            _hotKeyManager = new HotKeyManager();
            _hotKeyManager.HotKeyPressed += OnGlobalHotKeyPressed;
            _hotKeyManager.Start();
        }

        private void SetupTopEdgeHoverDetection()
        {
            _topEdgeDetectorTimer = new System.Windows.Threading.DispatcherTimer();
            _topEdgeDetectorTimer.Interval = TimeSpan.FromMilliseconds(100);
            _topEdgeDetectorTimer.Tick += (s, e) =>
            {
                // Posicion absoluta del cursor del raton en pantalla
                System.Drawing.Point cursor = System.Windows.Forms.Cursor.Position;

                // Si el raton esta en el borde superior de la pantalla (Y <= 2 px)
                if (cursor.Y <= 2)
                {
                    if (_topEdgeHoverStart == null)
                    {
                        _topEdgeHoverStart = DateTime.Now;
                    }
                    else if ((DateTime.Now - _topEdgeHoverStart.Value).TotalMilliseconds >= 1000)
                    {
                        _topEdgeHoverStart = null; // Reiniciar para no disparar en bucle

                        if (_clotheslineWindow != null && _clotheslineWindow.Visibility != Visibility.Visible)
                        {
                            _clotheslineWindow.ShowAndSlideIn();
                        }
                    }
                }
                else
                {
                    _topEdgeHoverStart = null;
                }
            };
            _topEdgeDetectorTimer.Start();
        }

        private void SetupTrayIcon()
        {
            _notifyIcon = new NotifyIcon();
            _notifyIcon.Text = "Portafotos - Tendedero de Capturas";
            _notifyIcon.Icon = GenerateAppIcon();
            _notifyIcon.Visible = true;

            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add("📷 Nueva captura (Impr Pant)", null, (s, e) => TriggerCapture());
            menu.Items.Add("🧺 Ver tendedero de fotos", null, (s, e) => _clotheslineWindow.ShowAndSlideIn());
            menu.Items.Add("📁 Abrir carpeta de capturas", null, (s, e) => OpenCapturesFolder());
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("❌ Salir de Portafotos", null, (s, e) => ExitApplication());

            _notifyIcon.ContextMenuStrip = menu;
            _notifyIcon.DoubleClick += (s, e) => _clotheslineWindow.ShowAndSlideIn();
        }

        private Icon GenerateAppIcon()
        {
            using (Bitmap bmp = new Bitmap(32, 32))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    g.Clear(Color.Transparent);

                    // Tarjeta polaroid blanca
                    g.FillRectangle(Brushes.White, 3, 5, 26, 23);
                    g.DrawRectangle(new Pen(Color.FromArgb(160, 160, 160), 1), 3, 5, 26, 23);

                    // Foto interior azul
                    g.FillRectangle(Brushes.DodgerBlue, 6, 8, 20, 15);

                    // Pinza de madera arriba
                    g.FillRectangle(Brushes.SandyBrown, 13, 1, 6, 8);
                    g.DrawRectangle(Pens.SaddleBrown, 13, 1, 6, 8);
                }
                return Icon.FromHandle(bmp.GetHicon());
            }
        }

        private void OnGlobalHotKeyPressed()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                TriggerCapture();
            }));
        }

        private void TriggerCapture()
        {
            if (_isSnipping) return;

            _isSnipping = true;

            try
            {
                // Capturar la pantalla completa
                Bitmap screenBmp = CaptureService.CaptureEntireScreen();

                // Abrir el selector de area con la pantalla congelada
                SnipperOverlayWindow snipper = new SnipperOverlayWindow(screenBmp, _storageService, (filePath) =>
                {
                    // Al completar el recorte, se agrega a la cuerda y se despliega la cuerda
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        _storageService.AddCapture(filePath);
                    }
                    _clotheslineWindow.ShowAndSlideIn();
                });

                snipper.Closed += (s, e) =>
                {
                    _isSnipping = false;
                };

                snipper.Show();
                snipper.Activate();
                snipper.Focus();
            }
            catch (Exception ex)
            {
                _isSnipping = false;
                MessageBox.Show("Error al iniciar captura: " + ex.Message, "Portafotos", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenCapturesFolder()
        {
            try
            {
                if (Directory.Exists(_storageService.CapturesDirectory))
                {
                    Process.Start("explorer.exe", _storageService.CapturesDirectory);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al abrir carpeta: " + ex.Message);
            }
        }

        private void ExitApplication()
        {
            if (_topEdgeDetectorTimer != null)
            {
                _topEdgeDetectorTimer.Stop();
            }

            if (_hotKeyManager != null)
            {
                _hotKeyManager.Dispose();
            }

            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
            }

            if (_mutex != null)
            {
                try
                {
                    _mutex.ReleaseMutex();
                }
                catch
                {
                }
            }
            Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            ExitApplication();
            base.OnExit(e);
        }
    }
}
