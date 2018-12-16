using System;
using System.IO;
using System.Linq;
using System.Text;

namespace PhotoshopFile
{
    ///////////////////////////////////////////////////////////////////
    // https://www.adobe.com/devnet-apps/photoshop/fileformatashtml/ //
    ///////////////////////////////////////////////////////////////////
    
    public class PsdStream : PsdFile
    {
        public static void Load(string filename, Encoding encoding, bool saveImg, Action<Layer> callback) {
            new PsdStream(filename, encoding, saveImg, callback);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="encoding"></param>
        /// <param name="saveImg"></param>
        /// <param name="callback"></param>
        public PsdStream(string filename, Encoding encoding, bool saveImg, Action<Layer> callback) : base()
        {
            using (var stream = new FileStream(filename, FileMode.Open))
            {
                var reader = new PsdBinaryReader(stream, encoding);
                
                LoadHeader(reader);
                LoadColorModeData(reader);
                LoadImageResources(reader);

                // 
                LoadLayerAndMaskInfo(reader, saveImg, callback);
            }
        }

        #region LoadLayerAndMaskInfo
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="SaveImage"></param>
        private void LoadLayerAndMaskInfo(PsdBinaryReader reader, bool SaveImage, Action<Layer> callback)
        {
            var layersAndMaskLength = reader.ReadUInt32();
            if (layersAndMaskLength <= 0)
                return;

            var startPosition = reader.BaseStream.Position;
            var endPosition = startPosition + layersAndMaskLength;
            
            LoadLayers(reader, true, SaveImage, callback);
            LoadGlobalLayerMask(reader);

            //-----------------------------------------------------------------------
            // Load Additional Layer Information

            while (reader.BaseStream.Position < endPosition)
            {
                var info = LayerInfoFactory.Load(reader, true);
                AdditionalInfo.Add(info);

                if (info is RawLayerInfo)
                {
                    var layerInfo = (RawLayerInfo)info;
                    switch (info.Key)
                    {
                        case "Layr":
                        case "Lr16":
                        case "Lr32":
                            using (var memoryStream = new MemoryStream(layerInfo.Data))
                            using (var memoryReader = new PsdBinaryReader(memoryStream, reader))
                            {
                                LoadLayers(memoryReader, false, SaveImage, callback);
                            }
                            break;

                        case "LMsk":
                            GlobalLayerMaskData = layerInfo.Data;
                            break;
                    }
                }
            }

            //-----------------------------------------------------------------------
            // make sure we are not on a wrong offset, so set the stream position 
            // manually
            reader.BaseStream.Position = startPosition + layersAndMaskLength;
        }
        #endregion

        #region LoadLayers
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="hasHeader"></param>
        /// <param name="SaveImage"></param>
        private void LoadLayers(PsdBinaryReader reader, bool hasHeader, bool SaveImage, Action<Layer> callback)
        {
            UInt32 sectionLength = 0;
            if (hasHeader)
            {
                sectionLength = reader.ReadUInt32();
                if (sectionLength <= 0)
                    return;
            }

            var startPosition = reader.BaseStream.Position;
            var numLayers = reader.ReadInt16();

            // If numLayers < 0, then number of layers is absolute value,
            // and the first alpha channel contains the transparency data for
            // the merged result.
            if (numLayers < 0)
            {
                AbsoluteAlpha = true;
                numLayers = Math.Abs(numLayers);
            }
            if (numLayers == 0)
                return;

            for (int i = 0; i < numLayers; i++)
            {
                var layer = new Layer(reader, this);
                Layers.Add(layer); // not work => Layers.Insert(0, layer);
            }

            #region Скрываем слои в скрытых папках
            bool LayerSectionVisible = true;
            int OpenCountLayerSection = 0;

            // Реверс списка
            foreach (var ly in Layers.AsEnumerable().Reverse())
            {
                // Начало или конец папки
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
                else
                {
                    // Папка скрыта
                    if (!LayerSectionVisible)
                        ly.Visible = false;
                }
            }
            #endregion

            #region Load image data for all channels
            foreach (var layer in Layers)
            {
                if (SaveImage)
                {
                    foreach (var channel in layer.Channels)
                    {
                        channel.LoadPixelData(reader);
                    }
                    
                    InvokeImg(layer, callback);
                }
                else
                {
                    callback?.Invoke(layer);
                }
            }
            #endregion

            // Length is set to 0 when called on higher bitdepth layers.
            if (sectionLength > 0)
            {
                // Layers Info section is documented to be even-padded, but Photoshop
                // actually pads to 4 bytes.
                var endPosition = startPosition + sectionLength;
                var positionOffset = reader.BaseStream.Position - endPosition;


                if (reader.BaseStream.Position < endPosition)
                    reader.BaseStream.Position = endPosition;
            }
        }
        #endregion

        #region InvokeImg
        private void InvokeImg(Layer layer, Action<Layer> callback)
        {
            // Декодируем изображение
            foreach (var channel in layer.Channels)
            {
                if (channel.ImageDataRaw == null)
                    continue;

                channel.DecodeImageData();

                if (channel.ID == -2)
                    layer.Masks.LayerMask.ImageData = channel.ImageData;
                else if (channel.ID == -3)
                    layer.Masks.UserMask.ImageData = channel.ImageData;
            }

            // Вызываем callback
            callback?.Invoke(layer);

            #region Чистим ресурсы
            if (layer.Masks != null)
            {
                if (layer.Masks.LayerMask != null)
                    layer.Masks.LayerMask.ImageData = null;

                if (layer.Masks.UserMask != null)
                    layer.Masks.UserMask.ImageData = null;
            }

            // Чистим ресурсы на каналах
            foreach (var channel in layer.Channels)
            {
                channel.ImageData = null;
                channel.ImageDataRaw = null;
            }
            #endregion
        }
        #endregion
    }
}
