using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using System.Windows;
using Portafotos.Models;

namespace Portafotos.Services
{
    public class StorageService
    {
        private const int MaxHistoryCount = 20;
        private readonly string _appFolder;
        private readonly string _capturesFolder;
        private readonly string _historyFile;
        private readonly JavaScriptSerializer _serializer;

        public ObservableCollection<CaptureItem> ActiveCaptures { get; private set; }

        public string CapturesDirectory
        {
            get { return _capturesFolder; }
        }

        public StorageService()
        {
            _appFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Portafotos");
            _capturesFolder = Path.Combine(_appFolder, "Capturas");
            _historyFile = Path.Combine(_appFolder, "history.json");
            _serializer = new JavaScriptSerializer();

            EnsureDirectoriesExist();
            ActiveCaptures = new ObservableCollection<CaptureItem>();
            LoadHistory();
        }

        private void EnsureDirectoriesExist()
        {
            if (!Directory.Exists(_appFolder))
            {
                Directory.CreateDirectory(_appFolder);
            }
            if (!Directory.Exists(_capturesFolder))
            {
                Directory.CreateDirectory(_capturesFolder);
            }
        }

        public string GenerateNewCaptureFilePath()
        {
            EnsureDirectoriesExist();
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
            return Path.Combine(_capturesFolder, string.Format("captura_{0}.png", timestamp));
        }

        public CaptureItem AddCapture(string filePath)
        {
            var item = new CaptureItem(filePath);

            Application.Current.Dispatcher.Invoke(() =>
            {
                // Insertar al inicio de la cuerda
                ActiveCaptures.Insert(0, item);

                // Limitar el historial activo en la cuerda a las ultimas 20 capturas
                while (ActiveCaptures.Count > MaxHistoryCount)
                {
                    ActiveCaptures.RemoveAt(ActiveCaptures.Count - 1);
                }
            });

            SaveHistory();
            return item;
        }

        public void RemoveFromClothesline(CaptureItem item)
        {
            if (item == null) return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                ActiveCaptures.Remove(item);
            });

            SaveHistory();
        }

        public void LoadHistory()
        {
            if (!File.Exists(_historyFile)) return;

            try
            {
                string json = File.ReadAllText(_historyFile);
                var paths = _serializer.Deserialize<string[]>(json);
                if (paths != null)
                {
                    ActiveCaptures.Clear();
                    foreach (var path in paths.Take(MaxHistoryCount))
                    {
                        if (File.Exists(path))
                        {
                            ActiveCaptures.Add(new CaptureItem(path));
                        }
                    }
                }
            }
            catch
            {
                // Si el archivo JSON esta corrupto se reinicia limpiamente
            }
        }

        public void SaveHistory()
        {
            try
            {
                var paths = ActiveCaptures.Select(c => c.FilePath).ToArray();
                string json = _serializer.Serialize(paths);
                File.WriteAllText(_historyFile, json);
            }
            catch
            {
                // Ignorar excepciones secundarias al guardar historial
            }
        }
    }
}
