namespace Arc4u.Diagnostics
{
    public class OpenTelemetrySettings
    {
        public OpenTelemetrySettings()
        {
            Attributes = new Dictionary<string, object>();
        }
        public string Address { get; set; }

        public Dictionary<string, object> Attributes { get; set; }

        public List<string> Sources { get; set; }
    }
}