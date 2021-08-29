using System.IO;
using System;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TjulfarBot.Net.Utils
{

    class JsonTool
    {

        public static string Serialize(object data)
        {
            return JsonConvert.SerializeObject(data, Formatting.Indented);
        }
        
        public static object DeserializeFromString(Type type, string json)
        {
            return (JsonConvert.DeserializeObject(json) as JObject)?.ToObject(type);
        }

        public static void Serialize(object data, string filePath)
        {
            JsonSerializer jsonSerializer = new JsonSerializer();
            if (File.Exists(filePath)) File.Delete(filePath);
            using var sw = new StreamWriter(filePath);
            using var jsonWriter = new JsonTextWriter(sw);
            jsonSerializer.Formatting = Formatting.Indented;
            jsonSerializer.Serialize(jsonWriter, data);
        }

        public static object Deserialize(Type type, string filePath)
        {
            JObject jObject = null;
            var jsonSerializer = new JsonSerializer();
            if(File.Exists(filePath))
            {
                using var sr = new StreamReader(filePath);
                using var jsonReader = new JsonTextReader(sr);
                jObject = jsonSerializer.Deserialize(jsonReader) as JObject;
            }
            return jObject.ToObject(type);
        }

    }

}