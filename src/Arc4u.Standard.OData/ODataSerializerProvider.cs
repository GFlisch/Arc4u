using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter.Serialization;

namespace Arc4u.OData
{
    internal class ODataSerializerProvider : Microsoft.AspNetCore.OData.Formatter.Serialization.ODataSerializerProvider
    {
        private readonly Uri _odataBaseAddress;

        public ODataSerializerProvider(IServiceProvider rootContainer, Uri odataBaseAddress)
            : base(rootContainer)
        {
            _odataBaseAddress = odataBaseAddress;
        }

        public override IODataSerializer GetODataPayloadSerializer(Type type, HttpRequest request)
        {
            var odataFeature = request.ODataFeature();
            var prefix = odataFeature.RoutePrefix + '/';
            if (!_odataBaseAddress.LocalPath.EndsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"The local path of the OData base address ({_odataBaseAddress.LocalPath}) does not end with the OData route prefix ({prefix})");
            }

            request.PathBase = _odataBaseAddress.LocalPath.Substring(0, _odataBaseAddress.LocalPath.Length - prefix.Length);
            // this is the reason why any alternative base address can only change the scheme and the host, but never the whole path: otherwise, we would need to write our own payload serializer
            // Not sure about the Path/PathBase would be a reliable option.
            request.Scheme = _odataBaseAddress.Scheme;
            request.Host = _odataBaseAddress.IsDefaultPort ? new HostString(_odataBaseAddress.Host) : new HostString(_odataBaseAddress.Host, _odataBaseAddress.Port);
            return base.GetODataPayloadSerializer(type, request);
        }
    }
}
