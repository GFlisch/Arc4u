using System.Runtime.Serialization;

namespace Arc4u.ServiceModel;

/// <summary>
/// Specifies the different categories of a <see cref="FaultMessage"/>.
/// </summary>
[DataContract]
public enum MessageCategory : short
{
    /// <summary>
    /// A message dedicates for IT people.
    /// </summary>
    [EnumMember]
    Technical = 0,
    /// <summary>
    /// A message dedicates for IT and Business people.
    /// </summary>
    [EnumMember]
    Business = 1,
}
