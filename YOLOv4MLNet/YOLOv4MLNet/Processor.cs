using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Onnx;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using YOLOv4MLNet.DataStructures;
using static Microsoft.ML.Transforms.Image.ImageResizingEstimator;

namespace YOLOv4MLNet
{
    public partial class Processor
    {
        private readonly TransformerChain<OnnxTransformer> _model;
        private readonly MLContext _mlContext;

        public Processor(string modelPath)
        {
            _mlContext = new MLContext();

            var pipeline = _mlContext.Transforms.ResizeImages(inputColumnName: "bitmap", outputColumnName: "input_1:0", imageWidth: 416, imageHeight: 416, resizing: ResizingKind.IsoPad)
                .Append(_mlContext.Transforms.ExtractPixels(outputColumnName: "input_1:0", scaleImage: 1f / 255f, interleavePixelColors: true))
                .Append(_mlContext.Transforms.ApplyOnnxModel(
                    shapeDictionary: new Dictionary<string, int[]>()
                    {
                        { "input_1:0", new[] { 1, 416, 416, 3 } },
                        { "Identity:0", new[] { 1, 52, 52, 3, 85 } },
                        { "Identity_1:0", new[] { 1, 26, 26, 3, 85 } },
                        { "Identity_2:0", new[] { 1, 13, 13, 3, 85 } },
                    },
                    inputColumnNames: new[]
                    {
                        "input_1:0"
                    },
                    outputColumnNames: new[]
                    {
                        "Identity:0",
                        "Identity_1:0",
                        "Identity_2:0"
                    },
                    modelFile: modelPath, recursionLimit: 100));

            _model = pipeline.Fit(_mlContext.Data.LoadFromEnumerable(new List<YoloV4BitmapData>()));
        }

        public ProcessBitmap ProcessImage(string path, IReadOnlyList<YoloV4Result> readyResults = null)
        {
            using var bitmap = new Bitmap(Image.FromFile(path));

            var results = readyResults;
            if (results is null)
            {
                var predictionEngine = _mlContext.Model.CreatePredictionEngine<YoloV4BitmapData, YoloV4Prediction>(_model);
                var predict = predictionEngine.Predict(new YoloV4BitmapData() { Image = bitmap });
                results = predict.GetResults(classesNames, 0.3f, 0.7f);
            }

            using var g = Graphics.FromImage(bitmap);
            foreach (var res in results)
            {
                var x1 = res.BBox[0];
                var y1 = res.BBox[1];
                var x2 = res.BBox[2];
                var y2 = res.BBox[3];
                g.DrawRectangle(Pens.Red, x1, y1, x2 - x1, y2 - y1);
                using (var brushes = new SolidBrush(Color.FromArgb(50, Color.Red)))
                {
                    g.FillRectangle(brushes, x1, y1, x2 - x1, y2 - y1);
                }

                g.DrawString(res.Label + " " + res.Confidence.ToString("0.00"),
                             new Font("Arial", 12), Brushes.Blue, new PointF(x1, y1));
            }

            var newBitmap = bitmap.Clone() as Bitmap;
            return new ProcessBitmap(path, newBitmap, results);
        }
    }
}
