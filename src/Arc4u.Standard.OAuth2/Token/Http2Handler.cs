namespace Arc4u.OAuth2
{
    /// <summary>
    /// Force the Http request to use Http 2.0
    /// </summary>
    public class Http2Handler : DelegatingHandler
    {
        public Http2Handler() : base(new HttpClientHandler())
        {
        }

        public Http2Handler(DelegatingHandler handler) : base(handler)
        {
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Version = new Version("2.0");

            return base.SendAsync(request, cancellationToken);
        }
    }
}
