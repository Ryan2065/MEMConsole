using System.Net;
using System.Text;

namespace AdminServiceForwarder
{
    /// <summary>
    /// Interface so ASForwarder can be used in DI
    /// </summary>
    public interface IASForwarder
    {
        /// <summary>
        /// Proxies the request to the real AdminServer
        /// </summary>
        /// <param name="urlPath">Path the user is requesting - this will be everything after /AdminService</param>
        /// <returns></returns>
        Task<string> RunRequestAsync(string urlPath);
    }
    /// <summary>
    /// Forwarder class which proxies requests to the AdminService
    /// </summary>
    public class ASForwarder : IASForwarder
    {
        private readonly HttpContext _context;
        private readonly AdminServiceForwarderSettings _asSettings;
        public ASForwarder(IHttpContextAccessor httpContext, AdminServiceForwarderSettings asSettings)
        {
            _context = httpContext?.HttpContext!;
            _asSettings = asSettings;
        }
        /// <summary>
        /// Gets the HttpMethod object from the string method in the request
        /// </summary>
        /// <param name="method">The string value of the method</param>
        /// <returns>HttpMethod equivalent</returns>
        private static HttpMethod GetMethod(string method)
        {
            return method.ToUpper() switch
            {
                "GET" => HttpMethod.Get,
                "POST" => HttpMethod.Post,
                "PUT" => HttpMethod.Put,
                "DELETE" => HttpMethod.Delete,
                "PATCH" => HttpMethod.Patch,
                _ => HttpMethod.Get,
            };
        }
        /// <summary>
        /// Will forward the request to the real admin service
        /// </summary>
        /// <param name="urlPath">Path the user requested</param>
        /// <returns>AdminService results</returns>
        /// <exception cref="AccessViolationException">Exception if the user requested WindowsAuth but isn't authenticated</exception>
        public async Task<string> RunRequestAsync(string urlPath)
        {
            string urlToCall = $"https://{_asSettings.AdminServiceServer}/AdminService/{urlPath}?{_context.Request.QueryString}";
            var client = AsForwarderHttpClient.GetHttpClient(_asSettings.EnableWindowsAuth, _asSettings.DisableSSLCertificateValidation);
            
            var httpRequestMessage = new HttpRequestMessage(GetMethod(_context.Request.Method), urlToCall);
            if(_context.Request.Body != null)
            {
                if (!_context.Request.Body.CanSeek) { _context.Request.EnableBuffering(); } 
                _context.Request.Body.Position = 0;
                var reader = new StreamReader(_context.Request.Body, Encoding.UTF8);
                httpRequestMessage.Content = new StringContent(await reader.ReadToEndAsync().ConfigureAwait(false));
            }
            foreach(var h in _context.Request.Headers)
            {
                if(h.Key.Equals("Authentication", StringComparison.OrdinalIgnoreCase) || h.Key.Equals("ExpiresOn", StringComparison.OrdinalIgnoreCase))
                {
                    httpRequestMessage.Headers.TryAddWithoutValidation(h.Key, h.Value.ToString());
                }
            }
            
            if (_context.User.Identity != null && _context.User.Identity.IsAuthenticated && _asSettings.EnableWindowsAuth)
            {
                var wi = (System.Security.Principal.WindowsIdentity)_context.User.Identity;
                return await System.Security.Principal.WindowsIdentity.RunImpersonatedAsync(wi.AccessToken, async () =>
                {
                    var responseBody = await client.SendAsync(httpRequestMessage);
                    responseBody.EnsureSuccessStatusCode();
                    return await responseBody.Content.ReadAsStringAsync();
                });
            }
            else if (_asSettings.EnableWindowsAuth)
            {
                
                throw new AccessViolationException($"Settings say to use Windows Auth but no user is authenticated {_context.User.Identity?.Name}");
            }
            else
            {
                var responseBody = await client.SendAsync(httpRequestMessage);
                responseBody.EnsureSuccessStatusCode();
                return await responseBody.Content.ReadAsStringAsync();
            }
        }
    }
    /// <summary>
    /// HttpClients are meant to be reused, so this is a static class to ensure we don't cause 
    /// issues and reuse the same http client
    /// </summary>
    public static class AsForwarderHttpClient
    {
        static HttpClient? _client;
        /// <summary>
        /// This will get a client configured based on the params. If the client doesn't already exist, it will create it
        /// </summary>
        /// <param name="windowsAuth">Should DefaultCredentials be used?</param>
        /// <param name="disableSSLCertValidation">Should the SSL verification be disabled?</param>
        /// <returns>Properly configured http client</returns>
        public static HttpClient GetHttpClient(bool windowsAuth, bool disableSSLCertValidation)
        {
            if(_client == null)
            {
                var httpHandler = new HttpClientHandler();
                if (windowsAuth)
                {
                    httpHandler.UseDefaultCredentials = true;
                }
                if (disableSSLCertValidation)
                {
                    httpHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
                    {
                        return true;
                    };
                }
                _client = new HttpClient(httpHandler);
            }
            return _client;
        }
    }
}
