using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Endogine.Serialization;

namespace PhotoShopFile
{
    public class PsdUnitFloat:PsdObject
    {
        /** The unit. */
        private string unit;
        
        /** The value. */
        private double value;

        
        public PsdUnitFloat(BinaryReverseReader stream) {
            unit = Encoding.Default.GetString(stream.ReadBytes(4));
            value = stream.ReadDouble();
        }
        
        /**
         * Gets the unit.
         *
         * @return the unit
         */
        public String getUnit() {
                return unit;
        }

        /**
         * Gets the value.
         *
         * @return the value
         */
        public double getValue() {
                return value;
        }

        
        public String toString() {
                return "UntF:<" + unit + ":" + value + ">";
        }
    }
}
