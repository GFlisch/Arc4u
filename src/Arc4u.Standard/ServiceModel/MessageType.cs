using System.Runtime.Serialization;

namespace Arc4u.ServiceModel;

/// <summary>
/// Specifies the different types of a <see cref="FaultMessage"/>.
/// </summary>
[DataContract]
public enum MessageType
{
    /// <summary>
    /// Indicates Information.
    /// </summary>
    [EnumMember]
    Information = 0,
    /// <summary>
    /// Indicates Warning. 
    /// </summary>
    [EnumMember]
    Warning = 1,
    /// <summary>
    /// Indicates Error. 
    /// </summary>
    [EnumMember]
    Error = 2,
    /// <summary>
    /// Indicates Critical. 
    /// </summary>
    [EnumMember]
    Critical = 4,
}
