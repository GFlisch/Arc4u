using Microsoft.Extensions.Logging;
using System;
using System.Runtime.Serialization;

namespace Arc4u.Diagnostics
{

    [DataContract(Namespace = "http://arc4u.net/2010/11/tracemessage")]
    public sealed class TraceMessage : MessageBase
    {
        /// <summary>
        /// For serialization purpose.
        /// </summary>
        public TraceMessage() { }

        public TraceMessage(MessageBase message)
        {
            Category = message.Category;
            Code = message.Code;
            Subject = message.Subject;
            base.Type = message.Type;
            Text = message.Text;
            Source = message.Source.Clone() as MessageSource;
        }

        [DataMember(EmitDefaultValue = false, Order = 0)]
        public new MessageCategory Category
        {
            get { return base.Category; }
            set { base.Category = value; }
        }

        [DataMember(EmitDefaultValue = false, Order = 1)]
        public new String Code
        {
            get { return base.Code; }
            set { base.Code = value; }
        }

        [DataMember(EmitDefaultValue = false, Order = 2)]
        public new String Text
        {
            get { return base.Text; }
            set { base.Text = value; }
        }

        [DataMember(EmitDefaultValue = false, Order = 3)]
        public new String Subject
        {
            get { return base.Subject; }
            set { base.Subject = value; }
        }

        [DataMember(EmitDefaultValue = false, Order = 4)]
        public new LogLevel Type
        {
            get
            {
                var logLevel = LogLevel.Information;
                // Convert from Fatal to Critical!
                // Before we had a dedicated LogLevel => and Fatal was used instead of Critical.
                Enum.TryParse(base.Type == "Fatal" ? "Critial" : base.Type, true, out logLevel);

                return logLevel;
            }
            set
            {
                var type = Enum.GetName(typeof(LogLevel), value);
                base.Type = type == "Critial" ? "Fatal" : type;
            }
        }



        [DataMember(EmitDefaultValue = false, Order = 5)]
        public String ActivityId { get; set; }


        [DataMember(EmitDefaultValue = false, Order = 6)]
        public new MessageSource Source
        {
            get { return base.Source; }
            set { base.Source = value; }
        }


        private byte[] _rawData;

        [DataMember(EmitDefaultValue = false, Order = 7)]
        public byte[] RawData
        {
            get
            {
                return _rawData;
            }
            set
            {
                _rawData = value;
            }
        }


        public override string ToString()
        {
            return string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8} - {9} - {10}{11}",
                                 Source.Date.ToString("dd/MM/yyyy HH:mm:ss,fff").PadRight(24),
                                 Enum.GetName(typeof(LogLevel), Type).Trim().PadRight(12),
                                 Category.ToString().PadRight(14),
                                 Source.IdentityName.PadRight(15),
                                 Source.ProcessId.ToString().PadRight(6),
                                 Source.ThreadId.ToString().PadRight(6),
                                 Source.EventId.ToString().PadRight(4),
                                 String.IsNullOrWhiteSpace(ActivityId)
                                     ? string.Empty.PadRight(40)
                                     : ActivityId.PadRight(40),
                                 Source.TypeName,
                                 Source.MethodName,
                                 FullText,
                                 Environment.NewLine);
        }



    }
}
