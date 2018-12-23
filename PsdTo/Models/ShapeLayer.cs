using System.Collections.Generic;

namespace PsdTo.Models
{
    public class ShapeLayer
    {
        public long Id;
        public long index;
        public string type => "shape";
        public string Name;
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public bool Visible;
        public byte Opacity;
        public byte fill;
        public string Image;
        public bool IsDublicate;
        public ColorRBG Color = new ColorRBG();
        public ColorRBG BorderColor = new ColorRBG();
        public int radius;
        public double lineThickness;
        public Dictionary<string, object> Fx;
        public bool Clipping;
    }
}
