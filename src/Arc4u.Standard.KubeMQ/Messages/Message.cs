using System;
using System.Collections.Generic;

namespace Arc4u.KubeMQ
{
    public abstract class MessageObject
    {
        public MessageObject(Object message, Dictionary<string, string> tags = null)
        {
            _message = message;
            _tags = tags;
        }

        private readonly Object _message;
        private readonly Dictionary<string, string> _tags;

        public Object Value => _message;

        public Dictionary<string, string> Tags => _tags;


    }
}
