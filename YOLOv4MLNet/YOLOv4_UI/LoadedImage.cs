using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace YOLOv4_UI
{
    public class LoadedImage : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string InputPath { get; set; }

        private byte[] imageData;
        public byte[] ImageData
        {
            get { return imageData; }
            set
            {
                imageData = value;
                OnPropertyChanged(nameof(ImageData));
            }
        }
        public bool Processed { get; set; } = false;

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
