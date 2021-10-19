using System;
using System.Collections.Generic;

namespace Arc4u.KubeMQ
{
    public class HandlerContext
    {
        public Guid ActivityId { get; set; }

        public Dictionary<string, string> Tags { get; set; }
    }
}
