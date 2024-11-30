using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;

namespace Arc4u.Extensions;

public static class EnumExtentions
{
    public static string GetDisplayName(this Enum value)
    {
        var type = value.GetType();
        var ti = type.GetTypeInfo();
        if (!ti.IsEnum)
        {
            throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Type '{0}' is not Enum", type));
        }

        var member = type.GetRuntimeField(value.ToString());

        if (null == member)
        {
            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "'{0}' is not a member of the enum '{1}'", value, type.Name));
        }

        var attributes = member.GetCustomAttributes(typeof(DisplayAttribute), false);
        if (attributes.Length == 0)
        {
            throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "'{0}.{1}' doesn't have DisplayAttribute", type.Name, value));
        }

        var attribute = (DisplayAttribute)attributes.First();

        if (null == attribute)
        {
            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "'{0}' doesn't have DisplayAttribute", value));
        }

        return attribute.GetName() ?? string.Empty;
    }

    public static string GetValue(this Enum value)
    {
        return value.ToString();
    }

    public static List<KeyValuePair<string, string>> ToTranslationList(this Type enumType)
    {
        var ti = enumType.GetTypeInfo();

        if (!ti.IsEnum)
        {
            throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Type '{0}' is not Enum", enumType));
        }

        var t = Enum.GetNames(enumType)
                    .Select(enumName => Enum.Parse(enumType, enumName) as Enum)
                    .Where(e => e != null)
                    .ToDictionary(e => e!.ToString(), e => e!.GetDisplayName());

        return t.Select(v => new KeyValuePair<string, string>(v.Key, v.Value)).ToList();
    }

    public static Dictionary<Enum, string> ToTranslationDictionary(this Type enumType)
    {
        var ti = enumType.GetTypeInfo();

        if (!ti.IsEnum)
        {
            throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Type '{0}' is not Enum", enumType));
        }

        return Enum.GetNames(enumType)
                   .Select(enumName => Enum.Parse(enumType, enumName) as Enum)
                   .Where(e => e != null)
                   .ToDictionary(e => e!, e => e!.GetDisplayName());
    }
}
