using System.Runtime.Serialization;

namespace WcfBindingPerformance.Models
{
    [DataContract(Name = "SomeObject", Namespace = "Root")]
    public class SomeObject
    {
        [DataMember]
        public int Id { get; set;}

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public double ElapsedTime { get; set; }
    }
}