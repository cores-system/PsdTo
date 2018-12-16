using System.Collections.Generic;

namespace PsdTo.Models
{
    public class ImageLayer
    {
        public long Id;
        public long index;
        public string type => "image";
        public string Name;
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public bool Visible;
        public bool IsDublicate;
        public byte Opacity;
        public string Source;
        public Dictionary<string, object> Fx;
        public bool Clipping;
    }
}
