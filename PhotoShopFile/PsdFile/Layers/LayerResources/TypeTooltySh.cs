using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.IO;
using Endogine;
using Endogine.Serialization;

namespace PhotoshopFile.LayerResources
{
    [Description("tySh")]
    public class TypeTooltySh
    {
        public string FontFamily { get; set; }
        public string Text { get; set; }
        public int FontSize { get; set; }
        public int FontWeight { get; set; }
        public int LineHeight { get; set; }
        public string Color { get; set; }

        public double ReadPSDDouble(BinaryReverseReader br)
        {
            //TODO: examine PSD format!
            double val = br.ReadDouble();
            unsafe
            {
               
                BinaryReverseReader.SwapBytes((byte*)&val, 8);
            }
            return val;
        }

        public class FontInfo
        {
            

            public ushort Mark;
            public uint FontType;
            public string FontName;
            public string FontFamilyName;
            public string FontStyleName;
            public ushort Script;
            public List<uint> DesignVectors;

            public string ReadPascalString(BinaryReverseReader rd)
            {
                var size = rd.ReadByte();
                var body = rd.ReadBytes(size);
                var text = System.Text.Encoding.ASCII.GetString(body);
                return text;
            }



            public FontInfo()
            {}
            public FontInfo(BinaryReverseReader r)
            {
                this.Mark = r.ReadUInt16();
                this.FontType = r.ReadUInt32();
                this.FontName = ReadPascalString(r);
                this.FontFamilyName = ReadPascalString(r);
                this.FontStyleName = ReadPascalString(r);
                this.Script = r.ReadUInt16();

                ushort NumDesignAxesVectors = r.ReadUInt16();
                this.DesignVectors = new List<uint>();
                for (int vectorNum = 0; vectorNum < NumDesignAxesVectors; vectorNum++)
                    this.DesignVectors.Add(r.ReadUInt32());
            }
        }

        public List<FontInfo> FontInfos;

        public TypeTooltySh()
        {
        }
        public TypeTooltySh(BinaryReverseReader areader)
        {
            ushort Version = areader.ReadUInt16(); //1= Photoshop 5.0

            for (int i = 0; i < 6; i++) //2D transform matrix
                ReadPSDDouble(areader);


            //Font info:
            ushort FontVersion = areader.ReadUInt16(); //6 = Photoshop 5.0
            ushort FaceCount = areader.ReadUInt16();
            this.FontInfos = new List<FontInfo>();
            for (int i = 0; i < FaceCount; i++)
                this.FontInfos.Add(new FontInfo(areader));

            //TODO: make classes of styles as well...
            ushort StyleCount = areader.ReadUInt16();
            for (int i = 0; i < StyleCount; i++)
            {
                ushort Mark = areader.ReadUInt16();
                ushort FaceMark = areader.ReadUInt16();
                uint Size = areader.ReadUInt32();
                uint Tracking = areader.ReadUInt32();
                uint Kerning = areader.ReadUInt32();
                uint Leading = areader.ReadUInt32();
                uint BaseShift = areader.ReadUInt32();
                
                byte AutoKern = areader.ReadByte();
                byte Extra = 0;
                if (Version <= 5)
                    Extra = areader.ReadByte();
                byte Rotate = areader.ReadByte();
            }

            //Text information
            ushort Type = areader.ReadUInt16();
            uint ScalingFactor = areader.ReadUInt32();
            uint CharacterCount = areader.ReadUInt32();

            uint HorizontalPlacement = areader.ReadUInt32();
            uint VerticalPlacement = areader.ReadUInt32();

            uint SelectStart = areader.ReadUInt32();
            uint SelectEnd = areader.ReadUInt32();

            ushort LineCount = areader.ReadUInt16();
            for (int i = 0; i < LineCount; i++)
            {
                uint CharacterCountLine = areader.ReadUInt32();
                ushort Orientation = areader.ReadUInt16();
                ushort Alignment = areader.ReadUInt16();

                ushort DoubleByteChar = areader.ReadUInt16();
                ushort Style = areader.ReadUInt16();
            }

            ushort ColorSpace = areader.ReadUInt16();
            for (int i = 0; i < 4; i++)
                areader.ReadUInt16(); //Color compensation
            byte AntiAlias = areader.ReadByte();
        }

    }
}
