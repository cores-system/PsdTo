using System;
using System.Collections.Generic;

namespace PsdTo.Models
{
    public class RootLayer
    {
        public List<TextLayer> TextLayers = new List<TextLayer>();
        public List<ShapeLayer> ShapeLayers = new List<ShapeLayer>();
        public List<ImageLayer> ImageLayers = new List<ImageLayer>();
    }
}
