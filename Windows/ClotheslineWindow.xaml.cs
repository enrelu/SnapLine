using System;
using System.Collections.Specialized;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using SnapLine.Models;
using SnapLine.Services;

namespace SnapLine.Windows
{
    public partial class ClotheslineWindow : Window
    {
        private readonly StorageService _storageService;
        private Point _dragStartPoint;
        private bool _isDragging = false;
        private DispatcherTimer _autoHideTimer;
        private bool _isHiding = false;

        public ClotheslineWindow(StorageService storageService)
        {
            InitializeComponent();

            _storageService = storageService;
            DataContext = _storageService;

            SetupWindowPlacement();
            SetupAutoHideTimer();

            _storageService.ActiveCaptures.CollectionChanged += ActiveCaptures_CollectionChanged;
        }

        private void SetupWindowPlacement()
        {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            Width = Math.Min(1500, screenWidth * 0.94);
            Left = (screenWidth - Width) / 2;
            Top = 0;
        }

        private void SetupAutoHideTimer()
        {
            _autoHideTimer = new DispatcherTimer();
            _autoHideTimer.Tick += (s, e) =>
            {
                _autoHideTimer.Stop();
                SlideOutAndHide();
            };
        }

        private void ActiveCaptures_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                CapturesScrollViewer.ScrollToLeftEnd();
            }
        }

        /// <summary>
        /// Muestra la cuerda únicamente cuando se realiza una captura o se invoca explícitamente.
        /// </summary>
        public void ShowAndSlideIn()
        {
            _isHiding = false;
            _autoHideTimer.Stop();

            Visibility = Visibility.Visible;
            Topmost = true;

            DoubleAnimation slideDown = new DoubleAnimation
            {
                From = -200,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(320),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            SlideTransform.BeginAnimation(TranslateTransform.YProperty, slideDown);
            CapturesScrollViewer.ScrollToLeftEnd();

            // Ocultar automaticamente tras 7 segundos si el usuario no interactua
            _autoHideTimer.Interval = TimeSpan.FromSeconds(7);
            _autoHideTimer.Start();
        }

        /// <summary>
        /// Oculta la cuerda deslizándola suavemente hacia arriba.
        /// </summary>
        public void SlideOutAndHide()
        {
            if (_isHiding || Visibility != Visibility.Visible) return;

            _isHiding = true;
            _autoHideTimer.Stop();

            DoubleAnimation slideUp = new DoubleAnimation
            {
                To = -200,
                Duration = TimeSpan.FromMilliseconds(260),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            slideUp.Completed += (s, e) =>
            {
                if (_isHiding)
                {
                    Visibility = Visibility.Hidden;
                    _isHiding = false;
                }
            };

            SlideTransform.BeginAnimation(TranslateTransform.YProperty, slideUp);
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            // Mientras el raton este sobre la cuerda o fotos, no se oculta
            _autoHideTimer.Stop();
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            // Al salir el raton, comenzar temporizador de 4 segundos antes de ocultar
            if (Visibility == Visibility.Visible && !_isHiding)
            {
                _autoHideTimer.Interval = TimeSpan.FromSeconds(4);
                _autoHideTimer.Start();
            }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            // Si el usuario hace clic fuera o cambia de ventana, se oculta suavemente
            SlideOutAndHide();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                SlideOutAndHide();
            }
        }

        private void DismissClotheslineButton_Click(object sender, RoutedEventArgs e)
        {
            SlideOutAndHide();
        }

        private void CapturesScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta != 0)
            {
                CapturesScrollViewer.ScrollToHorizontalOffset(CapturesScrollViewer.HorizontalOffset - e.Delta);
                e.Handled = true;
            }
        }

        private void Card_MouseEnter(object sender, MouseEventArgs e)
        {
            _autoHideTimer.Stop();

            Grid cardGrid = sender as Grid;
            if (cardGrid != null)
            {
                Panel.SetZIndex(cardGrid, 50);

                StackPanel buttonsPanel = cardGrid.FindName("ActionButtonsPanel") as StackPanel;
                if (buttonsPanel != null) buttonsPanel.Visibility = Visibility.Visible;

                TransformGroup transformGroup = cardGrid.RenderTransform as TransformGroup;
                if (transformGroup != null)
                {
                    foreach (Transform tr in transformGroup.Children)
                    {
                        RotateTransform rot = tr as RotateTransform;
                        if (rot != null)
                        {
                            rot.BeginAnimation(RotateTransform.AngleProperty,
                                new DoubleAnimation(0, TimeSpan.FromMilliseconds(150)));
                        }
                        else
                        {
                            ScaleTransform scale = tr as ScaleTransform;
                            if (scale != null)
                            {
                                scale.BeginAnimation(ScaleTransform.ScaleXProperty,
                                    new DoubleAnimation(1.05, TimeSpan.FromMilliseconds(150)));
                                scale.BeginAnimation(ScaleTransform.ScaleYProperty,
                                    new DoubleAnimation(1.05, TimeSpan.FromMilliseconds(150)));
                            }
                        }
                    }
                }
            }
        }

        private void Card_MouseLeave(object sender, MouseEventArgs e)
        {
            Grid cardGrid = sender as Grid;
            if (cardGrid != null)
            {
                Panel.SetZIndex(cardGrid, 0);

                StackPanel buttonsPanel = cardGrid.FindName("ActionButtonsPanel") as StackPanel;
                if (buttonsPanel != null) buttonsPanel.Visibility = Visibility.Collapsed;

                CaptureItem item = cardGrid.DataContext as CaptureItem;
                double originalAngle = item != null ? item.TiltAngle : 0;

                TransformGroup transformGroup = cardGrid.RenderTransform as TransformGroup;
                if (transformGroup != null)
                {
                    foreach (Transform tr in transformGroup.Children)
                    {
                        RotateTransform rot = tr as RotateTransform;
                        if (rot != null)
                        {
                            rot.BeginAnimation(RotateTransform.AngleProperty,
                                new DoubleAnimation(originalAngle, TimeSpan.FromMilliseconds(150)));
                        }
                        else
                        {
                            ScaleTransform scale = tr as ScaleTransform;
                            if (scale != null)
                            {
                                scale.BeginAnimation(ScaleTransform.ScaleXProperty,
                                    new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(150)));
                                scale.BeginAnimation(ScaleTransform.ScaleYProperty,
                                    new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(150)));
                            }
                        }
                    }
                }
            }
        }

        private void Card_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DependencyObject dep = e.OriginalSource as DependencyObject;
            if (dep != null && FindParent<Button>(dep) != null)
            {
                return;
            }

            _dragStartPoint = e.GetPosition(null);
            _isDragging = false;
        }

        private void Card_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging) return;

            DependencyObject dep = e.OriginalSource as DependencyObject;
            if (dep != null && FindParent<Button>(dep) != null)
            {
                return;
            }

            Grid cardGrid = sender as Grid;
            if (cardGrid != null)
            {
                CaptureItem item = cardGrid.DataContext as CaptureItem;
                if (item != null)
                {
                    OpenEditor(item);
                }
            }
        }

        private void Card_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !_isDragging)
            {
                Point currentPos = e.GetPosition(null);
                Vector diff = _dragStartPoint - currentPos;

                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    Grid cardGrid = sender as Grid;
                    if (cardGrid != null)
                    {
                        CaptureItem item = cardGrid.DataContext as CaptureItem;
                        if (item != null && File.Exists(item.FilePath))
                        {
                            _isDragging = true;

                            DataObject data = new DataObject(DataFormats.FileDrop, new string[] { item.FilePath });
                            DragDropEffects result = DragDrop.DoDragDrop(cardGrid, data, DragDropEffects.Copy);

                            // Al soltarse en otra aplicacion, desaparece de la cuerda
                            if (result != DragDropEffects.None)
                            {
                                AnimateAndRemoveCard(cardGrid, item);
                            }

                            _isDragging = false;
                        }
                    }
                }
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
            {
                CaptureItem item = btn.Tag as CaptureItem;
                if (item != null)
                {
                    OpenEditor(item);
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
            {
                CaptureItem item = btn.Tag as CaptureItem;
                if (item != null)
                {
                    Grid cardGrid = FindParent<Grid>(btn);
                    AnimateAndRemoveCard(cardGrid, item);
                }
            }
        }

        private void OpenEditor(CaptureItem item)
        {
            if (item == null || !File.Exists(item.FilePath)) return;

            _autoHideTimer.Stop();

            AnnotationEditorWindow editor = new AnnotationEditorWindow(item, _storageService);
            editor.Owner = this;
            editor.ShowDialog();

            _autoHideTimer.Interval = TimeSpan.FromSeconds(5);
            _autoHideTimer.Start();
        }

        private void AnimateAndRemoveCard(Grid cardGrid, CaptureItem item)
        {
            if (cardGrid != null)
            {
                TransformGroup transformGroup = cardGrid.RenderTransform as TransformGroup;
                TranslateTransform dropTransform = null;
                if (transformGroup != null)
                {
                    foreach (Transform tr in transformGroup.Children)
                    {
                        TranslateTransform tt = tr as TranslateTransform;
                        if (tt != null) { dropTransform = tt; break; }
                    }
                }

                DoubleAnimation dropAnim = new DoubleAnimation
                {
                    To = 120,
                    Duration = TimeSpan.FromMilliseconds(250),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
                };

                DoubleAnimation fadeAnim = new DoubleAnimation
                {
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(250)
                };

                fadeAnim.Completed += (s, ev) =>
                {
                    _storageService.RemoveFromClothesline(item);
                    if (_storageService.ActiveCaptures.Count == 0)
                    {
                        SlideOutAndHide();
                    }
                };

                if (dropTransform != null)
                {
                    dropTransform.BeginAnimation(TranslateTransform.YProperty, dropAnim);
                }
                cardGrid.BeginAnimation(UIElement.OpacityProperty, fadeAnim);
            }
            else
            {
                _storageService.RemoveFromClothesline(item);
            }
        }

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(child);
            while (parent != null && !(parent is T))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as T;
        }
    }
}
