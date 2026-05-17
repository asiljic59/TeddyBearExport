using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.IO;


namespace TeddyBearExport.Repository
{
    public class TarifeRepository
    {
        private readonly ConcurrentDictionary<String, JsonObject> _sheetCache = new ConcurrentDictionary<string, JsonObject>();

        public JsonObject? getSheetData(int tarifa)
        {
            string filename = $"Tarifa_{tarifa}.json";

            return _sheetCache.GetOrAdd(filename, name =>
            {
                return LoadSheetData(name);
            });
        }

        private JsonObject? LoadSheetData(string name)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", name);

            if (!File.Exists(path)) return null;

            using FileStream stream = File.OpenRead(path);
            return JsonNode.Parse(stream) as JsonObject;
        }
    }
}
