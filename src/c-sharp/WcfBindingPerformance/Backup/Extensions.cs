using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace WcfBindingPerformance
{
    public static class Extensions
    {
        public static string Serialize<T>(this T instance)
        {
            using (var ms = new MemoryStream())
            {
                var d = new DataContractJsonSerializer(typeof(T));
                d.WriteObject(ms, instance);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        public static T Deserialize<T>(this string data)
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(data)))
            {
                var d = new DataContractJsonSerializer(typeof(T));
                return (T)d.ReadObject(ms);
            }
        }
    }
}