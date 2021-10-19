using System;
using System.Collections.Generic;

namespace Arc4u.Dependency.Configuration
{
    public class AssemblyConfig
    {
        public String Assembly { get; set; }

        public ICollection<String> RejectedTypes { get; set; }
    }
}
