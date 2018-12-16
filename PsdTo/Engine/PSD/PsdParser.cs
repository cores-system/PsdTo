using Endogine;
using Endogine.Serialization;
using Newtonsoft.Json;
using PhotoshopFile.LayerResources;
using PhotoShopFile;
using PsdTo.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;

namespace PsdTo.Engine
{
    public static class PsdParser
    {
        #region FontSize
        /// <summary>
        /// 
        /// </summary>
        /// <param name="d"></param>
        public static double FontSize(Dictionary<string, object> d)
        {
            try
            {
                return (double)TdTaParser.query(d, "FontSize");
            }
            catch { return 16; }
        }
        #endregion

        #region FontName
        /// <summary>
        /// 
        /// </summary>
        /// <param name="txt"></param>
        /// <param name="styleSheetSet"></param>
        /// <param name="d"></param>
        public static string FontName(TypeToolTyShPH6 txt, Dictionary<string, object> styleSheetSet, Dictionary<string, object> d)
        {
            try
            {
                // Font из StyleRun
                return TdTaParser.getString(txt.StylesheetReader.getFontSet()[Convert.ToInt32(TdTaParser.query(d, "Font").ToString())], "Name$");
            }
            catch { }
            
            // Font из StyleSheetSet
            if (styleSheetSet.TryGetValue("Font", out object fontIndexOb) && double.TryParse(fontIndexOb.ToString(), out double fontIndex))
                return TdTaParser.getString(txt.StylesheetReader.getFontSet()[(int)fontIndex], "Name$");

            // хз
            return string.Empty;
        }
        #endregion

        #region FontColor
        public static Color FontColor(TypeToolTyShPH6 txt, Dictionary<string, object> styleSheetSet, Dictionary<string, object> d)
        {
            try
            {
                // Текущий цвет
                return TdTaParser.getColor(d, "FillColor");
            }
            catch { }

            try
            {
                // Если не указан текущий то цвет по умолчанию
                if (styleSheetSet.TryGetValue("FillColor", out dynamic fillColor))
                {
                    //Get the array of values
                    List<object> values = fillColor["Values"] as List<object>;
                    return Color.FromArgb((int)Math.Round((double)values[0] * 255), (int)Math.Round((double)values[1] * 255), (int)Math.Round((double)values[2] * 255), (int)Math.Round((double)values[3] * 255));
                }
            }
            catch { }

            // На угад
            return Color.Black;
        }
        #endregion

        #region StyleRun
        public static object StyleRun(TypeToolTyShPH6 txt)
        {
            var styleRun = ((dynamic)txt.engineData["EngineDict"])["StyleRun"];

            foreach (Dictionary<string, object> runArray in styleRun["RunArray"])
            {
                foreach (var styleSheet in runArray)
                {
                    foreach (var styleSheetData in (Dictionary<string, object>)styleSheet.Value)
                    {
                        var val = (Dictionary<string, object>)styleSheetData.Value;

                        // Font
                        if (val.TryGetValue("Font", out object fontIndexOb) && double.TryParse(fontIndexOb.ToString(), out double fontIndex))
                            val["Font"] = TdTaParser.getString(txt.StylesheetReader.getFontSet()[(int)fontIndex], "Name$");

                        // FillColor
                        if (val.TryGetValue("FillColor", out dynamic fillColor))
                        {
                            List<object> values = fillColor["Values"] as List<object>;
                            Color color = Color.FromArgb((int)Math.Round((double)values[0] * 255), (int)Math.Round((double)values[1] * 255), (int)Math.Round((double)values[2] * 255), (int)Math.Round((double)values[3] * 255));
                            val["FillColor"] = new ColorRBG() { A = color.A, B = color.B, R = color.R, G = color.G, };
                        }

                        // мусор
                        val.Remove("StrokeColor");
                    }
                }
            }

            return styleRun;
        }
        #endregion
    }
}
