

using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Net.Http.Headers;

namespace MEMConsole.Client.Models
{
    public class MEMConsoleConnection
    {
        private readonly ISyncLocalStorageService _localStorage = default!;
        private readonly NavigationManager _navManager = default!;
        private readonly AuthenticationStateProvider? _authenticationStateProvider;
        public MEMConsoleConnection(ISyncLocalStorageService localStorage, NavigationManager navManager, AuthenticationStateProvider authenticationStateProvider)
        {
            _localStorage = localStorage;
            _navManager = navManager;
            _authenticationStateProvider = authenticationStateProvider;
        }
        public MEMConsoleConnection(ISyncLocalStorageService localStorage, NavigationManager navManager)
        {
            _localStorage = localStorage;
            _navManager = navManager;
        }
        public string? TenantId { get; set; }
        public string? ServerAppId { get; set; }
        public string? ClientId { get; set; }
        public string? Authority { get; set; }
        public string? Scopes { get; set; }
        public string? AdminServiceUrl { get; set; }
        public string? UserName { get; set; }
        public bool IsAuthenticated { get; set; } = false;

        private bool loaded = false;

        public void Save()
        {
            _localStorage.SetItem("MEMConsoleConnection", GetAsDictionary());
        }

        public async Task Load()
        {
            if (loaded) { return; }
            if(_authenticationStateProvider != null)
            {
                var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
                IsAuthenticated = authState.User.Identity?.IsAuthenticated ?? false;
                UserName = authState.User.Identity?.Name ?? "";
            }
            
            var memConsole = _localStorage.GetItem<Dictionary<string, string>>("MEMConsoleConnection");
            if (memConsole != null)
                SetFromDictionary(memConsole);
            loaded = true;
        }

        public void LoginUser()
        {
            Save();
            _navManager.NavigateTo("/authentication/login");
        }

        public Dictionary<string, string> GetAsDictionary()
        {
            var returnDictionary = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(TenantId)) { returnDictionary.Add("TenantId", TenantId); }
            if (!string.IsNullOrEmpty(ServerAppId)) { returnDictionary.Add("ServerAppId", ServerAppId); }
            if (!string.IsNullOrEmpty(ClientId)) { returnDictionary.Add("ClientId", ClientId); }
            if (!string.IsNullOrEmpty(Authority)) { returnDictionary.Add("Authority", Authority); }
            if (!string.IsNullOrEmpty(Scopes)) { returnDictionary.Add("Scopes", Scopes); }
            if (!string.IsNullOrEmpty(AdminServiceUrl)) { returnDictionary.Add("AdminServiceUrl", AdminServiceUrl); }
            if (!string.IsNullOrEmpty(UserName)) { returnDictionary.Add("UserName", UserName); }
            return returnDictionary;
        }

        public void SetFromDictionary(Dictionary<string, string> memClientSettings)
        {
            if (memClientSettings == null) { return; }
            foreach (var key in memClientSettings.Keys)
            {
                if (key.Equals("TenantId", StringComparison.OrdinalIgnoreCase)) { TenantId = memClientSettings[key]; }
                else if (key.Equals("ServerAppId", StringComparison.OrdinalIgnoreCase)) { ServerAppId = memClientSettings[key]; }
                else if (key.Equals("ClientId", StringComparison.OrdinalIgnoreCase)) { ClientId = memClientSettings[key]; }
                else if (key.Equals("Authority", StringComparison.OrdinalIgnoreCase)) { Authority = memClientSettings[key]; }
                else if (key.Equals("Scopes", StringComparison.OrdinalIgnoreCase)) { Scopes = memClientSettings[key]; }
                else if (key.Equals("AdminServiceUrl", StringComparison.OrdinalIgnoreCase)) { AdminServiceUrl = memClientSettings[key]; }
                else if (key.Equals("UserName", StringComparison.OrdinalIgnoreCase)) { UserName = memClientSettings[key]; }
            }
        }

    }
}
