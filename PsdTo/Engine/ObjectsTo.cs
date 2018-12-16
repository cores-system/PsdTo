using PhotoShopFile;
using System;
using System.Collections.Generic;

namespace PsdTo.Engine
{
    public static class ObjectsTo
    {
        #region Dictionary
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objects"></param>
        public static Dictionary<string, object> Dictionary(Dictionary<string, PsdObject> objects)
        {
            Dictionary<string, object> mass = new Dictionary<string, object>();

            foreach (var ob in objects)
            {
                switch (ob.Value)
                {
                    case PsdDouble psdDouble:
                        {
                            mass.Add(ob.Key.Trim(), PsdCommon.ReadPSDDouble(PsdCommon.GetByteStreamFromDouble(psdDouble.getValue())));
                            break;
                        }
                    case PsdLong psdLong:
                        {
                            mass.Add(ob.Key.Trim(), psdLong.getValue());
                            break;
                        }
                    case PsdBoolean psdBoolean:
                        {
                            mass.Add(ob.Key.Trim(), psdBoolean.getValue());
                            break;
                        }
                    case PsdUnitFloat psdUnitFloat:
                        {
                            mass.Add(ob.Key.Trim(), PsdCommon.ReadPSDDouble(PsdCommon.GetByteStreamFromDouble(psdUnitFloat.getValue())));
                            break;
                        }
                    case PsdEnum psdEnum:
                        {
                            mass.Add(ob.Key.Trim(), psdEnum.getValue());
                            break;
                        }
                    case PsdText psdText:
                        {
                            mass.Add(ob.Key.Trim(), psdText.getValue());
                            break;
                        }
                    case PsdDescriptor psdDescriptor:
                        {
                            mass.Add(ob.Key.Trim(), Dictionary(psdDescriptor.getObjects()));
                            break;
                        }
                    case PsdList psdList:
                        {
                            mass.Add(ob.Key.Trim(), List(psdList.getObjects()));
                            break;
                        }
                    case PsdTextData psdTextData:
                        {
                            var engine = (object)psdTextData.getProperties();
                            var engineData = (Dictionary<string, object>)engine;
                            mass.Add(ob.Key.Trim(), engineData);
                            break;
                        }
                    case PsdObject psdObject:
                        {
                            mass.Add(ob.Key.Trim(), "psdObject");
                            break;
                        }
                }
            }


            return mass;
        }
        #endregion

        #region List
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objects"></param>
        public static List<object> List(List<PsdObject> objects)
        {
            List<object> mass = new List<object>();

            foreach (var ob in objects)
            {
                switch (ob)
                {
                    case PsdDouble psdDouble:
                        {
                            mass.Add(PsdCommon.ReadPSDDouble(PsdCommon.GetByteStreamFromDouble(psdDouble.getValue())));
                            break;
                        }
                    case PsdLong psdLong:
                        {
                            mass.Add(psdLong.getValue());
                            break;
                        }
                    case PsdBoolean psdBoolean:
                        {
                            mass.Add(psdBoolean.getValue());
                            break;
                        }
                    case PsdUnitFloat psdUnitFloat:
                        {
                            mass.Add(PsdCommon.ReadPSDDouble(PsdCommon.GetByteStreamFromDouble(psdUnitFloat.getValue())));
                            break;
                        }
                    case PsdEnum psdEnum:
                        {
                            mass.Add(psdEnum.getValue());
                            break;
                        }
                    case PsdText psdText:
                        {
                            mass.Add(psdText.getValue());
                            break;
                        }
                    case PsdDescriptor psdDescriptor:
                        {
                            mass.Add(Dictionary(psdDescriptor.getObjects()));
                            break;
                        }
                    case PsdList psdList:
                        {
                            mass.Add(List(psdList.getObjects()));
                            break;
                        }
                    case PsdTextData psdTextData:
                        {
                            var engine = (object)psdTextData.getProperties();
                            var engineData = (Dictionary<string, object>)engine;
                            mass.Add(engineData);
                            break;
                        }
                    case PsdObject psdObject:
                        {
                            mass.Add("psdObject");
                            break;
                        }
                }
            }


            return mass;
        }
        #endregion
    }
}
