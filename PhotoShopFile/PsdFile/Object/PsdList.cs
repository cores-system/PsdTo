using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Endogine.Serialization;
using System.Collections;

namespace PhotoShopFile
{
    public class PsdList:PsdObject
    {
        /** The objects. */
        private List<PsdObject> objects = new List<PsdObject>();

        /**
         * Instantiates a new psd list.
         *
         * @param stream the stream
         * @throws IOException Signals that an I/O exception has occurred.
         */
        public PsdList(BinaryReverseReader stream) {
                int itemsCount = stream.ReadInt32();
                for (int i = 0; i < itemsCount; i++) {
                        objects.Add(PsdObjectFactory.loadPsdObject(stream));
                }
        }

        public int size() {
                return objects.Count;
        }

        public List<PsdObject> getObjects()
        {
            return objects;
        }
        
       
        public string toString() {
                return "VlLs:" + objects.ToString();
        }

    }
}
