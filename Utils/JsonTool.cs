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
            JsonSerializer jsonSerializer = new JsonSerializer();
            using var memStream = new MemoryStream();
            using var sw = new StreamWriter(memStream);
            using var jsonWriter = new JsonTextWriter(sw);
            jsonSerializer.Serialize(jsonWriter, data);

            using var reader = new StreamReader(memStream);

            return reader.ReadToEnd();
        }
        
        public static object DeserializeFromString(Type type, string json)
        {
            var jsonSerializer = new JsonSerializer();
            using var memStream = new MemoryStream();
            var bytes = Encoding.UTF8.GetBytes(json);
            memStream.Write(bytes, 0, bytes.Length);
            using var sr = new StreamReader(memStream);
            using var jsonReader = new JsonTextReader(sr);
            var jObject = jsonSerializer.Deserialize(jsonReader) as JObject;
            return jObject.ToObject(type);
        }

        public static void Serialize(object data, string filePath)
        {
            JsonSerializer jsonSerializer = new JsonSerializer();
            if (File.Exists(filePath)) File.Delete(filePath);
            using var sw = new StreamWriter(filePath);
            using var jsonWriter = new JsonTextWriter(sw);
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