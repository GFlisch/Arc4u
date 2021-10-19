﻿using System;
using System.Collections.Generic;
using System.Composition.Hosting.Core;

namespace Arc4u.ComponentModel.Composition
{
    // This one-instance-per-provider design is not efficient for more than a few instances;
    // we're just aiming to show the mechanics here.
    class InstanceExportDescriptorProvider : SinglePartExportDescriptorProvider
    {
        object _exportedInstance;

        public InstanceExportDescriptorProvider(object exportedInstance, Type contractType, string contractName, IDictionary<string, object> metadata)
            : base(contractType, contractName, metadata)
        {
            if (exportedInstance == null) throw new ArgumentNullException("exportedInstance");
            _exportedInstance = exportedInstance;
        }

        public override IEnumerable<ExportDescriptorPromise> GetExportDescriptors(CompositionContract contract, DependencyAccessor descriptorAccessor)
        {
            if (IsSupportedContract(contract))
                yield return new ExportDescriptorPromise(contract, _exportedInstance.ToString(), true, NoDependencies, _ =>
                    ExportDescriptor.Create((c, o) => _exportedInstance, Metadata));
        }
    }
}
