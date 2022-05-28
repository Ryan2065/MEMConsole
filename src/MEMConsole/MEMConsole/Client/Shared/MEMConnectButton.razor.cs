using Blazorise;
using MEMConsole.Client.Models;
using Microsoft.JSInterop;
using System.Text;

namespace MEMConsole.Client.Shared
{
    public partial class MEMConnectButton
    {
        private bool modalIsVisible = false;

        private readonly string guidRegexPattern = "(^([0-9A-Fa-f]{8}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{12})$)";

        private string connectButtonString = "Connect";
        private string connectButtonLabel = "Show connect modal to connect to MEM Console";

        private string connectedTo = "";

        protected override async Task OnInitializedAsync()
        {
            if(MEMConnectStatus.ConnectionSetupError != null && MEMConnectStatus.WroteException == false)
            {
                logger.LogError(MEMConnectStatus.ConnectionSetupError, "Could not enable authentication with provided settings. Click Connect button to review settings");
            }
            await memClient.Load();
            if (memClient.IsAuthenticated && !connectButtonString.Equals("Reconnect"))
            {
                try
                {
                    var uri = new Uri(memClient.AdminServiceUrl ?? "");
                    connectedTo = $"Hello {memClient.UserName}, you're connected to {uri.Host}";
                    connectButtonString = "Reconnect";
                    connectButtonLabel = "Show connection modal to connect to a different MEM Console";
                }
                catch(Exception ex)
                {
                    connectedTo = "Could not parse AdminService URI";
                    logger.LogError(ex, "Could not parse AdminService URL");
                }
                StateHasChanged();
            }
        }

        async Task ConnectClicked()
        {
            await memClient.Load();
            modalIsVisible = true;
            StateHasChanged();
        }

        Task TenantIdTextChanged(string value)
        {
            memClient.TenantId = value;
            SetScope();
            SetAuthority();
            StateHasChanged();
            return Task.CompletedTask;
        }

        void ModalConnectClicked()
        {
            memClient.LoginUser();
            modalIsVisible = false;
        }


        Task ServerAppIdTextChanged(string value)
        {
            memClient.ServerAppId = value;
            SetScope();
            StateHasChanged();
            return Task.CompletedTask;
        }

        private void SetAuthority()
        {
            if (string.IsNullOrEmpty(memClient.Authority) || memClient.Authority.StartsWith("https://login.windows.net/") || string.IsNullOrEmpty(memClient.Authority))
            {
                memClient.Authority = $"https://login.windows.net/{memClient.TenantId}";
            }
        }

        private void SetScope()
        {
            var newScope = $"api://{memClient.TenantId}/{memClient.ServerAppId}/.default";
            if(memClient.Scopes == null) { memClient.Scopes = ""; }
            var splitScopes = memClient.Scopes.Split(",");
            var scopeStringBuilder = new StringBuilder();
            for(var i = 0; i < splitScopes.Length; i++)
            {
                if(i == 0)
                {
                    scopeStringBuilder.Append(newScope);
                }
                else
                {
                    scopeStringBuilder.Append(',');
                    scopeStringBuilder.Append(splitScopes[i]);
                }
            }
            memClient.Scopes = scopeStringBuilder.ToString();
        }
    }
    public static class MEMConnectStatus
    {
        public static Exception? ConnectionSetupError { get; set; }
        public static bool WroteException { get; set; } = false;
    }
}
