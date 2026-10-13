using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using SnapLine.Models;
using SnapLine.Services;
using Path = System.IO.Path;
using Point = System.Windows.Point;

namespace SnapLine.Windows
{
    public partial class AnnotationEditorWindow : Window
    {
        private readonly CaptureItem _item;
        private readonly StorageService _storageService;

        private Brush _selectedBrush = new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44)); // Rojo por defecto
        private double _selectedThickness = 2.0;

        private bool _isDrawing = false;
        private Point _startPoint;
        private UIElement _previewElement = null;

        private readonly Stack<UIElement> _drawnElements = new Stack<UIElement>();
        private TextBox _activeTextBox = null;

        public AnnotationEditorWindow(CaptureItem item, StorageService storageService)
        {
            InitializeComponent();

            _item = item;
            _storageService = storageService;

            InitializeColorPalette();
            LoadImage();
        }

        private void InitializeColorPalette()
        {
            var colors = new[]
            {
                new { Name = "Rojo", Hex = "#EF4444" },
                new { Name = "Verde", Hex = "#10B981" },
                new { Name = "Azul", Hex = "#3B82F6" },
                new { Name = "Amarillo", Hex = "#F59E0B" },
                new { Name = "Blanco", Hex = "#FFFFFF" },
                new { Name = "Negro", Hex = "#111827" }
            };

            foreach (var col in colors)
            {
                var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(col.Hex));
                brush.Freeze();

                Border border = new Border
                {
                    Width = 24,
                    Height = 24,
                    CornerRadius = new CornerRadius(12),
                    Background = brush,
                    BorderBrush = Brushes.SlateGray,
                    BorderThickness = new Thickness(1.5),
                    Margin = new Thickness(3, 0, 3, 0),
                    Cursor = System.Windows.Input.Cursors.Hand,
                    ToolTip = col.Name,
                    Tag = brush
                };

                border.MouseLeftButtonDown += (s, e) =>
                {
                    _selectedBrush = (Brush)(s as Border).Tag;
                    UpdatePaletteSelection();
                };

                ColorPalettePanel.Children.Add(border);
            }

            UpdatePaletteSelection();
        }

        private void UpdatePaletteSelection()
        {
            foreach (UIElement child in ColorPalettePanel.Children)
            {
                Border b = child as Border;
                if (b != null)
                {
                    if (b.Tag == _selectedBrush)
                    {
                        b.BorderBrush = Brushes.White;
                        b.BorderThickness = new Thickness(3);
                    }
                    else
                    {
                        b.BorderBrush = Brushes.SlateGray;
                        b.BorderThickness = new Thickness(1.5);
                    }
                }
            }
        }

        private void LoadImage()
        {
            if (File.Exists(_item.FilePath))
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.UriSource = new Uri(_item.FilePath, UriKind.Absolute);
                bmp.EndInit();
                bmp.Freeze();

                ScreenshotImage.Source = bmp;
                DrawingCanvas.Width = bmp.PixelWidth;
                DrawingCanvas.Height = bmp.PixelHeight;

                DimensionsLabel.Text = string.Format("Dimensiones: {0} × {1} px", bmp.PixelWidth, bmp.PixelHeight);
            }
        }

        private void StrokeThicknessCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem item = StrokeThicknessCombo.SelectedItem as ComboBoxItem;
            if (item != null && item.Tag != null)
            {
                double thick;
                if (double.TryParse(item.Tag.ToString(), out thick))
                {
                    _selectedThickness = thick;
                }
            }
        }

        private void DrawingCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;

            CommitActiveText();

            _startPoint = e.GetPosition(DrawingCanvas);

            if (TextToolRadio.IsChecked == true)
            {
                StartTextInput(_startPoint);
                return;
            }

            _isDrawing = true;
            DrawingCanvas.CaptureMouse();
        }

        private void DrawingCanvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!_isDrawing) return;

            Point currentPoint = e.GetPosition(DrawingCanvas);

            // Eliminar preview anterior
            if (_previewElement != null)
            {
                DrawingCanvas.Children.Remove(_previewElement);
                _previewElement = null;
            }

            if (RectToolRadio.IsChecked == true)
            {
                _previewElement = CreateRectangle(_startPoint, currentPoint);
            }
            else if (CircleToolRadio.IsChecked == true)
            {
                _previewElement = CreateEllipse(_startPoint, currentPoint);
            }
            else if (ArrowToolRadio.IsChecked == true)
            {
                _previewElement = CreateArrow(_startPoint, currentPoint);
            }

            if (_previewElement != null)
            {
                DrawingCanvas.Children.Add(_previewElement);
            }
        }

        private void DrawingCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isDrawing) return;

            _isDrawing = false;
            DrawingCanvas.ReleaseMouseCapture();

            if (_previewElement != null)
            {
                _drawnElements.Push(_previewElement);
                _previewElement = null;
            }
        }

        private System.Windows.Shapes.Rectangle CreateRectangle(Point p1, Point p2)
        {
            double x = Math.Min(p1.X, p2.X);
            double y = Math.Min(p1.Y, p2.Y);
            double w = Math.Abs(p1.X - p2.X);
            double h = Math.Abs(p1.Y - p2.Y);

            var rect = new System.Windows.Shapes.Rectangle
            {
                Width = Math.Max(1, w),
                Height = Math.Max(1, h),
                Stroke = _selectedBrush,
                StrokeThickness = _selectedThickness,
                Fill = Brushes.Transparent
            };

            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);
            return rect;
        }

        private System.Windows.Shapes.Ellipse CreateEllipse(Point p1, Point p2)
        {
            double x = Math.Min(p1.X, p2.X);
            double y = Math.Min(p1.Y, p2.Y);
            double w = Math.Abs(p1.X - p2.X);
            double h = Math.Abs(p1.Y - p2.Y);

            var el = new System.Windows.Shapes.Ellipse
            {
                Width = Math.Max(1, w),
                Height = Math.Max(1, h),
                Stroke = _selectedBrush,
                StrokeThickness = _selectedThickness,
                Fill = Brushes.Transparent
            };

            Canvas.SetLeft(el, x);
            Canvas.SetTop(el, y);
            return el;
        }

        private System.Windows.Shapes.Path CreateArrow(Point start, Point end)
        {
            PathGeometry geometry = new PathGeometry();
            PathFigure figure = new PathFigure { StartPoint = start, IsClosed = false };

            // Linea principal
            figure.Segments.Add(new LineSegment(end, true));
            geometry.Figures.Add(figure);

            // Cabeza de flecha
            Vector dir = end - start;
            if (dir.Length > 5)
            {
                dir.Normalize();
                Vector normal = new Vector(-dir.Y, dir.X);

                double arrowLength = Math.Max(12, _selectedThickness * 4);
                double arrowWidth = Math.Max(7, _selectedThickness * 2.5);

                Point arrowLeft = end - dir * arrowLength + normal * arrowWidth;
                Point arrowRight = end - dir * arrowLength - normal * arrowWidth;

                PathFigure headFigure = new PathFigure { StartPoint = arrowLeft, IsClosed = true, IsFilled = true };
                headFigure.Segments.Add(new LineSegment(end, true));
                headFigure.Segments.Add(new LineSegment(arrowRight, true));
                geometry.Figures.Add(headFigure);
            }

            return new System.Windows.Shapes.Path
            {
                Data = geometry,
                Stroke = _selectedBrush,
                Fill = _selectedBrush,
                StrokeThickness = _selectedThickness,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round
            };
        }

        private void StartTextInput(Point pos)
        {
            _activeTextBox = new TextBox
            {
                Foreground = _selectedBrush,
                Background = new SolidColorBrush(Color.FromArgb(180, 15, 23, 42)),
                BorderBrush = _selectedBrush,
                BorderThickness = new Thickness(1),
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Padding = new Thickness(4, 2, 4, 2),
                MinWidth = 80,
                AcceptsReturn = true
            };

            Canvas.SetLeft(_activeTextBox, pos.X);
            Canvas.SetTop(_activeTextBox, pos.Y);

            _activeTextBox.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter && (Keyboard.Modifiers & ModifierKeys.Shift) == 0)
                {
                    CommitActiveText();
                }
                else if (e.Key == Key.Escape)
                {
                    CancelActiveText();
                }
            };

            _activeTextBox.LostFocus += (s, e) => CommitActiveText();

            DrawingCanvas.Children.Add(_activeTextBox);
            _activeTextBox.Focus();
        }

        private void CommitActiveText()
        {
            if (_activeTextBox != null)
            {
                string text = _activeTextBox.Text.Trim();
                double x = Canvas.GetLeft(_activeTextBox);
                double y = Canvas.GetTop(_activeTextBox);

                DrawingCanvas.Children.Remove(_activeTextBox);
                _activeTextBox = null;

                if (!string.IsNullOrEmpty(text))
                {
                    Border textContainer = new Border
                    {
                        Background = new SolidColorBrush(Color.FromArgb(140, 15, 23, 42)),
                        CornerRadius = new CornerRadius(3),
                        Padding = new Thickness(4, 2, 4, 2),
                        Child = new TextBlock
                        {
                            Text = text,
                            Foreground = _selectedBrush,
                            FontSize = 18,
                            FontWeight = FontWeights.Bold
                        }
                    };

                    Canvas.SetLeft(textContainer, x);
                    Canvas.SetTop(textContainer, y);

                    DrawingCanvas.Children.Add(textContainer);
                    _drawnElements.Push(textContainer);
                }
            }
        }

        private void CancelActiveText()
        {
            if (_activeTextBox != null)
            {
                DrawingCanvas.Children.Remove(_activeTextBox);
                _activeTextBox = null;
            }
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            CommitActiveText();

            if (_drawnElements.Count > 0)
            {
                UIElement element = _drawnElements.Pop();
                DrawingCanvas.Children.Remove(element);
            }
        }

        private RenderTargetBitmap RenderCanvasToBitmap()
        {
            CommitActiveText();

            int width = (int)DrawingCanvas.Width;
            int height = (int)DrawingCanvas.Height;

            RenderTargetBitmap renderBmp = new RenderTargetBitmap(
                width,
                height,
                96,
                96,
                PixelFormats.Pbgra32);

            renderBmp.Render(DrawingCanvas);
            return renderBmp;
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RenderTargetBitmap bmp = RenderCanvasToBitmap();
                Clipboard.SetImage(bmp);

                ShowToast("¡Copiado al portapapeles con éxito!");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error al copiar: " + ex.Message);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RenderTargetBitmap bmp = RenderCanvasToBitmap();

                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bmp));

                // Guardar en archivo temporal y reemplazar para evitar bloqueos
                string tempFile = _item.FilePath + ".tmp";
                using (var fs = new FileStream(tempFile, FileMode.Create, FileAccess.Write))
                {
                    encoder.Save(fs);
                }

                if (File.Exists(_item.FilePath))
                {
                    File.Delete(_item.FilePath);
                }
                File.Move(tempFile, _item.FilePath);

                // Recargar miniatura de la tarjeta
                _item.LoadThumbnail();

                ShowToast("¡Cambios guardados!");
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error al guardar: " + ex.Message);
            }
        }

        private void ShowToast(string message)
        {
            ToastLabel.Text = message;
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2.5)
            };
            timer.Tick += (s, e) =>
            {
                ToastLabel.Text = "";
                timer.Stop();
            };
            timer.Start();
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Z && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                UndoButton_Click(null, null);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                Close();
            }
        }
    }
}
