using System.Runtime.Serialization;

namespace Arc4u.ServiceModel
{
    [DataContract]
    public class LocalizedMessage
    {
        [DataMember(Name = "t")]
        public string Type { get; set; }

        [DataMember(Name = "m")]
        public string Message { get; set; }

        [DataMember(Name = "p")]
        public object[] Parameters { get; set; }
    }
}
