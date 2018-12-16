using Endogine.Serialization;
using Newtonsoft.Json;
using PhotoShopFile;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace PsdTo.Engine.TypeTools
{
    [Description("lfx2")]
    public class TypeToollfx2PH6
    {
        public PsdDescriptor fxDescriptor;
        public Dictionary<string, object> engineData = new Dictionary<string, object>();

        public TypeToollfx2PH6() { }
        public TypeToollfx2PH6(BinaryReverseReader r)
        {
            ///////////////////////////////////////////////////////////////////
            // https://www.adobe.com/devnet-apps/photoshop/fileformatashtml/ //
            ///////////////////////////////////////////////////////////////////

            this.fxDescriptor = new PsdDescriptor(r);
            engineData = ObjectsTo.Dictionary(fxDescriptor.getObjects());


            // Достаем список отключеных эфектов
            List<string> removeFx = new List<string>();
            foreach (var engine in engineData)
            {
                #region Локальный метод - "RmToFx"
                bool RmToFx(object ob)
                {
                    if (ob is Dictionary<string, object> val)
                    {
                        if (val.TryGetValue("enab", out object enabOb) && bool.TryParse(enabOb.ToString(), out bool enab))
                        {
                            if (enab == false)
                                removeFx.Add(engine.Key);

                            // Это эфект и он оставлен или удален, но всегда true
                            return true;
                        }
                    }

                    // Это не эфект
                    return false;
                }
                #endregion

                if (!RmToFx(engine.Value))
                {
                    if (engine.Value is List<object> mass)
                    {
                        foreach (var item in mass)
                            RmToFx(item);
                    }
                }
            }

            // Удаляем отключеные эфекты
            foreach (var key in removeFx)
                engineData.Remove(key);


            // Меняем RGB
            string json = JsonConvert.SerializeObject(engineData, Formatting.Indented);
            json = Regex.Replace(json, "\"(Rd|Grn|Bl)\": ([0-9\\.]+)", (g) =>
            {
                var val = (int)Math.Round(double.Parse(g.Groups[2].Value.Replace(".", ",")));

                switch (g.Groups[1].Value)
                {
                    case "Rd":
                        return $"\"R\": {val}";
                    case "Grn":
                        return $"\"G\": {val}";
                    case "Bl":
                        return $"\"B\": {val}";
                }

                return g.Groups[0].Value;
            });

            // 
            engineData = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

            // 
            r.Dispose();
        }
    }
}
