using Endogine.Serialization;
using PhotoshopFile;
using PhotoShopFile;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace PsdTo.Engine
{
    public static class PsdCommon
    {
        public static int GetRadius(PsdDescriptor descr)
        {
            ReturnedObject = null;
            var obj = descr.getObjects();
            GetObject(obj, "keyOriginRRectRadii");
            PsdDescriptor a = (PsdDescriptor)ReturnedObject;
            if (a != null)
            {
                PsdUnitFloat radiusobj = (PsdUnitFloat)a.getObjects().FirstOrDefault(em => em.Key == "topLeft").Value;
                double db = ReadPSDDouble(GetByteStreamFromDouble(radiusobj.getValue()));
                return Convert.ToInt32(db);
            }
            return 0;
        }


        public static string GetColor(PsdDescriptor descr, string key)
        {
            ReturnedObject = null;
            var obj = descr.getObjects();
            GetObject(obj, key);
            if (ReturnedObject != null)
            {
                return GetPSDColor((PsdDescriptor)ReturnedObject);
            }
            return "No Color";
        }


        public static string GetPSDColor(PsdDescriptor descr)
        {
            ReturnedObject = null;
            var obj = descr.getObjects();
            GetObject(obj, "Clr ");
            PsdDescriptor clr = (PsdDescriptor)ReturnedObject;
            if (clr != null)
            {

                Dictionary<string, PsdObject> values = clr.getObjects();
                var valR = (PsdDouble)values["Rd  "];
                var valG = (PsdDouble)values["Grn "];
                var valB = (PsdDouble)values["Bl  "];
                return Color.FromArgb((int)Math.Round(ReadPSDDouble(GetByteStreamFromDouble(valR.getValue()))), (int)Math.Round(ReadPSDDouble(GetByteStreamFromDouble(valG.getValue()))), (int)Math.Round(ReadPSDDouble(GetByteStreamFromDouble(valB.getValue())))).Name;
            }
            return "No Color";

        }

        public static Color GetPSDColorRBG(PsdDescriptor descr)
        {
            ReturnedObject = null;
            var obj = descr.getObjects();
            GetObject(obj, "Clr ");
            PsdDescriptor clr = (PsdDescriptor)ReturnedObject;
            if (clr != null)
            {

                Dictionary<string, PsdObject> values = clr.getObjects();
                var valR = (PsdDouble)values["Rd  "];
                var valG = (PsdDouble)values["Grn "];
                var valB = (PsdDouble)values["Bl  "];
                return Color.FromArgb((int)Math.Round(ReadPSDDouble(GetByteStreamFromDouble(valR.getValue()))), (int)Math.Round(ReadPSDDouble(GetByteStreamFromDouble(valG.getValue()))), (int)Math.Round(ReadPSDDouble(GetByteStreamFromDouble(valB.getValue()))));
            }
            return Color.Black;

        }


        public static BinaryReverseReader GetByteStreamFromDouble(double val)
        {
            byte[] barr = ConvertDoubleToByteArray(val);
            Stream st = new MemoryStream(barr);
            BinaryReverseReader rd = new BinaryReverseReader(st);
            return rd;
        }


        public static double ReadPSDDouble(BinaryReverseReader br)
        {

            double val = br.ReadDouble();
            unsafe
            {

                BinaryReverseReader.SwapBytes((byte*)&val, 8);
            }
            return val;
        }


        public static byte[] ConvertDoubleToByteArray(double d)
        {
            return BitConverter.GetBytes(d);
        }


        public static string LayerName(string name)
        {
            string result = Regex.Replace(name, @"[^\p{L}\p{N}]+", "");
            return result;
        }


        public static object ReturnedObject { get; set; }
        public static void GetObject(object obj, string val)
        {
            if (obj.GetType() == typeof(Dictionary<string, PsdObject>))
            {
                var dict = (Dictionary<string, PsdObject>)obj;
                foreach (string key in dict.Keys)
                {
                    if (key == val)
                    {
                        ReturnedObject = dict[key];
                        break;
                    }
                    GetObject(dict[key], val);
                }
            }
            else
            {
                if (obj.GetType() == typeof(PsdList))
                {
                    var listobj = (PsdList)obj;
                    foreach (var lob in listobj.getObjects())
                    {
                        GetObject(lob, val);
                    }
                }
                else if (obj.GetType() == typeof(PsdDescriptor))
                {
                    var descrobj = (PsdDescriptor)obj;

                    GetObject(descrobj.getObjects(), val);
                }
                else if (obj.GetType() == typeof(PsdTextData))
                {
                    var textDataobj = (PsdTextData)obj;

                    GetObject(textDataobj.getProperties(), val);

                }
                else if (obj.GetType() == typeof(Dictionary<string, object>))
                {
                    var textdatadict = (Dictionary<string, object>)obj;
                    foreach (string key in textdatadict.Keys)
                    {
                        if (key == val)
                        {
                            ReturnedObject = textdatadict[key];
                            break;
                        }
                        GetObject(textdatadict[key], val);
                    }
                }
                else if (obj.GetType() == typeof(List<object>))
                {
                    var listdict = (List<object>)obj;
                    foreach (var obje in listdict)
                    {

                        GetObject(obje, val);
                    }
                }
            }
        }


        public static BinaryReverseReader CreateStream(LayerInfo info)
        {
            RawLayerInfo rawly = (RawLayerInfo)info;
            Stream str = new MemoryStream(rawly.Data);
            BinaryReverseReader brd = new BinaryReverseReader(str);
            if (rawly.Key == "vogk")
            {
                int version = brd.ReadInt32();
                int version1 = brd.ReadInt32();
            }
            else if (rawly.Key == "vstk")
            {
                int vers = brd.ReadInt32();
            }
            else if (rawly.Key == "vscg")
            {
                int v = brd.ReadInt32();
                int v1 = brd.ReadInt32();
            }
            else if (rawly.Key == "lfx2")
            {
                int v = brd.ReadInt32();
                int v1 = brd.ReadInt32();
            }
            else if (rawly.Key == "SoCo")
            {
                int v = brd.ReadInt32();
            }
            return brd;
        }
    }
}
