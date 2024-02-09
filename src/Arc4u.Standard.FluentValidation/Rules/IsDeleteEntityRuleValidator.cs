using Arc4u.Data;
using FluentValidation;
using FluentValidation.Resources;
using FluentValidation.Validators;
using System;

namespace Arc4u.FluentValidation.Rules
{
    public class IsDeleteEntityRuleValidator<T, TProperty> : PropertyValidator<T, TProperty> where T : IPersistEntity where TProperty : Enum
    {
        static string ruleName = "IsDeleteRuleValidator";
        public override string Name => ruleName;

        static IsDeleteEntityRuleValidator()
        {
            var lgMgr = ValidatorOptions.Global.LanguageManager as LanguageManager;
            lgMgr?.AddTranslation("en", ruleName, "PersistEntity is expected to be set as Delete and is {PropertyValue}.");
        }
        public override bool IsValid(ValidationContext<T> context, TProperty value)
        {
            return value.Equals(PersistChange.Delete);
        }

        protected override string GetDefaultMessageTemplate(string errorCode)
        {
            return Localized(errorCode, Name);
        }
    }
}
