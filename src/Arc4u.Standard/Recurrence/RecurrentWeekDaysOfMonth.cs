using System;
using System.Runtime.Serialization;

namespace Arc4u
{
    /// <summary>
    /// Represents <see cref="RecurrentWeekDays"/> positioned in a month.
    /// </summary>
    [DataContract]
    public struct RecurrentWeekDaysOfMonth :
         IEquatable<RecurrentWeekDaysOfMonth>
    {
        private WeekOfMonthPosition _position;

        /// <summary>
        /// Gets the <see cref="WeekOfMonthPosition"/> of the <see cref="WeekDays"/>.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public WeekOfMonthPosition Position
        {
            get { return _position; }
            internal set { _position = value; }
        }

        private RecurrentWeekDays _weekDays;

        /// <summary>
        /// Gets the recurrent week days (ex: every <see cref="RecurrentWeekDays.WeekEnd"/>).
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public RecurrentWeekDays WeekDays
        {
            get { return _weekDays; }
            internal set { _weekDays = value; }
        }

        private bool _anyWeekDays;

        /// <summary>
        /// Gets a value indicating whether any <see cref="WeekDays"/> is considered when determining the next occurence; otherwise, all the <see cref="WeekDays"/> are considered.
        /// </summary>
        /// <remarks>
        /// When combining any <see cref="WeekDays"/> in the <see cref="WeekOfMonthPosition.Last"/> position, 
        /// the latest date and time matching the specified <see cref="WeekDays"/> is considered.
        /// </remarks>
        [DataMember(EmitDefaultValue = false)]
        public bool AnyWeekDays
        {
            get { return _anyWeekDays; }
            internal set { _anyWeekDays = value; }
        }

        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{0} {1} in the {2} week of the month"
                , AnyWeekDays ? "Any" : "All"
                , WeekDays
                , Position);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            int hash = 0x4043ed48;
            hash = (hash * -1521134295) + Position.GetHashCode();
            hash = (hash * -1521134295) + WeekDays.GetHashCode();
            hash = (hash * -1521134295) + AnyWeekDays.GetHashCode();
            return hash;

        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return (obj is RecurrentWeekDaysOfMonth)
                ? Equals((RecurrentWeekDaysOfMonth)obj)
                : false;
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified <see cref="RecurrentWeekDaysOfMonth"/>.
        /// </summary>
        /// <param name="other">A <see cref="RecurrentWeekDaysOfMonth"/> to compare to this instance.</param>
        /// <returns>
        ///     <b>true</b> if this instance equals the <paramref name="other"/>; otherwise, <b>false</b>.
        /// </returns>
        public bool Equals(RecurrentWeekDaysOfMonth other)
        {
            return object.Equals(Position, other.Position)
                && object.Equals(WeekDays, other.WeekDays)
                && object.Equals(AnyWeekDays, other.AnyWeekDays);
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>A copy of this instance.</returns>
        public object Clone()
        {
            return new RecurrentWeekDaysOfMonth(Position, WeekDays, AnyWeekDays);
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RecurrentWeekDaysOfMonth"/> struct 
        /// where all <paramref name="weekDays"/> are considered when determining the next occurence.
        /// </summary>
        /// <param name="position">The position of the <paramref name="weekDays"/> in month.</param>
        /// <param name="weekDays">The recurrent week days (ex: every <see cref="RecurrentWeekDays.WeekEnd"/>).</param>
        public RecurrentWeekDaysOfMonth(WeekOfMonthPosition position, RecurrentWeekDays weekDays)
            : this(position, weekDays, false)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="RecurrentWeekDaysOfMonth"/> struct.
        /// </summary>
        /// <param name="position">The position of the <paramref name="weekDays"/> in month.</param>
        /// <param name="weekDays">The recurrent week days (ex: every <see cref="RecurrentWeekDays.WeekEnd"/>).</param>
        /// <param name="anyWeekDays">If set to <c>true</c> any <paramref name="weekDays"/> is considered when determining the next occurence; otherwise, all the <paramref name="weekDays"/> are considered.</param>
        /// <remarks>
        /// When combining any <paramref name="weekDays"/> in the <see cref="WeekOfMonthPosition.Last"/> position, 
        /// the latest date and time matching the specified <see cref="_weekDays"/> is considered.
        /// </remarks>
        public RecurrentWeekDaysOfMonth(WeekOfMonthPosition position, RecurrentWeekDays weekDays, bool anyWeekDays)
        {
            _position = position;
            _weekDays = weekDays;
            _anyWeekDays = anyWeekDays;
        }

        #endregion

        #region Operators

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left <see cref="RecurrentWeekDaysOfMonth"/>.</param>
        /// <param name="right">The right <see cref="RecurrentWeekDaysOfMonth"/>.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(RecurrentWeekDaysOfMonth left, RecurrentWeekDaysOfMonth right)
        {
            return object.Equals(left, right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left <see cref="RecurrentWeekDaysOfMonth"/>.</param>
        /// <param name="right">The right <see cref="RecurrentWeekDaysOfMonth"/>.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(RecurrentWeekDaysOfMonth left, RecurrentWeekDaysOfMonth right)
        {
            return !object.Equals(left, right);
        }

        #endregion
    }
}