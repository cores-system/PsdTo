using System;
using System.Text;
using Endogine.Serialization;

namespace PhotoShopFile
{
    public class PsdObjectFactory
    {
        public static PsdObject loadPsdObject(BinaryReverseReader stream)
        {
            string type = Encoding.Default.GetString(stream.ReadBytes(4));
            switch (type)
            {
                case "Objc":
                    return new PsdDescriptor(stream);
                case "GlbO":
                    return new PsdDescriptor(stream);
                case "VlLs":
                    return new PsdList(stream);
                case "doub":
                    return new PsdDouble(stream);
                case "long":
                    return new PsdLong(stream);
                case "bool":
                    return new PsdBoolean(stream);
                case "UntF":
                    return new PsdUnitFloat(stream);
                case "enum":
                    return new PsdEnum(stream);
                case "TEXT":
                    return new PsdText(stream);
                case "tdta":
                    return new PsdTextData(stream);
                default:
                    return new PsdObject();
            }
        }
    }
}
