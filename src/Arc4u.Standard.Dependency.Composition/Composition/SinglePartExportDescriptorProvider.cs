﻿using System;
using System.Collections.Generic;
using System.Composition.Hosting.Core;
using System.Linq;

namespace Arc4u.ComponentModel.Composition
{
    abstract class SinglePartExportDescriptorProvider : ExportDescriptorProvider
    {
        readonly Type _contractType;
        readonly string _contractName;
        readonly IDictionary<string, object> _metadata;

        protected SinglePartExportDescriptorProvider(Type contractType, string contractName, IDictionary<string, object> metadata)
        {
            if (contractType == null) throw new ArgumentNullException("contractType");

            _contractType = contractType;
            _contractName = contractName;
            _metadata = metadata ?? new Dictionary<string, object>();
        }

        protected bool IsSupportedContract(CompositionContract contract)
        {
            if (contract.ContractType != _contractType ||
                contract.ContractName != _contractName)
                return false;

            if (contract.MetadataConstraints != null)
            {
                var subsetOfConstraints = contract.MetadataConstraints.Where(c => _metadata.ContainsKey(c.Key)).ToDictionary(c => c.Key, c => _metadata[c.Key]);
                var constrainedSubset = new CompositionContract(contract.ContractType, contract.ContractName,
                    subsetOfConstraints.Count == 0 ? null : subsetOfConstraints);

                if (!contract.Equals(constrainedSubset))
                    return false;
            }

            return true;
        }

        protected IDictionary<string, object> Metadata { get { return _metadata; } }
    }
}
