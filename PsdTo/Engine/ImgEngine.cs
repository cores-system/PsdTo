using PhotoshopFile;
using PhotoShopFile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PsdTo.Engine
{
    public static class ImgEngine
    {
        static int tmpFileCount = 0;
        static int pngFileCount = 0;

        #region SaveBitmap
        public static (string source, bool IsDublicate, bool ErrorDecodeImage) SaveBitmap(Layer ly, string LayerName, string BaseImageDir, bool saveImg)
        {
            string source = string.Empty;

            if (saveImg)
            {
                var bitmap = ImageDecoder.DecodeImage(ly);
                if (bitmap == null)
                    return (source, false, true);

                try
                {
                    // Временный файл
                    string tmpFile = BaseImageDir + $"tmp_{tmpFileCount}.png";
                    tmpFileCount++;

                    // Сохраняем картинку
                    bitmap.Save(tmpFile);
                    int ImgWidth = bitmap.Width;
                    int ImgHight = bitmap.Height;
                    bitmap.Dispose();

                    // Проверяем дубликаты
                    if (GetDublicate(BaseImageDir, (new FileInfo(tmpFile)).Length, ImgWidth, ImgHight, out source))
                    {
                        File.Delete(tmpFile);
                        return (source, true, false);
                    }
                    else
                    {
                        source = BaseImageDir + PsdCommon.LayerName(LayerName) + "_" + pngFileCount + ".png";
                        File.Move(tmpFile, source);
                        pngFileCount++;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    throw new Exception();
                }
            }

            return (source, false, false);
        }
        #endregion

        #region GetDublicate
        /// <summary>
        /// /
        /// </summary>
        /// <param name="BaseImageDir"></param>
        /// <param name="size"></param>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <param name="source"></param>
        private static bool GetDublicate(string BaseImageDir, long size, int Width, int Height, out string source)
        {
            foreach (var file in Directory.GetFiles(BaseImageDir, "*.*"))
            {
                if (Regex.IsMatch(file, @"(\\|/)tmp_[0-9]+\.png$"))
                    continue;

                var imgInfo = ImageInfo(file);
                if (Width == imgInfo.Width && Height == imgInfo.Height && (new FileInfo(file)).Length == size)
                {
                    source = file;
                    return true;
                }
            }

            source = null;
            return false;
        }
        #endregion

        #region ImageInfo
        static Dictionary<string, (int Width, int Height)> massImages = new Dictionary<string, (int Width, int Height)>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileImg"></param>
        private static (int Width, int Height) ImageInfo(string fileImg)
        {
            if (massImages.TryGetValue(fileImg, out var res))
                return res;

            using (var fs = File.OpenRead(fileImg))
            {
                byte[] header = new byte[8];
                fs.Read(header, 0, 8);
                if (!header.SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }))
                    throw new InvalidOperationException("not a png file");

                var br = new BinaryReader(fs, Encoding.ASCII, true);
                while (fs.Position != fs.Length)
                {
                    uint chunkLength = br.ReadUInt32BE();
                    string chunkType = new string(br.ReadChars(4));
                    if (chunkType == "IHDR")
                    {
                        // читаем чанк
                        uint width = br.ReadUInt32BE();
                        uint height = br.ReadUInt32BE();
                        //byte bitDepth = br.ReadByte();
                        //byte colorType = br.ReadByte();
                        //byte comprMethod = br.ReadByte();
                        //byte filterMethod = br.ReadByte();
                        //byte interlaceMethod = br.ReadByte();
                        massImages.Add(fileImg, ((int)width, (int)height));
                        return ((int)width, (int)height);
                    }
                    else
                        // пропускаем чанк
                        fs.Position += chunkLength;
                    uint crc = br.ReadUInt32BE();
                }
            }

            return (0, 0);
        }
        #endregion
    }
}
