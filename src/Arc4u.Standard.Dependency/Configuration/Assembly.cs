namespace Arc4u.Dependency.Configuration
{
    public class AssemblyConfig
    {
        public string Assembly { get; set; }

        public ICollection<string> RejectedTypes { get; set; }
    }
}
