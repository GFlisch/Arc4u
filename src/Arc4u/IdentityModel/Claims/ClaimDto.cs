using System.Globalization;
using System.Runtime.Serialization;

namespace Arc4u.IdentityModel.Claims;

[DataContract]
public class ClaimDto
{
    public ClaimDto()
    {
        ClaimType = string.Empty;
        Value = string.Empty;
    }

    public ClaimDto(string type, string value)
    {
        ClaimType = type;

        Value = value;
    }

    [DataMember(Name = "claimType")]
    public string ClaimType { get; set; }

    [DataMember(Name = "value")]
    public string Value { get; set; }

    public override string ToString()
    {
        return string.Format(CultureInfo.InvariantCulture, "{0} : {1}", ClaimType, Value);
    }
}
