using System.Collections;
using System.Globalization;
using System.Reflection;

namespace Arc4u.Utils;

public readonly struct Enum<T>(T value) : IEnumerable<T>
                                            where T : struct, IComparable, IFormattable, IConvertible
{
    private readonly T _value = value;

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
    public override readonly string ToString()
    {
        return _value.ToString() ?? string.Empty;
    }

    public override readonly bool Equals(object? obj)
    {
        return _value.Equals(obj);
    }

    public override readonly int GetHashCode()
    {
        return _value.GetHashCode();
    }
    #endregion

    #region IEnumerable Members

    readonly IEnumerator IEnumerable.GetEnumerator()
    {
        return (this as IEnumerable<T>).GetEnumerator();
    }

    #endregion

    #region IEnumerable<T> Members

    readonly IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        foreach (var item in EnumUtil.GetValues(typeof(T), true))
        {
            switch (Convert.GetTypeCode(_value))
            {
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    if ((Convert.ToInt64(_value, CultureInfo.InvariantCulture) & Convert.ToInt64(item, CultureInfo.InvariantCulture)) == Convert.ToInt64(item, CultureInfo.InvariantCulture))
                    {
                        yield return (T)Enum.ToObject(typeof(T), item);
                    }

                    break;
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    if ((Convert.ToUInt64(_value, CultureInfo.InvariantCulture) & Convert.ToUInt64(item, CultureInfo.InvariantCulture)) != Convert.ToUInt64(item, CultureInfo.InvariantCulture))
                    {
                        yield return (T)Enum.ToObject(typeof(T), item);
                    }

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
        return EnumUtil.GetNames<T>(string.Empty, false);
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

    public static bool operator ==(Enum<T> left, Enum<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Enum<T> left, Enum<T> right)
    {
        return !(left == right);
    }
    #endregion
}

public struct EnumUtil
{
    private const string Arg_EnumUnderlyingTypeAndObjectMustBeSameType = "Enum underlying type and the object must be same type or object must be a string. Type passed in was '{0}'; the enum underlying type was '{1}'.";
    private const string Arg_EnumAndObjectMustBeSameType = "Object must be the same type as the enum. The type passed in was '{0}'; the enum type was '{1}'.";
    private const string Arg_MustBeValueTypeOrstring = "Object must be a ValueType or a string.";
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
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(enumType);
#else
        if (enumType == null)
        {
            throw new ArgumentNullException(nameof(enumType));
        }
#endif
        if (!enumType.GetTypeInfo().IsEnum)
        {
            throw new ArgumentException(Arg_MustBeEnum, nameof(enumType));
        }

        //get enum values
        var enumValues = Enum.GetValues(enumType);
        ulong maxValue = 0;

        //add single values
        var values = new List<Enum>(enumValues.Length);
        foreach (Enum value in enumValues)
        {
            values.Add(value);
            if (ulong.TryParse(value.ToString("D"), out var result) && result > maxValue)
            {
                maxValue = result;
            }
        }

        //add flagged values
        if (!ignoreFlags && enumType.GetTypeInfo().GetCustomAttributes(typeof(FlagsAttribute), true).Length != 0)
        {
            for (ulong i = 0; i < maxValue * 2; i++)
            {
                var value = (Enum)Enum.ToObject(enumType, i);
                if (value.ToString("G") != value.ToString("D") && values.IndexOf(value) == -1)
                {
                    values.Add(value);
                }
            }
        }

        //sort values
        for (var j = 1; j < values.Count; j++)
        {
            var index = j;
            var value = values[j];
            var flag = false;
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
        foreach (var value in GetValues(typeof(T), ignoreFlags))
        {
            yield return (T)Enum.ToObject(typeof(T), value);
        }
    }

    public static IEnumerable<string> GetNames(Type enumType)
    {
        return GetNames(enumType, string.Empty, false);
    }

    public static IEnumerable<string> GetNames(Type enumType, string format)
    {
        return GetNames(enumType, format, false);
    }

    public static IEnumerable<string> GetNames(Type enumType, string format, bool ignoreFlags)
    {
        foreach (var value in GetValues(enumType, ignoreFlags))
        {
            yield return value.ToString(format);
        }
    }

    public static IEnumerable<string> GetNames<T>(string format)
        where T : struct, IComparable, IFormattable, IConvertible
    {
        return GetNames<T>(format, false);
    }

    public static IEnumerable<string> GetNames<T>(string format, bool ignoreFlags)
        where T : struct, IComparable, IFormattable, IConvertible
    {
        return GetNames<T>(format, ignoreFlags);
    }

    public static bool IsDefined<T>(object value)
        where T : struct, IComparable, IFormattable, IConvertible
    {
        return TryParse(value, false, out T? _);
    }

    public static bool IsDefined<T>(object value, bool ignoreCase)
        where T : struct, IComparable, IFormattable, IConvertible
    {
        return TryParse(value, ignoreCase, out T? _);
    }

    public static T Parse<T>(object value)
        where T : struct, IComparable, IFormattable, IConvertible
    {
        return Parse<T>(value, false);
    }

    public static T Parse<T>(object value, bool ignoreCase)
        where T : struct, IComparable, IFormattable, IConvertible
    {
        var e = TryParseEnum(value, ignoreCase, out T? result);

        if (e == null)
        {
            return result!.Value;
        }
        else
        {
            throw e;
        }
    }

    public static T? TryParse<T>(object value)
        where T : struct, IComparable, IFormattable, IConvertible
    {
        return TryParse<T>(value, false);
    }

    public static T? TryParse<T>(object value, bool ignoreCase)
        where T : struct, IComparable, IFormattable, IConvertible
    {
        TryParse(value, ignoreCase, out T? result);
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

    private static Exception? TryParseEnum<T>(object value, bool ignoreCase, out T? result)
        where T : struct, IComparable, IFormattable, IConvertible
    {
        result = null;

        if (value == null)
        {
            return new ArgumentNullException(nameof(value));
        }

        var type = value.GetType();
        if ((type != typeof(string)) && !type.GetTypeInfo().IsSubclassOf(typeof(ValueType)))
        {
            return new ArgumentException(Arg_MustBeValueTypeOrstring, nameof(value));
        }

        var underlyingType = Enum.GetUnderlyingType(typeof(T));
        if (type.GetTypeInfo().IsEnum)
        {
            if (type != typeof(T))
            {
                return new ArgumentException(string.Format(CultureInfo.InvariantCulture, Arg_EnumAndObjectMustBeSameType, type.ToString(), typeof(T).ToString()));
            }
            type = Enum.GetUnderlyingType(type);
        }
        else if (type != underlyingType && !(type == typeof(string)))
        {
            return new ArgumentException(string.Format(CultureInfo.InvariantCulture, Arg_EnumUnderlyingTypeAndObjectMustBeSameType, type.ToString(), underlyingType.ToString()));
        }

        if (type == typeof(string))
        {
            var name = (string)value;
            name = name.Trim();
            if (name.Length == 0)
            {
                return new ArgumentException(Arg_MustContainEnumInfo, nameof(value));
            }

            foreach (var val in GetValues(typeof(T), false))
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
                    var lng = Convert.ToInt64(value, CultureInfo.InvariantCulture);
                    foreach (var val in GetValues(typeof(T), false))
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
                    var ulng = Convert.ToUInt64(value, CultureInfo.InvariantCulture);
                    foreach (var val in GetValues(typeof(T), false))
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

        return new ArgumentException(string.Format(CultureInfo.InvariantCulture, Arg_EnumValueNotFound, value), nameof(value));
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
                foreach (var t in value)
                {
                    val |= Convert.ToInt64(t, CultureInfo.InvariantCulture);
                }
                return (T)Enum.ToObject(typeof(T), val);
            case TypeCode.Byte:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
                ulong uval = 0;
                foreach (var t in value)
                {
                    uval |= Convert.ToUInt64(t, CultureInfo.InvariantCulture);
                }
                return (T)Enum.ToObject(typeof(T), uval);
            default:
                throw new InvalidOperationException(InvalidOperation_UnknownEnumType);
        }
    }
}
