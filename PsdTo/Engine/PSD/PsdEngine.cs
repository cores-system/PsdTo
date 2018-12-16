using Endogine.Serialization;
using Newtonsoft.Json;
using PhotoshopFile;
using PhotoshopFile.LayerResources;
using PhotoShopFile;
using PsdTo.Engine.TypeTools;
using PsdTo.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace PsdTo.Engine
{
    public static class PsdEngine
    {
        static int index;
        static RootLayer md;
        static bool LayerSectionVisible = true;
        static int OpenCountLayerSection = 0;

        #region Invoke
        public static RootLayer Invoke(string filePsd, string BaseImageDir, string ReplaceToImageDir, bool saveImg)
        {
            // Модель
            md = new RootLayer();

            #region Удаляем существующие файлы
            foreach (var rmFile in Directory.EnumerateFiles(BaseImageDir))
            {
                try
                {
                    File.Delete(rmFile);
                }
                catch { }
            }
            #endregion

            // Считываем файл
            PsdStream.Load(filePsd, Encoding.BigEndianUnicode, saveImg, (ly) => 
            {
                #region Начало или конец папки
                if (ly.AdditionalInfo.SingleOrDefault(x => x is LayerSectionInfo) != null)
                {
                    if (LayerSectionVisible)
                    {
                        if (ly.Visible == false)
                        {
                            LayerSectionVisible = false;
                            OpenCountLayerSection = 1;
                        }
                    }
                    else
                    {
                        if (ly.Name == "</Layer group>")
                        {
                            // Конец папки
                            OpenCountLayerSection--;

                            // Скрытая папка закрыта
                            if (OpenCountLayerSection == 0)
                            {
                                LayerSectionVisible = true;
                            }
                        }
                        else
                        {
                            // Новая папка в папке
                            OpenCountLayerSection++;
                        }
                    }
                }
                #endregion

                #region Другие слои
                else
                {
                    // Папка скрыта
                    if (!LayerSectionVisible)
                        ly.Visible = false;

                    // Эфекты
                    var iflfx2 = ly.AdditionalInfo.SingleOrDefault(x => x.Key == "lfx2");

                    // 
                    var iftext = ly.AdditionalInfo.SingleOrDefault(x => x.Key == "TySh");
                    var ifshape1 = ly.AdditionalInfo.SingleOrDefault(x => x.Key == "vogk");
                    var ifshape2 = ly.AdditionalInfo.SingleOrDefault(x => x.Key == "vstk");
                    var ifshape3 = ly.AdditionalInfo.SingleOrDefault(x => x.Key == "vscg");
                    var ifshape4 = ly.AdditionalInfo.SingleOrDefault(x => x.Key == "vmsk");

                    if (iftext != null)
                    {
                        TextInvoke(ly, iftext, iflfx2);
                    }
                    else if (ifshape1 != null || ifshape2 != null || ifshape3 != null || ifshape4 != null)
                    {
                        ShapeInvoke(ly, iflfx2, ifshape1, ifshape2, ifshape3, ifshape4, BaseImageDir, ReplaceToImageDir, saveImg);
                    }
                    else
                    {
                        ImageInvoke(ly, iflfx2, BaseImageDir, ReplaceToImageDir, saveImg);
                    }
                }
                #endregion
            });

            // Отдаем модель
            return md;
        }
        #endregion


        #region TextInvoke
        private static void TextInvoke(Layer ly, LayerInfo text, LayerInfo lfx2)
        {
            TypeToolTyShPH6 txt = new TypeToolTyShPH6(PsdCommon.CreateStream(text));
            Dictionary<string, object> d = txt.StylesheetReader.GetStylesheetDataFromLongestRun();
            Dictionary<string, object> styleSheetSet = txt.StylesheetReader.GetStyleSheetSet();


            //Console.WriteLine(JsonConvert.SerializeObject(ObjectsTo.Dictionary(txt.TxtDescriptor.getObjects()), Formatting.Indented));
            //Console.ReadLine();

            var Text = txt.StylesheetReader.Text;

            double lineHeight = 1.2;
            Color fontColor = PsdParser.FontColor(txt, styleSheetSet, d);

            try
            {
                #warning Ошибка у некоторых вылетает
                lineHeight = (double)TdTaParser.query(d, "Leading");
            }
            catch { }


            md.TextLayers.Add(new TextLayer()
            {
                Id = GetTolyid(ly),
                index = index++,
                Text = Text,
                X = ly.Rect.X,
                Y = ly.Rect.Y,
                Width = ly.Rect.Width,
                Height = ly.Rect.Height,
                Visible = ly.Visible,
                Opacity = ly.Opacity,
                fontName = PsdParser.FontName(txt, styleSheetSet, d),
                fontSize = PsdParser.FontSize(d),
                lineHeight = lineHeight,
                Color = new ColorRBG() { A = fontColor.A, B = fontColor.B, R = fontColor.R, G = fontColor.G, },
                Name = ly.Name,
                StyleRun = PsdParser.StyleRun(txt),
                Fx = GetTolfx2(lfx2),
                Clipping = ly.Clipping,
                Transform = new Transform()
                {
                    xx = txt.Transform.M11,
                    xy = txt.Transform.M12,
                    yx = txt.Transform.M13,
                    yy = txt.Transform.M21,
                    tx = txt.Transform.M22,
                    ty = txt.Transform.M23,
                }
            });
        }
        #endregion

        #region ShapeInvoke
        private static void ShapeInvoke(Layer ly, LayerInfo lfx2, LayerInfo ifshape1, LayerInfo ifshape2, LayerInfo ifshape3, LayerInfo ifshape4, string BaseImageDir, string ReplaceToImageDir, bool saveImg)
        {
            // radius is implemented only for photoshop shapetool objects
            int radius = 0;

            Color bordercolor = Color.Black;
            Color color = Color.Black;

            double lineThickness = 0;
            if (ifshape1 != null)
            {
                PhotoShopFile.PsdDescriptor descr = new PhotoShopFile.PsdDescriptor(PsdCommon.CreateStream(ifshape1));
                radius = PsdCommon.GetRadius(descr);
            }
            if (ifshape2 != null)
            {
                PsdCommon.ReturnedObject = null;
                PhotoShopFile.PsdDescriptor descrvstroke = new PhotoShopFile.PsdDescriptor(PsdCommon.CreateStream(ifshape2));
                var getobj = descrvstroke.getObjects();
                PsdCommon.GetObject(getobj, "strokeStyleLineWidth");
                PsdUnitFloat b = (PsdUnitFloat)PsdCommon.ReturnedObject;
                if (b != null)
                {

                    lineThickness = PsdCommon.ReadPSDDouble(PsdCommon.GetByteStreamFromDouble(b.getValue()));
                }

                bordercolor = PsdCommon.GetPSDColorRBG(descrvstroke);
            }
            if (ifshape3 != null)
            {
                PsdCommon.ReturnedObject = null;
                PhotoShopFile.PsdDescriptor descrvstroke = new PhotoShopFile.PsdDescriptor(PsdCommon.CreateStream(ifshape3));
                color = PsdCommon.GetPSDColorRBG(descrvstroke);
            }
            if (ifshape4 != null)
            {
                var effprop = ly.AdditionalInfo.SingleOrDefault(x => x.Key == "lfx2");
                var solidColor = ly.AdditionalInfo.SingleOrDefault(x => x.Key == "SoCo");
                if (effprop != null)
                {
                    PsdCommon.ReturnedObject = null;
                    PhotoShopFile.PsdDescriptor eff = new PhotoShopFile.PsdDescriptor(PsdCommon.CreateStream(effprop));
                    var getobj = eff.getObjects();
                    PsdCommon.GetObject(getobj, "FrFX");
                    if (PsdCommon.ReturnedObject != null)
                    {
                        PsdDescriptor strokedescr = (PsdDescriptor)PsdCommon.ReturnedObject;
                        var strokeobj = strokedescr.getObjects();
                        PsdUnitFloat size = (PsdUnitFloat)strokeobj["Sz  "];
                        lineThickness = PsdCommon.ReadPSDDouble(PsdCommon.GetByteStreamFromDouble(size.getValue()));
                        bordercolor = PsdCommon.GetPSDColorRBG(strokedescr);
                    }
                }
                if (solidColor != null)
                {
                    PsdCommon.ReturnedObject = null;
                    PhotoShopFile.PsdDescriptor eff = new PhotoShopFile.PsdDescriptor(PsdCommon.CreateStream(solidColor));
                    color = PsdCommon.GetPSDColorRBG(eff);
                }
                //color = GetColor(eff, "SoFi");
            }

            var res = ImgEngine.SaveBitmap(ly, ly.Name, BaseImageDir, saveImg);

            md.ShapeLayers.Add(new ShapeLayer()
            {
                Id = GetTolyid(ly),
                index = index++,
                Name = ly.Name,
                X = ly.Rect.X,
                Y = ly.Rect.Y,
                Width = ly.Rect.Width,
                Height = ly.Rect.Height,
                Visible = ly.Visible,
                Opacity = ly.Opacity,
                Image = res.source.Replace(BaseImageDir, ReplaceToImageDir),
                IsDublicate = res.IsDublicate,
                Color = new ColorRBG() { A = color.A, B = color.B, R = color.R, G = color.G, },
                BorderColor = new ColorRBG() { A = bordercolor.A, B = bordercolor.B, R = bordercolor.R, G = bordercolor.G, },
                radius = radius,
                lineThickness = lineThickness,
                Fx = GetTolfx2(lfx2),
                Clipping = ly.Clipping
            });
        }
        #endregion

        #region ImageInvoke
        private static void ImageInvoke(Layer ly, LayerInfo lfx2, string BaseImageDir, string ReplaceToImageDir, bool saveImg)
        {
            var res = ImgEngine.SaveBitmap(ly, ly.Name, BaseImageDir, saveImg);

            if (!res.ErrorDecodeImage)
            {
                md.ImageLayers.Add(new ImageLayer()
                {
                    Id = GetTolyid(ly),
                    index = index++,
                    Name = ly.Name,
                    X = ly.Rect.X,
                    Y = ly.Rect.Y,
                    Width = ly.Rect.Width,
                    Height = ly.Rect.Height,
                    Visible = ly.Visible,
                    Opacity = ly.Opacity,
                    IsDublicate = res.IsDublicate,
                    Source = res.source.Replace(BaseImageDir, ReplaceToImageDir),
                    Fx = GetTolfx2(lfx2),
                    Clipping = ly.Clipping
                });
            }
        }
        #endregion


        #region GetTolfx2
        private static Dictionary<string, object> GetTolfx2(LayerInfo iflfx2)
        {
            if (iflfx2 != null)
            {
                TypeToollfx2PH6 lfx2 = new TypeToollfx2PH6(PsdCommon.CreateStream(iflfx2));
                return lfx2.engineData;
            }

            return null;
        }
        #endregion

        #region GetTolyid
        private static long GetTolyid(Layer ly)
        {
            if (ly != null && ly.AdditionalInfo.SingleOrDefault(x => x.Key == "lyid") is LayerInfo lyid && lyid != null)
            {
                using (BinaryReverseReader reader = PsdCommon.CreateStream(lyid))
                {
                    return reader.ReadInt32();
                }
            }

            return -1;
        }
        #endregion
    }
}
