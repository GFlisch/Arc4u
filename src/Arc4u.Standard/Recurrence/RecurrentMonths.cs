using System;
using System.Runtime.Serialization;

namespace Arc4u
{
    /// <summary>
    /// Specifies the recurrent months.
    /// </summary>
    [Flags]
    [DataContract]
    public enum RecurrentMonths : short
    {
        /// <summary>
        /// Occurs every January.
        /// </summary>
        [EnumMember]
        January = 1,
        /// <summary>
        /// Occurs every February.
        /// </summary>
        [EnumMember]
        February = 2,
        /// <summary>
        /// Occurs every March.
        /// </summary>
        [EnumMember]
        March = 4,
        /// <summary>
        /// Occurs every April.
        /// </summary>
        [EnumMember]
        April = 8,
        /// <summary>
        /// Occurs every May.
        /// </summary>
        [EnumMember]
        May = 16,
        /// <summary>
        /// Occurs every June.
        /// </summary>
        [EnumMember]
        June = 32,
        /// <summary>
        /// Occurs every July.
        /// </summary>
        [EnumMember]
        July = 64,
        /// <summary>
        /// Occurs every August.
        /// </summary>
        [EnumMember]
        August = 128,
        /// <summary>
        /// Occurs every September.
        /// </summary>
        [EnumMember]
        September = 256,
        /// <summary>
        /// Occurs every October.
        /// </summary>
        [EnumMember]
        October = 512,
        /// <summary>
        /// Occurs every November.
        /// </summary>
        [EnumMember]
        November = 1024,
        /// <summary>
        /// Occurs every December.
        /// </summary>
        [EnumMember]
        December = 2048,
        /// Occurs every January, February, March, April, May, June, July, Augustus, September, October, November and December.
        [EnumMember]
        EveryMonth = 4095,
    }
}
