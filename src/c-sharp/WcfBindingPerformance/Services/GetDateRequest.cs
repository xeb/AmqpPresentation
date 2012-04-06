using System;
using System.Runtime.Serialization;

namespace WcfBindingPerformance.Services
{
    [DataContract]
    public class GetDateRequest
    {
        [DataMember(Order = 1)]
        public DateTime RequestTime { get; set; }
    }
}