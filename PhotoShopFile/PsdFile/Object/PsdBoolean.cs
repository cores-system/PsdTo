using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Endogine.Serialization;

namespace PhotoShopFile
{
   public class PsdBoolean:PsdObject
    {
       /** The value. */
        private bool value;

        
        public PsdBoolean(BinaryReverseReader stream) {
                value = stream.ReadBoolean();
                
        }

        /**
         * Gets the value.
         *
         * @return the value
         */
        public bool getValue() {
                return value;
        }

        
        public String toString() {
                return "bool:" + value;
        }
    }
}
