using Arc4u.Data;
using Arc4u.FluentValidation.Rules;
using Arc4u.ServiceModel;
using FluentValidation;
using FluentValidation.Results;
using System;

namespace Arc4u.FluentValidation;

public static class DefaultValidatorExtensions
{
    public static IRuleBuilderOptions<T, TProperty> IsInsert<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder) where T : IPersistEntity where TProperty : Enum
    {
        return ruleBuilder.SetValidator(new IsInsertEntityRuleValidator<T, TProperty>());
    }

    public static IRuleBuilderOptions<T, TProperty> IsUpdate<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder) where T : IPersistEntity where TProperty : Enum
    {
        return ruleBuilder.SetValidator(new IsUpdateEntityRuleValidator<T, TProperty>());
    }

    public static IRuleBuilderOptions<T, TProperty> IsDelete<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder) where T : IPersistEntity where TProperty : Enum
    {
        return ruleBuilder.SetValidator(new IsDeleteEntityRuleValidator<T, TProperty>());
    }

    public static IRuleBuilderOptions<T, TProperty> IsNone<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder) where T : IPersistEntity where TProperty : Enum
    {
        return ruleBuilder.SetValidator(new IsNoneEntityRuleValidator<T, TProperty>());
    }

    public static IRuleBuilderOptions<T, TProperty> IsUtcDateTime<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder) where T : class where TProperty : struct
    {
        return ruleBuilder.SetValidator(new IsUtcDateTimeRuleValidator<T, TProperty>());
    }

    public static IRuleBuilderOptions<T, TProperty> IsDateOnly<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder) where T : class where TProperty : struct
    {
        return ruleBuilder.SetValidator(new IsUtcDateOnlyRuleValidator<T, TProperty>());
    }

    public static Messages ToMessages(this ValidationResult result)
    {
        Messages messages = new();

        foreach (var r in result.Errors)
        {
            messages.Add(new Message(MessageCategory.Technical, r.Severity.ToMessageType(), r.ErrorCode, "", r.ErrorMessage));
        }

        return messages;
    }

    public static MessageType ToMessageType(this Severity severity)
    {
        switch (severity)
        {
            case Severity.Error:
                return MessageType.Error;
            case Severity.Warning:
                return MessageType.Warning;
            case Severity.Info:
                return MessageType.Information;
            default:
                return MessageType.Error;

        }
    }
}
