using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Endogine.Serialization;
using System.Diagnostics;
using System.IO;

namespace PhotoShopFile
{
   public class PsdTextData:PsdObject
    {
        private IDictionary<string, object> properties;
        private int cachedByte = -1;
        private bool useCachedByte;
        
        public PsdTextData(BinaryReverseReader stream) {
            int size = stream.ReadInt32();
            byte[] array=new byte[size];
            array = stream.ReadBytes(size);
            Stream str = new MemoryStream(array);
            BinaryReverseReader br = new BinaryReverseReader(str);
            properties = readMap(br);
        }

        /**
         * Gets the properties.
         *
         * @return the properties
         */
        public IDictionary<string, object> getProperties() {
                return properties;
        }

        private IDictionary<string, object> readMap(BinaryReverseReader stream)
        {
            skipWhitespaces(stream);
            char c = (char)readByte(stream);

            if (c == ']')
            {
                return null;
            }
            else if (c == '<')
            {
                skipString(stream, "<");
            }
            Dictionary<string, object> map = new Dictionary<string, object>();
            while (true)
            {
                skipWhitespaces(stream);
                c = (char)readByte(stream);
                if (c == '>')
                {
                    skipString(stream, ">");
                    return map;
                }
                else
                {
                    Debug.Assert(c == '/', "unknown char: " + c + ", byte: " + (sbyte)c);
                    string name = readName(stream);
                    skipWhitespaces(stream);
                    c = (char)lookForwardByte(stream);
                    if (c == '<')
                    {
                        map[name] = readMap(stream);
                    }
                    else
                    {
                        map[name] = readValue(stream);
                    }
                }
            }
        }

        private String readName(BinaryReverseReader stream)
        {
                string name = "";
                while (true) {
                        char c = (char) readByte(stream);
                        if (c == ' ' || c == 10) {
                                break;
                        }
                        name += c;
                }
                return name;
        }

        private object readValue(BinaryReverseReader stream)
        {
            char c = (char)readByte(stream);
            if (c == ']')
            {
                return null;
            }
            else if (c == '(')
            {
                // unicode string
                string str = "";
                int stringSignature = readShort(stream) & 0xFFFF;
                Debug.Assert(stringSignature == 0xFEFF);
                while (true)
                {
                    byte b1 = readByte(stream);
                    if (b1 == ')')
                    {
                        return str;
                    }
                    byte b2 = readByte(stream);
                    if (b2 == '\\')
                    {
                        b2 = readByte(stream);
                    }
                    if (b2 == 13)
                    {
                        str += '\n';
                    }
                    else
                    {
                        str += (char)((b1 << 8) | b2);
                    }
                }
            }
            else if (c == '[')
            {
                List<object> list = new List<object>();
                // array
                c = (char)readByte(stream);
                while (true)
                {
                    skipWhitespaces(stream);
                    c = (char)lookForwardByte(stream);
                    if (c == '<')
                    {
                        object val = readMap(stream);
                        if (val == null)
                        {
                            return list;
                        }
                        else
                        {
                            list.Add(val);
                        }
                    }
                    else
                    {
                        object val = readValue(stream);
                        if (val == null)
                        {
                            return list;
                        }
                        else
                        {
                            list.Add(val);
                        }
                    }
                }
            }
            else
            {
                string val = "";
                do
                {
                    val += c;
                    c = (char)readByte(stream);
                } while (c != 10 && c != ' ');
                if (bool.TryParse(val, out _))
                {
                    return Convert.ToBoolean(val);
                }
                else
                {
                    try
                    {
                        return Convert.ToDouble(val.Replace(".", ","));
                    }
                    catch { return 0.0; }
                }
            }
        }

        private void skipWhitespaces(BinaryReverseReader stream) {
                byte b;
                do {
                        b = readByte(stream);
                } while (b == ' ' || b == 10 || b == 9);
                putBack();
        }

        private void skipString(BinaryReverseReader stream, string str) {
                for (int i = 0; i < str.Length; i++) {
                        char streamCh = (char) readByte(stream);
                        Debug.Assert(streamCh == str[i], "char " + streamCh + " mustBe " + str[i]);
                }
        }

        
        public override string ToString() {
                return properties.ToString();
        }

        private byte readByte(BinaryReverseReader stream) {
           if (useCachedByte)
			{
				Debug.Assert(cachedByte != -1);
				useCachedByte = false;
				return (byte) cachedByte;
			}
			else
			{
				cachedByte = stream.ReadByte();
				return (byte) cachedByte;
			}
        }

        private short readShort(BinaryReverseReader stream) {
                cachedByte = -1;
                useCachedByte = false;
                return stream.ReadInt16();
        }

        private void putBack() {
            Debug.Assert(cachedByte != -1);
            Debug.Assert(!useCachedByte);
            useCachedByte = true;
        }

        private byte lookForwardByte(BinaryReverseReader stream)  {
                byte b = readByte(stream);
                putBack();
                return b;
        }
    }
}
