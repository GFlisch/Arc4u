using System;
using System.Runtime.Serialization;
using System.Text;

namespace Arc4u.Diagnostics
{
    [DataContract]
    public abstract class MessageBase
    {
        protected internal MessageCategory Category;
        protected internal String Code;
        protected internal String Subject;
        protected internal String Text;
        protected internal String Type;
        protected internal MessageSource Source;

        public String FullText
        {
            get
            {
                var text = new StringBuilder();
                var separator = String.Empty;

                if (!String.IsNullOrEmpty(Code))
                    text.Append(Code + ": ");

                if (!String.IsNullOrEmpty(Subject))
                {
                    text.Append(Subject);
                    separator = ", ";
                }

                if (!String.IsNullOrEmpty(Text))
                    text.Append($"{separator}{Text}");

                return text.ToString();
            }
        }

    }
}
