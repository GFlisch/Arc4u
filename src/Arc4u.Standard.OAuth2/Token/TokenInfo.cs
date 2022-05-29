using System;
using System.Runtime.Serialization;

namespace Arc4u.OAuth2.Token
{
    [DataContract]
    public class TokenInfo
    {

        public TokenInfo(string accessTokenType, string accessToken, string idToken, DateTime expiresOnUtc)
        {
            this.AccessTokenType = accessTokenType;
            this.AccessToken = accessToken;
            this.IdToken = idToken;
            this.ExpiresOnUtc = expiresOnUtc.ToUniversalTime();
        }


        /// <summary>
        /// Gets the type of the Access Token returned. 
        /// </summary>
        [DataMember]
        public string AccessTokenType { get; private set; }

        /// <summary>
        /// Gets the Access Token requested.
        /// </summary>
        [DataMember]
        public string AccessToken { get; internal set; }

        /// <summary>
        /// Gets the point in time in which the Access Token returned in the AccessToken property ceases to be valid.
        /// This value is calculated based on current UTC time measured locally and the value expiresIn received from the service.
        /// </summary>
        [DataMember]
        public DateTime ExpiresOnUtc { get; internal set; }

        /// <summary>
        /// Gets the entire Id Token if returned by the service or null if no Id Token is returned.
        /// </summary>
        [DataMember]
        public string IdToken { get; internal set; }

    }
}
