using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace YOLOv4MLNet.DataStructures
{
    public class ProcessBitmap
    {
        public string ImageName { get; set; }
        public Bitmap Bitmap { get; set; }
        public byte[] Bytes { get; set; }
        public IReadOnlyList<YoloV4Result> DetailedResults { get; set; }

        public ProcessBitmap(string imageName, Bitmap bitmap, IReadOnlyList<YoloV4Result> detailedResults)
        {
            ImageName = imageName.Split('\\').LastOrDefault();
            Bitmap = bitmap;
            DetailedResults = detailedResults;

            using var stream = new MemoryStream();
            bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            Bytes = stream.ToArray();
        }
    }
}
