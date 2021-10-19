using System;

namespace Arc4u.KubeMQ.AspNetCore.Handler
{
    public class HandlerDefintion
    {
        public string Name { get; set; }

        public Type HandlerType { get; set; }

        public string Serializer { get; set; }
    }
}
