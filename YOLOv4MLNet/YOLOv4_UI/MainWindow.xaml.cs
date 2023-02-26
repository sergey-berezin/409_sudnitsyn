using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using YOLOv4MLNet;

namespace YOLOv4_UI
{
    public partial class MainWindow : Window
    {
        private List<LoadedImage> loadedImages = new List<LoadedImage>();
        private string modelPath = @"D:\Downloads\test\yolov4.onnx";
        private Processor processor;
        private bool isCancelProcessing = false;
        private string outputFolder = @"C:\YOLOv4\Output";

        public MainWindow()
        {
            InitializeComponent();
            modelPath = Path.Combine(Environment.CurrentDirectory, @"Models\yolov4.onnx");
            outputFolder = Path.Combine(Environment.CurrentDirectory, @"Output");
            processor = new Processor(modelPath);
        }

        private void Btn_SelectImages_Click(object sender, RoutedEventArgs e)
        {
            SelectImages();
        }

        private void SelectImages()
        {
            var ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            ofd.Filter = "Изображения|*.jpg;*.jpeg;*.png;";

            if (ofd.ShowDialog() == false)
                return;

            var i = 0;
            var images = ofd.FileNames.Select(x =>
            {
                var bytes = File.ReadAllBytes(x);

                return new LoadedImage()
                {
                    Id = i++,
                    ImageData = bytes,
                    InputPath = x,
                };
            }).ToList();

            loadedImages = images;
            LV_Images.ItemsSource = loadedImages;
        }

        private async void Btn_BeginTask_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(modelPath))
            {
                MessageBox.Show("Выберите путь для модели нейросети");
                return;
            }

            if (string.IsNullOrEmpty(outputFolder))
            {
                MessageBox.Show("Выберите путь для выгрузки результата");
                return;
            }

            Btn_CancelTask.Visibility = Visibility.Visible;

            if (loadedImages.Count == 0)
                SelectImages();

            var readyResults = new List<JsonResultModel>();
            if (Directory.Exists(outputFolder))
            {
                var readyFiles = Directory.EnumerateFiles(outputFolder, "*.json", SearchOption.TopDirectoryOnly);

                foreach (var file in readyFiles)
                {
                    var text = File.ReadAllText(file);
                    try
                    {
                        var model = JsonConvert.DeserializeObject<JsonResultModel>(text);
                        readyResults.Add(model);
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                }
            }

            foreach (var loadedImage in loadedImages)
            {
                if (loadedImage.Processed) continue;

                var readyResult = readyResults.FirstOrDefault(x => x.ImageFullInputPath == loadedImage.InputPath);
                var result = await Task.Run(() => processor.ProcessImage(loadedImage.InputPath, readyResult?.DetailedResults));

                var exist = loadedImages.FirstOrDefault(x => x.Id == loadedImage.Id);
                if (exist is null) continue;

                exist.ImageData = result.Bytes;
                exist.Processed = true;

                var jsonModel = new JsonResultModel()
                {
                    DetailedResults = result.DetailedResults,
                    ImageFullInputPath = loadedImage.InputPath,
                    ProcessDateTime = DateTime.Now,
                };

                var json = JsonConvert.SerializeObject(jsonModel);
                if (!Directory.Exists(outputFolder))
                    Directory.CreateDirectory(outputFolder);

                var outputPath = Path.Combine(outputFolder, result.ImageName + ".json");
                File.WriteAllText(outputPath, json);

                if (isCancelProcessing)
                {
                    isCancelProcessing = false;
                    break;
                }
            }

            Btn_CancelTask.Visibility = Visibility.Collapsed;
        }

        private void Btn_CancelTask_Click(object sender, RoutedEventArgs e)
        {
            isCancelProcessing = true;
        }

        private void Btn_SelectOutputPath_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new CommonOpenFileDialog
            {
                Title = "Выбор каталога для вывода",
                IsFolderPicker = true,

                AddToMostRecentlyUsedList = false,
                AllowNonFileSystemItems = false,
                EnsureFileExists = true,
                EnsurePathExists = true,
                EnsureReadOnly = false,
                EnsureValidNames = true,
                Multiselect = false,
                ShowPlacesList = true
            };

            if (dlg.ShowDialog() != CommonFileDialogResult.Ok)
                return;

            outputFolder = dlg.FileName;
            Lbl_OutputPath.Content = "Каталог для результата: " + outputFolder;
        }

        private void Btn_SelectModelPath_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            ofd.Filter = "Модель|*.onnx;";

            if (ofd.ShowDialog() == false)
                return;

            modelPath = ofd.FileName;
            Lbl_ModelPath.Content = "Путь к модели нейросети: " + modelPath;
        }
    }
}
