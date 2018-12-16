using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Endogine.Serialization;

namespace PhotoShopFile
{
    public class PsdText:PsdObject
    {
         
        private string value;

        
        public PsdText(BinaryReverseReader stream) {
                int textSize = stream.ReadInt32()*2;
                StringBuilder buffer = new StringBuilder(textSize);
                bool stopReading = false;
                for (int ti = 0; ti < textSize; ti++)
                {
                    char b = (char)stream.ReadByte();
                    if (b == 0)
                    {
                        stopReading = true;
                    }
                    else
                    {
                        stopReading = false;
                    }
                    if (!stopReading)
                    {
                        if (b == 13 || b == 10)
                        {
                            buffer.Append('\n');
                        }
                        else
                        {
                            buffer.Append(b);
                        }
                    }
                }
                value = buffer.ToString();

        }

        /**
         * Gets the value.
         *
         * @return the value
         */
        public String getValue() {
                return value;
        }

        /* (non-Javadoc)
         * @see java.lang.Object#toString()
         */
        
        public string toString() {
                return value;
        }
    }
}
