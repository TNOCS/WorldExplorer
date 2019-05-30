using System;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

public class NetworkUtil
{

    public class ApiException : Exception
    {
        public int StatusCode { get; set; }

        public string Content { get; set; }
    }

    public static async Task<String> DownloadStringAsync(CancellationToken cancellationToken, string url)
    {
        using (var client = new HttpClient())
        using (var request = new HttpRequestMessage(HttpMethod.Get, url))
        using (var response = await client.SendAsync(request /*, cancellationToken*/))
        {
            ServicePointManager.ServerCertificateValidationCallback = TrustCertificate;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode == false)
            {
                throw new ApiException
                {
                    StatusCode = (int)response.StatusCode,
                    Content = content
                };
            }
            return content;
        }
    }

    public static async Task<byte[]> DownloadDataAsync(CancellationToken cancellationToken, string url)
    {
        using (var client = new HttpClient())
        using (var request = new HttpRequestMessage(HttpMethod.Get, url))
        using (var response = await client.SendAsync(request /*, cancellationToken */))
        {
            ServicePointManager.ServerCertificateValidationCallback = TrustCertificate;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            var content = await response.Content.ReadAsByteArrayAsync();
            if (response.IsSuccessStatusCode == false)
            {
                throw new ApiException
                {
                    StatusCode = (int)response.StatusCode,
                    Content = ""
                };
            }
            return content;
        }
    }

    private static bool TrustCertificate(object sender, X509Certificate x509Certificate, X509Chain x509Chain, SslPolicyErrors sslPolicyErrors)
    {
        // all Certificates are accepted
        return true;
    }
}