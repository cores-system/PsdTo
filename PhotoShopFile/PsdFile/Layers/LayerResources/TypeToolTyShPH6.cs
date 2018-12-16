using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.IO;
using Endogine;
using Endogine.Serialization;
using PhotoShopFile;
using System.Linq;

namespace PhotoshopFile.LayerResources
{
    public class Matrix2D
    {
        public double M11;
        public double M12;
        public double M13;
        public double M21;
        public double M22;
        public double M23;

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

        public Matrix2D()
        { }
        public Matrix2D(BinaryReverseReader r)
        {
            this.M11 = ReadPSDDouble(r);
            this.M12 = ReadPSDDouble(r);
            this.M13 = ReadPSDDouble(r);
            this.M21 = ReadPSDDouble(r);
            this.M22 = ReadPSDDouble(r);
            this.M23 = ReadPSDDouble(r);
        }
    }

    [Description("TySh")]
    public class TypeToolTyShPH6
    {
       
        public Matrix2D Transform;
        
        public PsdDescriptor TxtDescriptor;
        
        public PsdDescriptor  WarpDescriptor;
        
        public ERectangleF WarpRect;

        public byte[] Data;
       
        public TdTaStylesheetReader StylesheetReader;
        public Dictionary<string, object> engineData;
        public object engine;
        
        public Boolean isTextHorizontal
        {
            get
            {
                return (TxtDescriptor.getObjects()["Ornt"].ToString() == "Hrzn");
            }
        }

        public TypeToolTyShPH6()
        {
        }
        private Dictionary<String, PsdObject> objects = new Dictionary<String, PsdObject>();
        public TypeToolTyShPH6(BinaryReverseReader r)
        {
            ushort Version = r.ReadUInt16(); //1= Photoshop 5.0

            this.Transform = new Matrix2D(r);

            ushort TextDescriptorVersion = r.ReadUInt16(); //=50. For Photoshop 6.0.

            if (TextDescriptorVersion == 50)
            {
                uint XTextDescriptorVersion = r.ReadUInt32(); //=16. For Photoshop 6.0.
                
                this.TxtDescriptor = new PsdDescriptor(r);

                ushort WarpVersion = r.ReadUInt16(); //2 bytes, =1. For Photoshop 6.0.
                uint WarpDescriptorVersion = r.ReadUInt32(); //4 bytes, =16. For Photoshop 6.0.
                var a = (PsdTextData)TxtDescriptor.getObjects()["EngineData"];
                engine = (object)a.getProperties();
                engineData = (Dictionary<string, object>)engine;
                StylesheetReader = new TdTaStylesheetReader(engineData);

                //string desc = this.TxtDescriptor.getString();

                this.WarpDescriptor = new PsdDescriptor(r); //Warp descriptor
                this.Data = r.ReadBytes((int)r.BytesToEnd); //17 bytes???? All zeroes?
                if (Data.Length != 17 || !(Array.TrueForAll(Data, b => b == 0)))
                {
                    string s = ReadableBinary.CreateHexEditorString(Data);
                   
                }
            }
            
        }

    }
}