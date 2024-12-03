using System.Globalization;
using System.Text;

namespace Arc4u.Dependency.ComponentModel;

public class MultipleRegistrationException<T> : Exception
{
    public MultipleRegistrationException(IEnumerable<T> instances) : base(MakeMessage(instances)) { }

    private static string MakeMessage(IEnumerable<T> instances)
    {
        var content = new StringBuilder();

        if (null == instances || !instances.Any())
        {
            content.AppendLine(string.Format(CultureInfo.InvariantCulture, "No registrations exist for type {0}.", typeof(T).Name));
            return content.ToString();
        }

        content.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0} registrations exist for type {1}.", instances.Count(), typeof(T).Name));
        content.AppendLine("Only one is expected.");
        foreach (var instance in instances)
        {
            if (instance is not null)
            {
                content.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0} is registered.", instance.GetType().Name));
            }
        }

        content.AppendLine("Use ResolveAll instead.");

        return content.ToString();
    }
}
