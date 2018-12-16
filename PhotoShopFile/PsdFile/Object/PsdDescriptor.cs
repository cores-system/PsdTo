using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Endogine.Serialization;

namespace PhotoShopFile
{
    public class PsdDescriptor : PsdObject
    {
        /** The class id or layer type. */
        private string classId;

        /** a map containing all values of the descriptor */
        private Dictionary<String, PsdObject> objects = new Dictionary<String, PsdObject>();

        /**
         * Instantiates a new psd descriptor.
         *
         * @param stream the stream
         * @throws IOException Signals that an I/O exception has occurred.
         */
        public PsdDescriptor(BinaryReverseReader stream)
        {
            int unicstrlength = stream.ReadInt32() * 2;
            string unicstr = Encoding.Unicode.GetString(stream.ReadBytes(unicstrlength));
            int classlength = stream.ReadInt32();
            if (classlength != 0)
            {
                classId = Encoding.Default.GetString(stream.ReadBytes(classlength));
            }
            else
            {
                classId = Encoding.Default.GetString(stream.ReadBytes(4));
            }
            // Unicode string: name from classID
            // int nameLen = stream.ReadInt32() * 2;
            //stream.ReadBytes(nameLen);

            //classId = stream.ReadString();
            int itemsCount = stream.ReadInt32();
            for (int i = 0; i < itemsCount; i++)
            {
                int size = stream.ReadInt32();
                if (size == 0)
                {
                    size = 4;

                }

                String key = Encoding.Default.GetString(stream.ReadBytes(size));
                objects.Add(key, PsdObjectFactory.loadPsdObject(stream));
                //objects.Add(key, new PsdObject());
            }
        }

        /**
         * Gets the class id.
         *
         * @return the class id
         */
        public String getClassId()
        {
            return classId;
        }

        /**
         * Gets the objects.
         *
         * @return the objects
         */
        public Dictionary<String, PsdObject> getObjects()
        {
            return objects;
        }

        /**
         * Gets the.
         *
         * @param key the key
         * @return the psd object
         */
        public PsdObject get(String key)
        {
            return objects[key];
        }

        /**
         * Contains key.
         *
         * @param key the key
         * @return true, if successful
         */
        public bool containsKey(String key)
        {
            return objects.Any(e => e.Key == key);
        }


        public String toString()
        {
            return "Objc:" + objects.ToString();
        }

    }
}
