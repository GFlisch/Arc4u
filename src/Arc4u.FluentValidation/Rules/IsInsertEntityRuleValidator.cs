using Arc4u.Data;
using FluentValidation;
using FluentValidation.Resources;
using FluentValidation.Validators;

namespace Arc4u.FluentValidation.Rules;

public class IsInsertEntityRuleValidator<T, TProperty> : PropertyValidator<T, TProperty> where T : IPersistEntity where TProperty : Enum
{
    const string ruleName = "IsInsertRuleValidator";
    public override string Name => ruleName;

    static IsInsertEntityRuleValidator()
    {
        var lgMgr = ValidatorOptions.Global.LanguageManager as LanguageManager;
        lgMgr?.AddTranslation("en", ruleName, "PersistEntity is expected to be set as Insert and is {PropertyValue}.");
    }
    public override bool IsValid(ValidationContext<T> context, TProperty value)
    {
        return value.Equals(PersistChange.Insert);
    }

    protected override string GetDefaultMessageTemplate(string errorCode)
    {
        return Localized(errorCode, Name);
    }
}
