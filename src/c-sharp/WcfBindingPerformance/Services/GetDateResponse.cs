using System;
using System.Runtime.Serialization;

namespace WcfBindingPerformance.Services
{
    [DataContract]
    public class GetDateResponse
    {
        [DataMember(Order = 1)]
        public DateTime ResponseTime { get; set; }
    }
}