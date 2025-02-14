using System.Runtime.Serialization;

namespace Arc4u;

/// <summary>
/// Specifies the possible directions of a <see cref="Bound&lt;T&gt;"/>.
/// </summary>
[DataContract]
public enum BoundDirection
{
    /// <summary>
    /// Indicates an opened <see cref="Bound&lt;T&gt;"/>.
    /// </summary>
    [EnumMember]
    Opened = 0,
    /// <summary>
    /// Indicates a closed <see cref="Bound&lt;T&gt;"/>.
    /// </summary>
    [EnumMember]
    Closed = 1,
}
