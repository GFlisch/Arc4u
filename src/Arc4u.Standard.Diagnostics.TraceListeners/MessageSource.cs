using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security.Principal;
using System.Threading;
using System.Xml.Serialization;

namespace Arc4u.Diagnostics
{
    [Obsolete("Use Serilog")]
    [DataContract(Namespace = "http://arc4u.net/2010/11/messagesource")]
    [Serializable, XmlRoot("MessageBase", Namespace = "http://arc4u.net/2010/11/messagesource", IsNullable = true)]
    public class MessageSource : ICloneable
    {
        private static readonly string _machineName;
        private static readonly int _processId;
        private static readonly string _from;

        static MessageSource()
        {
            _machineName = Environment.MachineName;
            _processId = Process.GetCurrentProcess().Id;
            var winIdentity = WindowsIdentity.GetCurrent();
            if (null != winIdentity)
                _from = winIdentity.Name;
        }

        public MessageSource(String typeName, string methodName, String application, string identityName, int eventId)
        {
            TypeName = typeName;
            MethodName = methodName;
            Date = DateTime.Now;
            MachineName = _machineName;
            ProcessId = _processId;
            IdentityName = identityName;
            From = _from;
            ThreadId = Thread.CurrentThread.ManagedThreadId;
            EventId = eventId;
            Application = application;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("DateTimeOffset = {0}, MachineName = {1}, ProcessId = {2}, ThreadId = {3}, IdentityName = {4}, Name = {5}, TypeName = {6}, MethodName = {7}"
                , Date
                , MachineName
                , ProcessId
                , ThreadId
                , IdentityName
                , Name
                , TypeName
                , MethodName);
        }


        /// <summary>
        /// For xmlSerialization purpose.
        /// </summary>
        public MessageSource() { }

        [XmlElement(Order = 0)]
        [DataMember(EmitDefaultValue = false, Order = 0)]
        public String Stacktrace { get; set; }

        [XmlElement(Order = 1)]
        [DataMember(EmitDefaultValue = false, Order = 1)]
        public DateTime Date { get; set; }

        [XmlElement(Order = 2)]
        [DataMember(EmitDefaultValue = false, Order = 2)]
        public String Application { get; set; }

        [XmlElement(Order = 3)]
        [DataMember(EmitDefaultValue = false, Order = 3)]
        public int ThreadId { get; set; }

        [XmlElement(Order = 4)]
        [DataMember(EmitDefaultValue = false, Order = 4)]
        public int ProcessId { get; set; }

        [XmlElement(Order = 5)]
        [DataMember(EmitDefaultValue = false, Order = 5)]
        public String IdentityName { get; set; }

        [XmlElement(Order = 6)]
        [DataMember(EmitDefaultValue = false, Order = 6)]
        public String MachineName { get; set; }

        [XmlElement(Order = 7)]
        [DataMember(EmitDefaultValue = false, Order = 7)]
        public int EventId { get; set; }

        [XmlElement(Order = 8)]
        [DataMember(EmitDefaultValue = false, Order = 8)]
        public String MethodName { get; set; }

        [XmlElement(Order = 9)]
        [DataMember(EmitDefaultValue = false, Order = 9)]
        public String TypeName { get; set; }

        [XmlElement(Order = 10)]
        [DataMember(EmitDefaultValue = false, Order = 10)]
        public String Name { get; set; } // Should check for Diagnostic integration!

        [XmlElement(Order = 11)]
        [DataMember(EmitDefaultValue = false, Order = 11)]
        public String Assembly { get; set; }

        /// <summary>
        /// Set the service who send the message.
        /// </summary>
        [XmlElement(Order = 12)]
        [DataMember(EmitDefaultValue = false, Order = 12)]
        public String From { get; set; }




        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
