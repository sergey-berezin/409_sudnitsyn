using System;
using System.Collections.Generic;
using YOLOv4MLNet.DataStructures;

namespace YOLOv4_UI
{
    public class JsonResultModel
    {
        public string ImageFullInputPath { get; set; }
        public DateTime ProcessDateTime { get; set; } = DateTime.Now;
        public IReadOnlyList<YoloV4Result> DetailedResults { get; set; }
    }
}
