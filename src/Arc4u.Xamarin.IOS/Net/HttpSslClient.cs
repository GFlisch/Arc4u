using Arc4u.OAuth2.Net;
using Foundation;
using Newtonsoft.Json;
using Security;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Arc4u.Net
{
    [System.Composition.Export("SSL", typeof(IHttpClient))]
    public class HttpSslClient : NSUrlConnectionDataDelegate, IHttpClient
    {
        const String contentType = "Content-Type";

        private TaskCompletionSource<HttpResponseMessage> _taskSource;
        public double TimeOut { get; set; } = 30;
        public X509Certificate2 Certificate { get; set; }

        #region IHttpClient
        public async Task<HttpResponseMessage> GetAsync(Uri requestUri, IDictionary<String, String> headers)
        {
            return await Send(requestUri, String.Empty, "GET", headers);
        }

        public async Task<HttpResponseMessage> PostAsync<T>(Uri requestUri, T value, IDictionary<String, String> headers)
        {
            return await Send(requestUri, value, "POST", headers);
        }

        public async Task<HttpResponseMessage> PostAsync(Uri requestUri, string content, IDictionary<String, String> headers)
        {
            return await Send(requestUri, content, "POST", headers);
        }

        private async Task<HttpResponseMessage> Send<T>(Uri requestUri, T value, string method, IDictionary<String, String> headers)
        {
            var content = JsonConvert.SerializeObject(value);

            var head = headers;
            if (null == headers)
            {
                head = new Dictionary<String, String>();
            }

            if (head.Keys.Contains(contentType))
            {
                head.Remove(contentType);
            }
            head.Add(contentType, "application/json; charset=utf-8");

            return await Send(requestUri, content, method, head);
        }

        private async Task<HttpResponseMessage> Send(Uri requestUri, string content, string method, IDictionary<String, String> headers)
        {
            if (null == Certificate)
                throw new ApplicationException("No certificate has been provided. The request is cancelled.");

            _taskSource = new TaskCompletionSource<HttpResponseMessage>();

            var request = new NSMutableUrlRequest(new NSUrl(requestUri.ToString()), NSUrlRequestCachePolicy.ReloadIgnoringCacheData, TimeOut)
            {
                HttpMethod = method,
                Body = NSData.FromString(content)
            };

            if (null != headers)
            {
                foreach (var header in headers)
                    request[header.Key] = new NSString(header.Value);
            }

            var conn = NSUrlConnection.FromRequest(request, this);

            return await _taskSource.Task;
        }

        public async Task<HttpResponseMessage> PutAsync<T>(Uri requestUri, T value, IDictionary<String, String> headers)
        {
            return await Send(requestUri, value, "PUT", headers);
        }
        public async Task<HttpResponseMessage> PutAsync(Uri requestUri, string content, IDictionary<String, String> headers)
        {
            return await Send(requestUri, content, "PUT", headers);
        }

        public async Task<HttpResponseMessage> PatchAsync<T>(Uri requestUri, T value, IDictionary<String, String> headers)
        {
            return await Send(requestUri, value, "PATCH", headers);
        }
        public async Task<HttpResponseMessage> PatchAsync(Uri requestUri, string content, IDictionary<String, String> headers)
        {
            return await Send(requestUri, content, "PATCH", headers);
        }
        #endregion

        #region NSUrlConnectionDataDelegate
        byte[] result = new byte[0];
        int status_code = -1;
        public override void ReceivedData(NSUrlConnection connection, NSData data)
        {

            byte[] nb = new byte[result.Length + (int)data.Length];

            result.CopyTo(nb, 0);

            Marshal.Copy(data.Bytes, nb, result.Length, (int)data.Length);

            result = nb;

        }

        public override void ReceivedResponse(NSUrlConnection connection, NSUrlResponse response)
        {
            if (response is NSHttpUrlResponse http_response)
            {
                status_code = (int)http_response.StatusCode;
            }
        }

        public override void FailedWithError(NSUrlConnection connection, NSError error)
        {
            _taskSource.SetException(new Exception(error.Description));
        }

        public override void FinishedLoading(NSUrlConnection connection)
        {
            try
            {
                // Build the response.
                var response = new HttpResponseMessage((HttpStatusCode)status_code)
                {
                    Content = new ByteArrayContent(result)
                };

                _taskSource.SetResult(response);
            }
            catch (Exception ex)
            {
                _taskSource.SetException(ex);
            }

        }

        public override void WillSendRequestForAuthenticationChallenge(NSUrlConnection connection, NSUrlAuthenticationChallenge challenge)
        {
            var identity = SecIdentity.Import(Certificate);
            var certificate = new SecCertificate(Certificate);
            SecCertificate[] certificates = { certificate };

            var credential = NSUrlCredential.FromIdentityCertificatesPersistance(identity, certificates, NSUrlCredentialPersistence.ForSession);

            challenge.Sender.UseCredential(credential, challenge);
        }
        #endregion
    }
}