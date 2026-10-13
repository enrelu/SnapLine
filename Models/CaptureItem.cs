using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Media.Imaging;

namespace Portafotos.Models
{
    public class CaptureItem : INotifyPropertyChanged
    {
        private static readonly Random _random = new Random();

        public string Id { get; set; }
        public string FilePath { get; set; }
        public DateTime Timestamp { get; set; }
        public double TiltAngle { get; set; }

        private BitmapSource _thumbnail;
        public BitmapSource Thumbnail
        {
            get { return _thumbnail; }
            set
            {
                if (_thumbnail != value)
                {
                    _thumbnail = value;
                    OnPropertyChanged("Thumbnail");
                }
            }
        }

        private bool _isVisibleOnLine = true;
        public bool IsVisibleOnLine
        {
            get { return _isVisibleOnLine; }
            set
            {
                if (_isVisibleOnLine != value)
                {
                    _isVisibleOnLine = value;
                    OnPropertyChanged("IsVisibleOnLine");
                }
            }
        }

        public CaptureItem()
        {
            Id = Guid.NewGuid().ToString("N");
            Timestamp = DateTime.Now;
            // Angulo sutil de inclinacion entre -2.5 y +2.5 grados
            TiltAngle = (_random.NextDouble() * 5.0) - 2.5;
        }

        public CaptureItem(string filePath) : this()
        {
            FilePath = filePath;
            LoadThumbnail();
        }

        public void LoadThumbnail()
        {
            if (File.Exists(FilePath))
            {
                try
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.UriSource = new Uri(FilePath, UriKind.Absolute);
                    bmp.EndInit();
                    bmp.Freeze();
                    Thumbnail = bmp;
                }
                catch
                {
                    // Ignorar error si el archivo esta en proceso de escritura
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
