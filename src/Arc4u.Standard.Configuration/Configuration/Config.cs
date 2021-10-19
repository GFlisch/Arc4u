using Microsoft.Extensions.Configuration;

namespace Arc4u.Configuration
{
    public class Config
    {
        public Config()
        {
            Environment = new Environment();
        }

        public Config(IConfiguration configuration)
        {
            configuration.Bind("Application.Configuration", this);
        }

        /// <summary>
        /// Name used to dentify the application when used externally other than logging!
        /// Cache or authorization, etc...
        /// </summary>
        public string ApplicationName { get; set; }

        public Environment Environment { get; set; }
    }
}
