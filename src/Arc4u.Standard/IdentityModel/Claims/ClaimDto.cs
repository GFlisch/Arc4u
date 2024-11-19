using System.Runtime.Serialization;

namespace Arc4u.IdentityModel.Claims
{
    [DataContract]
    public class ClaimDto
    {
        public ClaimDto()
        {

        }

        public ClaimDto(string type, string value)
        {
            ClaimType = type;

            Value = value;
        }

        [DataMember(Name = "claimType")]
        public String ClaimType { get; set; }

        [DataMember(Name = "value")]
        public String Value { get; set; }

        public override string ToString()
        {
            return String.Format("{0} : {1}", ClaimType, Value);
        }
    }
}
