﻿using System;
using System.Collections.Generic;

namespace Arc4u.Dependency.ComponentModel
{
    public class NameResolver
    {
        public Dictionary<Tuple<String, Type>, List<Type>> NameResolution { get; private set; }
        public Dictionary<Tuple<String, Type>, List<object>> InstanceNameResolution { get; private set; }

        public NameResolver()
        {
            NameResolution = new Dictionary<Tuple<String, Type>, List<Type>>();
            InstanceNameResolution = new Dictionary<Tuple<string, Type>, List<object>>();
        }
    }
}
