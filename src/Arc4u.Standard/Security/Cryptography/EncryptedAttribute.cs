using System;
using Arc4u.Extensions;

namespace Arc4u.Security.Cryptography
{
    /// <summary>
    /// Attribute to tell the framework, that the value is encrypted. 
    /// </summary>
    /// <remarks>
    /// This attribute is meant to be used for configuration classes, that can be automatically decrypt during initialization (see <see cref="ConfigurationExtension.GetAndDecrypt{TConfiguration}"/> for more information)
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property)]
    public class EncryptedAttribute : Attribute
    {
        public Filter FilterValue { get; }

        /// <summary>
        /// Defines that the property is configured as being decrypted within the provided configuration
        /// </summary>
        /// <param name="filter">Defintion of the encrypted value</param>
        public EncryptedAttribute(Filter filter = Filter.None)
        {
            FilterValue = filter;
        }

        /// <summary>
        /// Description on what part of the secret is relevant, by applying a filter
        /// </summary>
        /// <remarks>
        /// a secret is always defined as "username:password"
        /// </remarks>
        public enum Filter
        {
            /// <summary>
            /// No filter will be applied, so the decrypted value will be "username:password"
            /// </summary>
            None,
            
            /// <summary>
            /// filter will be applied, so that only the decrypted password will be set
            /// </summary>
            Password, 
            
            /// <summary>
            /// filter will be applied, so that only the decrypted username will be set
            /// </summary>
            Username
        }
    }
}