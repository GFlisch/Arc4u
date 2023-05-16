using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Arc4u
{
    /// <summary>
    /// Provides extension methods for flags enumerations.
    /// </summary>
    public class FlagsEnum
    {
        /// <summary>
        /// Gets a value of <typeparamref name="TEnum"/> from two raised to the specified <paramref name="power"/>.
        /// </summary>
        /// <typeparam name="TEnum">An enumeration type.</typeparam>
        /// <param name="power">The raising power of two.</param>
        /// <returns>The matching value of <typeparamref name="TEnum"/>.</returns>
        /// <exception cref="ArgumentException">
        /// <typeparamref name="TEnum"/> is not an enumeration type -or-
        /// no match found in the defined values of <typeparamref name="TEnum"/>.
        /// </exception>
        public static TEnum PowerOfTwo<TEnum>(object power)
            where TEnum : struct
        {
            var type = typeof(TEnum);
            if (!type.GetTypeInfo().IsEnum)
                throw new ArgumentException("Type provided must be an enumeration.", "TEnum");

            var result = default(TEnum);
            if (TryPowerOfTwo(power, out result))
                return result;

            throw new ArgumentException("No match found from two raised to the specified power.", "power");
        }

        /// <summary>
        /// Tries to get a value of <typeparamref name="TEnum"/> from two raised to the specified <paramref name="power"/>.
        /// </summary>
        /// <typeparam name="TEnum">An enumeration type.</typeparam>
        /// <param name="power">The raising power of two.</param>
        /// <param name="result">When this methods returns, contains the matching value of <typeparamref name="TEnum"/>. This parameter is passed uninitialized.</param>
        /// <returns><b>true</b> if a matching value of <typeparamref name="TEnum"/> is found; otherwise, <b>false</b>.</returns>
        public static bool TryPowerOfTwo<TEnum>(object power, out TEnum result)
            where TEnum : struct
        {
            result = default(TEnum);
            var type = typeof(TEnum);
            if (!type.GetTypeInfo().IsEnum)
                return false;

            if (power == null || !typeof(IConvertible).GetTypeInfo().IsAssignableFrom(power.GetType().GetTypeInfo()))
                return false;

            var v = Math.Pow(2, Convert.ToDouble(power));
            if (Math.Floor(v) != v)
                return false;

            var underlyingValue = FlagsEnum.ConvertToUnderlyingEnumType<TEnum>(v);
            if (!Enum.IsDefined(type, underlyingValue))
                return false;

            result = (TEnum)Enum.ToObject(type, underlyingValue);
            return true;
        }

        static object ConvertToUnderlyingEnumType<TEnum>(object value)
        {
            var type = typeof(TEnum);
            if (!type.GetTypeInfo().IsEnum)
                throw new ArgumentException("Type provided must be an enumeration.", "TEnum");

            var underlyingType = Enum.GetUnderlyingType(type);

            if (underlyingType == typeof(sbyte))
                return Convert.ToSByte(value);
            if (underlyingType == typeof(short))
                return Convert.ToInt16(value);
            if (underlyingType == typeof(int))
                return Convert.ToInt32(value);
            if (underlyingType == typeof(long))
                return Convert.ToInt64(value);
            if (underlyingType == typeof(byte))
                return Convert.ToByte(value);
            if (underlyingType == typeof(ushort))
                return Convert.ToUInt16(value);
            if (underlyingType == typeof(uint))
                return Convert.ToUInt32(value);
            if (underlyingType == typeof(ulong))
                return Convert.ToUInt64(value);

            throw new NotSupportedException("Underlying type of provided type is not supported.");
        }

        /// <summary>
        /// Gets the power of two exponent from the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">A value.</param>
        /// <returns>The power of two exponent from the specified <paramref name="value"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="value"/> is not a power of two exponent.</exception>
        public static int PowerOfTwoExponent(object value)
        {
            var result = default(int);
            if (TryPowerOfTwoExponent(value, out result))
                return result;

            throw new ArgumentException("Argument is not a power of two exponent.", "value");
        }

        /// <summary>
        /// Tries to get the power of two exponent from the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">A value.</param>
        /// <param name="result">When this methods returns, contains the power of two exponent from the specified <paramref name="value"/>. This parameter is passed uninitialized.</param>
        /// <returns><b>true</b> if the <paramref name="value"/> parameter is a power of two exponent; otherwise, <b>false</b>.</returns>
        public static bool TryPowerOfTwoExponent(object value, out int result)
        {
            result = default(int);
            if (value == null || !typeof(IConvertible).GetTypeInfo().IsAssignableFrom(value.GetType().GetTypeInfo()))
                return false;

            var v = Math.Log(Convert.ToDouble(value), 2);
            if (Math.Floor(v) != v)
                return false;

            result = (int)v;
            return true;
        }

        /// <summary>
        /// Gets the flag values defined in <typeparamref name="TEnum"/>.
        /// </summary>
        /// <typeparam name="TEnum">An enumeration type.</typeparam>
        /// <returns>The defined flag values.</returns>
        /// <exception cref="ArgumentException"><typeparamref name="TEnum"/> is not an enumeration type.</exception>
        public static IEnumerable<TEnum> FlagValues<TEnum>()
            where TEnum : struct
        {
            var type = typeof(TEnum);
            if (!type.GetTypeInfo().IsEnum)
                throw new ArgumentException("Type provided must be an enumeration.", "TEnum");

            // call an inner method to avoid deferred argument check
            return FlagValues<TEnum>(type);
        }

        static IEnumerable<TEnum> FlagValues<TEnum>(Type type)
            where TEnum : struct
        {
            foreach (TEnum value in Enum.GetValues(type))
            {
                int result;
                if (TryPowerOfTwoExponent(value, out result))
                    yield return value;
            }
        }

        /// <summary>
        /// Gets the flagged values defined in <typeparamref name="TEnum"/>.
        /// </summary>
        /// <typeparam name="TEnum">An enumeration type.</typeparam>
        /// <returns>The defined flagged values.</returns>
        /// <exception cref="ArgumentException"><typeparamref name="TEnum"/> is not an enumeration type.</exception>
        public static IEnumerable<TEnum> FlaggedValues<TEnum>()
            where TEnum : struct
        {
            var type = typeof(TEnum);
            if (!type.GetTypeInfo().IsEnum)
                throw new ArgumentException("Type provided must be an enumeration.", "TEnum");

            // call an inner method to avoid deferred argument check
            return FlaggedValues<TEnum>(type);
        }

        static IEnumerable<TEnum> FlaggedValues<TEnum>(Type type)
            where TEnum : struct
        {
            foreach (TEnum value in Enum.GetValues(type))
            {
                int result;
                if (!TryPowerOfTwoExponent(value, out result))
                    yield return value;
            }
        }

        /// <summary>
        /// Gets the flag values of the specified <paramref name="value"/>.
        /// </summary>
        /// <typeparam name="TEnum">An enumeration type.</typeparam>
        /// <param name="value">A combined value of <typeparamref name="TEnum"/>.</param>
        /// <returns>The flag values of the specified <paramref name="value"/>.</returns>
        /// <exception cref="ArgumentException"><typeparamref name="TEnum"/> is not an enumeration type.</exception>
        public static IEnumerable<TEnum> FlagValues<TEnum>(TEnum value)
            where TEnum : struct
        {
            var type = typeof(TEnum);
            if (!type.GetTypeInfo().IsEnum)
                throw new ArgumentException("Type provided must be an enumeration.", "TEnum");

            // call an inner method to avoid deferred argument check
            return FlagValues<TEnum>(type, value);
        }

        static IEnumerable<TEnum> FlagValues<TEnum>(Type type, TEnum value)
             where TEnum : struct
        {
            foreach (var item in FlagValues<TEnum>(type))
            {
                if ((Convert.ToInt64(value) & Convert.ToInt64(item)) == Convert.ToInt64(item))
                    yield return item;
            }
        }

        /// <summary>
        /// Indicates if the flag values of the specified <paramref name="value"/> are continuous or not.
        /// </summary>
        /// <typeparam name="TEnum">An enumeration type.</typeparam>
        /// <param name="value">A combined value of <typeparamref name="TEnum"/>.</param>
        /// <returns><b>true</b> if the flag values of the specified <paramref name="value"/> are continuous; otherwise, <b>false</b>.</returns>
        /// <exception cref="ArgumentException"><typeparamref name="TEnum"/> is not an enumeration type.</exception>
        public static bool ContinuousFlagValues<TEnum>(TEnum value)
            where TEnum : struct
        {
            var type = typeof(TEnum);
            if (!type.GetTypeInfo().IsEnum)
                throw new ArgumentException("Type provided must be an enumeration.", "TEnum");

            // call an inner method to avoid deferred argument check
            return ContinuousFlagValues<TEnum>(type, value);
        }

        static bool ContinuousFlagValues<TEnum>(Type type, TEnum value)
            where TEnum : struct
        {
            var array = FlagValues(type, value).ToArray();
            if (array.Length == 0)
                return false;
            if (array.Length == 1)
                return true;

            var modulo = FlagValues<TEnum>(type).Count();
            //for each flag value consider continuity with the ones following cyclically (modulo)
            //for example Sunday|Monday|Tuesday|Friday|Saturday is continuous
            //when considering Friday with the flag values following: Saturday, Sunday, Monday, Tuesday
            for (int i = 0; i < array.Length; i++)
            {
                var continuous = true;
                var item1 = array[i];
                for (int j = 1; j < array.Length; j++)
                {
                    var item2 = array[(i + j) % array.Length];
                    if ((PowerOfTwoExponent(item1) + j) % modulo != PowerOfTwoExponent(item2))
                    {
                        continuous = false;
                        break;
                    }
                }

                if (continuous)
                    return true;
            }

            return false;
        }

    }
}
