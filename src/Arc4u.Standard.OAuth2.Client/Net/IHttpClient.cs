using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Arc4u.OAuth2.Net
{
    public interface IHttpClient
    {
        X509Certificate2 Certificate { get; set; }

        double TimeOut { get; set; }

        Task<HttpResponseMessage> GetAsync(Uri requestUri, IDictionary<String, String> headers);

        Task<HttpResponseMessage> PostAsync<T>(Uri requestUri, T value, IDictionary<String, String> headers);
        Task<HttpResponseMessage> PostAsync(Uri requestUri, string content, IDictionary<String, String> headers);

        Task<HttpResponseMessage> PutAsync<T>(Uri requestUri, T value, IDictionary<String, String> headers);
        Task<HttpResponseMessage> PutAsync(Uri requestUri, string content, IDictionary<String, String> headers);

        Task<HttpResponseMessage> PatchAsync<T>(Uri requestUri, T value, IDictionary<String, String> headers);
        Task<HttpResponseMessage> PatchAsync(Uri requestUri, string content, IDictionary<String, String> headers);
    }
}
