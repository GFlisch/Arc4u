namespace Arc4u.Network
{
    public class Handler
    {
        /// <summary>
        /// This static propery is used to implement any code that should be called before doing a HttpClient request.
        /// The idea is to force a vpn connexion for sample.
        /// </summary>
        public static Action<Uri> OnCalling { get; set; }
    }
}
