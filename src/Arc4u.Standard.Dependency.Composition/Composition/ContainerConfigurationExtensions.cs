using System;
using System.Collections.Generic;
using System.Composition.Hosting;

namespace Arc4u.ComponentModel.Composition
{
    public static class ContainerConfigurationExtensions
    {
        public static ContainerConfiguration RegisterInstance<T>(this ContainerConfiguration configuration, T exportedInstance, string contractName = null, IDictionary<string, object> metadata = null)
        {
            return RegisterInstance(configuration, exportedInstance, typeof(T), contractName, metadata);
        }

        public static ContainerConfiguration RegisterInstance(this ContainerConfiguration configuration, object exportedInstance, Type contractType, string contractName = null, IDictionary<string, object> metadata = null)
        {
            return configuration.WithProvider(new InstanceExportDescriptorProvider(
                exportedInstance, contractType, contractName, metadata));
        }

    }
}
