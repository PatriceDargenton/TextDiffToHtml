
using System.ComponentModel;
using System.Reflection;

namespace TextDiffToHtml
{
    //public enum StringEnum
    //{
    //    [Description("First Value with spaces")]
    //    FirstValue,
    //    [Description("Second Value with spaces")]
    //    SecondValue,
    //    [Description("Third Value with spaces")]
    //    ThirdValue
    //}
    //StringEnum result1 = EnumsNET.Enums.Parse<StringEnum>("FirstValue", ignoreCase: true);
    //StringEnum result2 = EnumsNET.Enums.Parse<StringEnum>("SecondValue", ignoreCase: true);
    //StringEnum result3 = typeof(StringEnum).GetValueFromDescription<StringEnum>("Second Value with spaces");
    //StringEnum result4 = typeof(StringEnum).GetValueFromDescription<StringEnum>("Third Value with spaces");

    internal static class EnumHelper
    {
        public static T GetDefaultValue<T>()
            where T : struct, Enum
        {
            var defaultValue = GetDefaultValue(typeof(T));
            if (defaultValue == null) return default;
            return (T)defaultValue;
        }

        public static object? GetDefaultValue(Type enumType)
        {
            var attribute = enumType.GetCustomAttribute<DefaultValueAttribute>(inherit: false);
            if (attribute != null) return attribute.Value ?? Activator.CreateInstance(enumType);

            var innerType = enumType.GetEnumUnderlyingType();
            var zero = Activator.CreateInstance(innerType);
            if (zero != null && enumType.IsEnumDefined(zero)) return zero;

            var values = enumType.GetEnumValues();
            return values.GetValue(0) ?? Activator.CreateInstance(enumType);
        }

        public static string GetEnumDescription<T>() where T : Enum
        {
            var type = typeof(T);
            var attributes = (DescriptionAttribute[])type.GetCustomAttributes(
                typeof(DescriptionAttribute), inherit: false);
            return attributes.Length > 0 ? attributes[0].Description : type.Name;
        }
    }

    public static class EnumExtensions
    {
        // EnumsNET: See https://github.com/TylerBrinkley/Enums.NET
        public static string ToShortDescription<TEnum>(this TEnum enumValue)
            where TEnum : struct, Enum
        {
            var a = EnumsNET.Enums.GetMember(enumValue);
            if (a is null) return "";
            var descr = a.Attributes.Get<DescriptionAttribute>()?.Description ?? "";
            if (string.IsNullOrEmpty(descr)) return "";
            descr = Helper.RemoveAfterParenthesis(descr) ?? "";
            return descr;
        }

        public static string ToDescription<TEnum>(this TEnum enumValue)
            where TEnum : struct, Enum
        {
            var a = EnumsNET.Enums.GetMember(enumValue);
            if (a is null) return "";
            return a.Attributes.Get<DescriptionAttribute>()?.Description ?? "";
        }

        public static TEnum GetValueFromDescription<TEnum>(this Type enumType, string description)
            where TEnum : struct
        {
            var names = Enum.GetNames(enumType).ToList();
            var values = Enum.GetValues(enumType).Cast<TEnum>().ToList();

            for (int i = 0; i < names.Count; i++)
            {
                var member = enumType.GetMember(names[i]).FirstOrDefault();
                var descriptionAttribute = member?.GetCustomAttribute<DescriptionAttribute>();
                var descr = descriptionAttribute?.Description;
                descr = Helper.RemoveAfterParenthesis(descr);

                if (descr == description)
                {
                    return values[i];
                }
            }
            var x = default(TEnum);
            return x;
        }

        public static T GetValueFromValue<T>(string value)
            where T : Enum
        {
            foreach (var field in typeof(T).GetFields())
            {
                //Debug.WriteLine("Field name : " + field.Name);
                if (field.Name == value)
                {
                    var value0 = field.GetValue(null);
                    if (value0 != null) return (T)value0;
                }
            }

            throw new ArgumentException("Not found.", nameof(value));
            // Or return default(T);
        }
    }
}