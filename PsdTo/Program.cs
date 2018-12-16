using Newtonsoft.Json;
using PsdTo.Engine;
using System;
using System.Text;

namespace PsdTo
{
    class Program
    {
        static void Main(string[] args)
        {
            //args = new string[] { @"C:\Users\htc\Desktop\Untitled-1.psd", @"C:\Users\htc\Desktop\мусор\", @"C:\Users\htc\Desktop\мусор\", @"true" };

            string filePsd = args[0];
            string BaseImageDir = args[1];
            string ReplaceToImageDir = args[2];
            bool saveImg = bool.Parse(args[3]);

            // UTF-8
            Console.OutputEncoding = Encoding.UTF8;
            
            // Загружаем PSD и сохраняем изображения
            var model = PsdEngine.Invoke(filePsd, BaseImageDir, ReplaceToImageDir, saveImg);

            // Model to Json
            string json = JsonConvert.SerializeObject(model); // Formatting.Indented

            // Выводим json
            Console.WriteLine(json);

            //Console.ReadLine();
            //System.IO.File.WriteAllText(@"C:\Users\htc\Desktop\11\json3.txt", json);
        }
    }
}
