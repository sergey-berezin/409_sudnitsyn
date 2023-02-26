namespace YOLOv4MLNet.DataStructures
{
    public class YoloV4Result
    {
        public float[] BBox { get; }

        public string Label { get; }

        public float Confidence { get; }

        public YoloV4Result(float[] bbox, string label, float confidence)
        {
            BBox = bbox;
            Label = label;
            Confidence = confidence;
        }
    }
}
