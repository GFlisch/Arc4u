using System.Runtime.Serialization;

namespace Arc4u.ServiceModel;

[DataContract]
public class LocalizedMessage
{
    [DataMember(Name = "t")]
    public string Type { get; set; } = string.Empty;

    [DataMember(Name = "m")]
    public string Message { get; set; } = string.Empty;

    [DataMember(Name = "p")]
    public object[] Parameters { get; set; } = [];
}
