using System;

namespace Arc4u.Security.Cryptography
{
    [AttributeUsage(AttributeTargets.Property)]
    public class EncryptedAttribute : Attribute
    {
        public Filter FilterValue { get; }

        public EncryptedAttribute(Filter filter = Filter.None)
        {
            FilterValue = filter;
        }

        public enum Filter
        {
            None, Password, Username
        }
    }
}