using System;

namespace Arc4u.Configuration;

public class ConfigurationException : Exception
{
    public ConfigurationException(string message) : base(message) { }
}
