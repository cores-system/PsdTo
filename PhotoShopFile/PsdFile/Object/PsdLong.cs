using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Endogine.Serialization;

namespace PhotoShopFile
{
    public class PsdLong:PsdObject
    {
        /** The value. */
        private  int value;

        /**
         * Instantiates a new psd long.
         *
         * @param stream the stream
         * @throws IOException Signals that an I/O exception has occurred.
         */
        public PsdLong(BinaryReverseReader stream) {
                value = stream.ReadInt32();
        }

        /**
         * Gets the value.
         *
         * @return the value
         */
        public int getValue() {
                return value;
        }

        
        public string toString() {
                return "long:" + value;
        }

    }
}
