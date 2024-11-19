using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Arc4u.Extensions
{
    public static class EnumExtentions
    {
        public static string GetDisplayName(this Enum value)
        {
            var type = value.GetType();
            TypeInfo ti = type.GetTypeInfo();
            if (!ti.IsEnum)
            {
                throw new ArgumentException(String.Format("Type '{0}' is not Enum", type));
            }

            var member = type.GetRuntimeField(value.ToString());

            var attributes = member.GetCustomAttributes(typeof(DisplayAttribute), false);
            if (attributes.Count() == 0)
            {
                throw new ArgumentException(String.Format("'{0}.{1}' doesn't have DisplayAttribute", type.Name, value));
            }

            var attribute = (DisplayAttribute)attributes.First();
            return attribute.GetName();
        }

        public static string GetValue(this Enum value)
        {
            return value.ToString();
        }

        public static List<KeyValuePair<String, String>> ToTranslationList(this Type enumType)
        {
            TypeInfo ti = enumType.GetTypeInfo();

            if (!ti.IsEnum)
            {
                throw new ArgumentException(String.Format("Type '{0}' is not Enum", enumType));
            }

            var t = Enum.GetNames(enumType).Select(enumName => Enum.Parse(enumType, enumName) as Enum)
                .Where(e => e != null).ToDictionary(e => e.ToString(), e => e.GetDisplayName());

            return t.Select(v => new KeyValuePair<String, String>(v.Key, v.Value)).ToList();
        }

        public static Dictionary<Enum, string> ToTranslationDictionary(this Type enumType)
        {
            TypeInfo ti = enumType.GetTypeInfo();

            if (!ti.IsEnum)
            {
                throw new ArgumentException(String.Format("Type '{0}' is not Enum", enumType));
            }

            return Enum.GetNames(enumType).Select(enumName => Enum.Parse(enumType, enumName) as Enum)
                .Where(e => e != null).ToDictionary(e => e, e => e.GetDisplayName());
        }
    }
}
