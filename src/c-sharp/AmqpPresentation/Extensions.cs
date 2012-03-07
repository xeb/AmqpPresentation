using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using ProtoBuf;

namespace AmqpPresentation
{
    public static class SerializationExtensions
    {
        public static string ToJson<T>(this T obj)
        {
            using (var ms = new MemoryStream())
            {
                var dc = new DataContractJsonSerializer(typeof(T));
                dc.WriteObject(ms, obj);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        public static T FromJson<T>(this string json)
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                var dc = new DataContractJsonSerializer(typeof(T));
                return (T)dc.ReadObject(ms);
            }
        }

        public static byte[] Serialize<T>(this T obj)
        {
            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public static T Deserialize<T>(this byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                return Serializer.Deserialize<T>(ms);
            }
        }
    }
}
