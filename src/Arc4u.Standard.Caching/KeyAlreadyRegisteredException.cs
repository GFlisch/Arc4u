using System;

namespace Arc4u.Caching
{
    public class KeyAlreadyRegisteredException : Exception
    {
        public KeyAlreadyRegisteredException(string key) : base(key) { }
    }
}
