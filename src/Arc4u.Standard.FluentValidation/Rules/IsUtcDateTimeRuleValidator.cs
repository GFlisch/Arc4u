using FluentValidation;
using FluentValidation.Resources;
using FluentValidation.Validators;
using System;

namespace Arc4u.FluentValidation.Rules
{
    internal class IsUtcDateTimeRuleValidator<T, TProperty> : PropertyValidator<T, TProperty> where T : class where TProperty : struct
    {
        static string ruleName = "IsUtcDateTimeRuleValidator";
        public override string Name => ruleName;

        static IsUtcDateTimeRuleValidator()
        {
            var lgMgr = ValidatorOptions.Global.LanguageManager as LanguageManager;
            lgMgr?.AddTranslation("en", ruleName, "Property must be a DateTime and in Utc {PropertyValue}.");
        }

        public override bool IsValid(ValidationContext<T> context, TProperty value)
        {
            var dt = value as DateTime?;
            return dt is { Kind: DateTimeKind.Utc };
        }

        protected override string GetDefaultMessageTemplate(string errorCode)
        {
            return Localized(errorCode, Name);
        }
    }
}
