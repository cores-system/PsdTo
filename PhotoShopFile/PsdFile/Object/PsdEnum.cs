using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Endogine.Serialization;

namespace PhotoShopFile
{
    public class PsdEnum:PsdObject
    {
         /** The type id. */
        private string typeId;
        
        /** The value. */
        private  string value;

        
        
        public PsdEnum(BinaryReverseReader stream) {
            
            typeId = Encoding.Default.GetString(stream.ReadBytes(getSize(stream)));
            value = Encoding.Default.GetString(stream.ReadBytes(getSize(stream)));
                
        }
        public int getSize(BinaryReverseReader str)
        {
            int size = str.ReadInt32();
            if (size == 0)
            {
                size = 4;
            }
            return size;
        }
        /**
         * Gets the type id.
         *
         * @return the type id
         */
        public String getTypeId() {
                return typeId;
        }

        /**
         * Gets the value.
         *
         * @return the value
         */
        public String getValue() {
                return value;
        }

        
        public String toString() {
                return "enum:<" + typeId + ":" + value + ">";
        }

    }
}
