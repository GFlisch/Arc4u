using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Arc4u.Utils
{
    public struct Enum<T> : IEnumerable<T>, IEnumerable
        where T : struct, IComparable, IFormattable, IConvertible
    {
        private readonly T _value;
        public Enum(T value)
        {
            _value = value;
        }

        #region Operator Members
        public static implicit operator T(Enum<T> e)
        {
            return e._value;
        }

        public static implicit operator Enum<T>(T e)
        {
            return new Enum<T>(e);
        }
        #endregion

        #region Overriden Members
        public override string ToString()
        {
            return _value.ToString();
        }

        public override bool Equals(object obj)
        {
            return _value.Equals(obj);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }
        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<T>).GetEnumerator();
        }

        #endregion

        #region IEnumerable<T> Members

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            foreach (Enum value in EnumUtil.GetValues(typeof(T), true))
            {
                switch (Convert.GetTypeCode(_value))
                {
                    case TypeCode.SByte:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                        if ((Convert.ToInt64(_value, CultureInfo.InvariantCulture) & Convert.ToInt64(value, CultureInfo.InvariantCulture)) == Convert.ToInt64(value, CultureInfo.InvariantCulture))
                            yield return (T)Enum.ToObject(typeof(T), value);
                        break;
                    case TypeCode.Byte:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                        if ((Convert.ToUInt64(_value, CultureInfo.InvariantCulture) & Convert.ToUInt64(value, CultureInfo.InvariantCulture)) != Convert.ToUInt64(value, CultureInfo.InvariantCulture))
                            yield return (T)Enum.ToObject(typeof(T), value);
                        break;
                }
            }
        }

        #endregion

        #region Static Members
        public static Enum<T> FromEnumerable(IEnumerable<T> value)
        {
            return EnumUtil.FromEnumerable(value);
        }

        public static IEnumerable<T> GetValues()
        {
            return EnumUtil.GetValues<T>(false);
        }

        public static IEnumerable<T> GetValues(bool ignoreFlags)
        {
            return EnumUtil.GetValues<T>(ignoreFlags);
        }

        public static IEnumerable<string> GetNames()
        {
            return EnumUtil.GetNames<T>(null, false);
        }

        public static IEnumerable<string> GetNames(string format)
        {
            return EnumUtil.GetNames<T>(format, false);
        }

        public static IEnumerable<string> GetNames(string format, bool ignoreFlags)
        {
            return EnumUtil.GetNames<T>(format, ignoreFlags);
        }

        public static bool IsDefined(object value)
        {
            return EnumUtil.IsDefined<T>(value, false);
        }

        public static bool IsDefined(object value, bool ignoreCase)
        {
            return EnumUtil.IsDefined<T>(value, ignoreCase);
        }

        public static object Parse(object value)
        {
            return EnumUtil.Parse<T>(value, false);
        }

        public static T Parse(object value, bool ignoreCase)
        {
            return EnumUtil.Parse<T>(value, ignoreCase);
        }

        public static T? TryParse(object value)
        {
            return EnumUtil.TryParse<T>(value, false);
        }

        public static T? TryParse(object value, bool ignoreCase)
        {
            return EnumUtil.TryParse<T>(value, ignoreCase);
        }

        public static bool TryParse(object value, out T? result)
        {
            return EnumUtil.TryParse(value, false, out result);
        }

        public static bool TryParse(object value, bool ignoreCase, out T? result)
        {
            return EnumUtil.TryParse(value, ignoreCase, out result);
        }
        #endregion
    }

    public struct EnumUtil
    {
        private const string Arg_EnumUnderlyingTypeAndObjectMustBeSameType = "Enum underlying type and the object must be same type or object must be a String. Type passed in was '{0}'; the enum underlying type was '{1}'.";
        private const string Arg_EnumAndObjectMustBeSameType = "Object must be the same type as the enum. The type passed in was '{0}'; the enum type was '{1}'.";
        private const string Arg_MustBeValueTypeOrString = "Object must be a ValueType or a String.";
        private const string Arg_MustContainEnumInfo = "Must specify valid information for parsing in the string.";
        private const string Arg_EnumValueNotFound = "Requested value '{0}' was not found.";
        private const string InvalidOperation_UnknownEnumType = "Unknown enum type.";
        private const string Arg_MustBeEnum = "Type provided must be an Enum.";

        public static IEnumerable<Enum> GetValues(Type enumType)
        {
            return GetValues(enumType, false);
        }

        public static IEnumerable<Enum> GetValues(Type enumType, bool ignoreFlags)
        {
            if (enumType == null)
            {
                throw new ArgumentNullException("enumType");
            }
            if (!enumType.GetTypeInfo().IsEnum)
            {
                throw new ArgumentException(Arg_MustBeEnum, "enumType");
            }

            //get enum values
            Array enumValues = Enum.GetValues(enumType);
            ulong maxValue = 0;

            //add single values
            IList<Enum> values = new List<Enum>(enumValues.Length);
            foreach (Enum value in enumValues)
            {
                values.Add(value);
                ulong result;
                if (ulong.TryParse(value.ToString("D"), out result) && result > maxValue)
                {
                    maxValue = result;
                }
            }

            //add flagged values
            if (!ignoreFlags && enumType.GetTypeInfo().GetCustomAttributes(typeof(FlagsAttribute), true).Count() != 0)
            {
                for (ulong i = 0; i < maxValue * 2; i++)
                {
                    Enum value = (Enum)Enum.ToObject(enumType, i);
                    if (value.ToString("G") != value.ToString("D") && values.IndexOf(value) == -1)
                    {
                        values.Add(value);
                    }
                }
            }

            //sort values
            for (int j = 1; j < values.Count; j++)
            {
                int index = j;
                Enum value = values[j];
                bool flag = false;
                while (values[index - 1].CompareTo(value) > 0)
                {
                    values[index] = values[index - 1];
                    index--;
                    flag = true;
                    if (index == 0)
                    {
                        break;
                    }
                }
                if (flag)
                {
                    values[index] = value;
                }
            }

            return values;
        }

        public static IEnumerable<T> GetValues<T>()
            where T : struct, IComparable, IFormattable, IConvertible
        {
            return GetValues<T>(false);
        }

        public static IEnumerable<T> GetValues<T>(bool ignoreFlags)
            where T : struct, IComparable, IFormattable, IConvertible
        {
            foreach (Enum value in GetValues(typeof(T), ignoreFlags))
            {
                yield return (T)Enum.ToObject(typeof(T), value);
            }
        }

        public static IEnumerable<string> GetNames(Type enumType)
        {
            return GetNames(enumType, null, false);
        }

        public static IEnumerable<string> GetNames(Type enumType, string format)
        {
            return GetNames(enumType, format, false);
        }

        public static IEnumerable<string> GetNames(Type enumType, string format, bool ignoreFlags)
        {
            foreach (Enum value in GetValues(enumType, ignoreFlags))
            {
                yield return value.ToString(format);
            }
        }

        public static IEnumerable<string> GetNames<T>(string format)
            where T : struct, IComparable, IFormattable, IConvertible
        {
            return GetNames(typeof(T), format, false);
        }

        public static IEnumerable<string> GetNames<T>(string format, bool ignoreFlags)
            where T : struct, IComparable, IFormattable, IConvertible
        {
            return GetNames(typeof(T), format, ignoreFlags);
        }

        public static bool IsDefined<T>(object value)
            where T : struct, IComparable, IFormattable, IConvertible
        {
            T? result;
            return TryParse(value, false, out result);
        }

        public static bool IsDefined<T>(object value, bool ignoreCase)
            where T : struct, IComparable, IFormattable, IConvertible
        {
            T? result;
            return TryParse(value, ignoreCase, out result);
        }

        public static T Parse<T>(object value)
            where T : struct, IComparable, IFormattable, IConvertible
        {
            return Parse<T>(value, false);
        }

        public static T Parse<T>(object value, bool ignoreCase)
            where T : struct, IComparable, IFormattable, IConvertible
        {
            T? result;
            Exception e = TryParseEnum(value, ignoreCase, out result);

            if (e == null)
            {
                return result.Value;
            }

            throw e;
        }

        public static T? TryParse<T>(object value)
            where T : struct, IComparable, IFormattable, IConvertible
        {
            return TryParse<T>(value, false);
        }

        public static T? TryParse<T>(object value, bool ignoreCase)
            where T : struct, IComparable, IFormattable, IConvertible
        {
            T? result;
            TryParse(value, ignoreCase, out result);
            return result;
        }

        public static bool TryParse<T>(object value, out T? result)
            where T : struct, IComparable, IFormattable, IConvertible
        {
            return TryParse(value, false, out result);
        }

        public static bool TryParse<T>(object value, bool ignoreCase, out T? result)
            where T : struct, IComparable, IFormattable, IConvertible
        {
            return (TryParseEnum(value, ignoreCase, out result) == null);
        }

        private static Exception TryParseEnum<T>(object value, bool ignoreCase, out T? result)
            where T : struct, IComparable, IFormattable, IConvertible
        {
            result = null;

            if (value == null)
            {
                return new ArgumentNullException("value");
            }

            Type type = value.GetType();
            if (!(type == typeof(string)) && !type.GetTypeInfo().IsSubclassOf(typeof(ValueType)))
            {
                return new ArgumentException(Arg_MustBeValueTypeOrString, "value");
            }

            Type underlyingType = Enum.GetUnderlyingType(typeof(T));
            if (type.GetTypeInfo().IsEnum)
            {
                if (type != typeof(T))
                {
                    return new ArgumentException(string.Format(CultureInfo.CurrentCulture, Arg_EnumAndObjectMustBeSameType, new object[] { type.ToString(), typeof(T).ToString() }));
                }
                type = Enum.GetUnderlyingType(type);
            }
            else if (type != underlyingType && !(type == typeof(string)))
            {
                return new ArgumentException(string.Format(CultureInfo.CurrentCulture, Arg_EnumUnderlyingTypeAndObjectMustBeSameType, new object[] { type.ToString(), underlyingType.ToString() }));
            }

            if (type == typeof(string))
            {
                string name = (string)value;
                name = name.Trim();
                if (name.Length == 0)
                {
                    return new ArgumentException(Arg_MustContainEnumInfo, "value");
                }

                foreach (Enum val in GetValues(typeof(T), false))
                {
                    if (string.Compare(val.ToString(), name, ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture) == 0)
                    {
                        result = (T)Enum.ToObject(typeof(T), val);
                        return null;
                    }
                }
            }
            else
            {
                switch (Convert.GetTypeCode(value))
                {
                    case TypeCode.SByte:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                        long lng = Convert.ToInt64(value, CultureInfo.InvariantCulture);
                        foreach (Enum val in GetValues(typeof(T), false))
                        {
                            if (lng == Convert.ToInt64(val, CultureInfo.InvariantCulture))
                            {
                                result = (T)Enum.ToObject(typeof(T), val);
                                return null;
                            }
                        }
                        break;
                    case TypeCode.Byte:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                        ulong ulng = Convert.ToUInt64(value, CultureInfo.InvariantCulture);
                        foreach (Enum val in GetValues(typeof(T), false))
                        {
                            if (ulng == Convert.ToUInt64(val, CultureInfo.InvariantCulture))
                            {
                                result = (T)Enum.ToObject(typeof(T), val);
                                return null;
                            }
                        }
                        break;
                    default:
                        return new InvalidOperationException(InvalidOperation_UnknownEnumType);
                }
            }

            return new ArgumentException(string.Format(Arg_EnumValueNotFound, value), "value");
        }


        public static Enum<T> FromEnumerable<T>(IEnumerable<T> value)
            where T : struct, IComparable, IFormattable, IConvertible
        {
            switch (Convert.GetTypeCode(value))
            {
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    long val = 0;
                    foreach (T t in value)
                    {
                        val |= Convert.ToInt64(t, CultureInfo.InvariantCulture);
                    }
                    return (T)Enum.ToObject(typeof(T), val);
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    ulong uval = 0;
                    foreach (T t in value)
                    {
                        uval |= Convert.ToUInt64(t, CultureInfo.InvariantCulture);
                    }
                    return (T)Enum.ToObject(typeof(T), uval);
                default:
                    throw new InvalidOperationException(InvalidOperation_UnknownEnumType);
            }
        }
    }
}
