using System;
using System.Runtime.Serialization;

namespace Arc4u.OAuth2.Token
{
    [DataContract]
    public class TokenInfo
    {

        public TokenInfo(string tokenType, string token, DateTime expiresOnUtc)
        {
            this.TokenType = tokenType;
            this.Token = token;
            this.ExpiresOnUtc = expiresOnUtc.ToUniversalTime();
        }


        /// <summary>
        /// Gets the type of the Access Token returned. 
        /// </summary>
        [DataMember]
        public string TokenType { get; private set; }

        /// <summary>
        /// Gets the Access Token requested.
        /// </summary>
        [DataMember]
        public string Token { get; internal set; }

        /// <summary>
        /// Gets the point in time in which the Access Token returned in the AccessToken property ceases to be valid.
        /// This value is calculated based on current UTC time measured locally and the value expiresIn received from the service.
        /// </summary>
        [DataMember]
        public DateTime ExpiresOnUtc { get; internal set; }

    }
}
