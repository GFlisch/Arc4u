using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Arc4u.Configuration
{
    public class ConfigurationHelper
    {
        /// <summary>
        /// Load json files for the configuration and returns a IConfiguration so we can extract the
        /// settings.
        /// More than one file is possible.
        /// </summary>
        /// <param name="configFiles"></param>
        /// <returns></returns>
        public static IConfiguration GetConfigurationFromFile(params string[] configFiles)
        {
            var configBuilder = new ConfigurationBuilder();
            foreach (var file in configFiles)
            {
                configBuilder.AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, file));
            }
            return configBuilder.Build();
        }

        public static IConfiguration GetConfigurationFromResourceStream(params string[] fileTypes)
        {
            var configBuilder = new ConfigurationBuilder();

            var disposables = new List<IDisposable>();
            var assemblies = new Dictionary<String, Assembly>();

            foreach (var file in fileTypes)
            {
                var fileInfo = file.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (fileInfo.Length != 2)
                {
                    throw new ApplicationException($"File info receives is badly formatted: '{file}'. We expect: 'Assembly, full namespace file'.");
                }
                var assemblyName = fileInfo[0].Trim();

                if (!assemblies.ContainsKey(assemblyName))
                {
                    var a = Assembly.Load(assemblyName);
                    if (null != a)
                        assemblies.Add(assemblyName, a);
                }

                var stream = assemblies[assemblyName].GetManifestResourceStream(fileInfo[1].Trim());
                if (null != stream)
                {
                    disposables.Add(stream);
                    configBuilder.AddJsonStream(stream);
                }
            }

            var configuration = configBuilder.Build();

            foreach (var disp in disposables)
                disp.Dispose();

            return configuration;
        }
    }
}
