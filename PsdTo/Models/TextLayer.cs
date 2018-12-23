using System.Collections.Generic;

namespace PsdTo.Models
{
    public class TextLayer
    {
        public long Id;
        public long index;
        public string type => "text";
        public string Name;
        public string Text;
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public bool Visible;
        public byte Opacity;
        public byte fill;
        public string fontName;
        public double fontSize;
        public double lineHeight;
        public ColorRBG Color = new ColorRBG();
        public object StyleRun;
        public Dictionary<string, object> Fx;
        public bool Clipping;
        public Transform Transform;
    }
}
