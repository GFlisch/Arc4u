using Serilog.Events;
using Serilog.Formatting.Json;

namespace Arc4u.Diagnostics.Formatter
{
    /// <summary>
    /// Build a json output of the properties added!
    /// </summary>
    public class JsonPropertiesFormatter
    {
        public JsonPropertiesFormatter()
        {
            Formatter = new JsonValueFormatter();
        }

        private JsonValueFormatter Formatter { get; set; }

        public void Format(List<LogEventProperty> properties, TextWriter output)
        {
            if (properties.Count > 0)
            {
                output.Write("{");
                var separator = String.Empty;
                foreach (var property in properties)
                {
                    output.Write($"{separator}\"{property.Name}\":");
                    Formatter.Format(property.Value, output);
                    separator = ",";
                }
                output.Write("}");
            }
        }
    }
}
