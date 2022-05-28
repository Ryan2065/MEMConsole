using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace MEMConsole.Client.Models
{
    public class MemConsoleMessageHandler : AuthorizationMessageHandler
    {
        public MemConsoleMessageHandler(IAccessTokenProvider provider, NavigationManager navigation) : base(provider, navigation)
        {
            List<string> authorizedUrls = new() { MEMAuthenticationState.AuthorizedUrl };
            List<string> scopes = new();
            scopes.AddRange(MEMAuthenticationState.Scopes.Split(","));
            ConfigureHandler(authorizedUrls: authorizedUrls, scopes: scopes);

        }
    }
}
