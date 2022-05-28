using System.Text;

namespace MEMConsole.Client.Shared
{
    public partial class MEMConnect
    {
        protected override async Task OnInitializedAsync()
        {
            await memClient.Load();
            StateHasChanged();
        }
        private readonly string guidRegexPattern = "(^([0-9A-Fa-f]{8}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{12})$)";
        void ConnectClicked()
        {
            memClient.LoginUser();
        }
        Task TenantIdTextChanged(string value)
        {
            memClient.TenantId = value;
            SetScope();
            SetAuthority();
            StateHasChanged();
            return Task.CompletedTask;
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
            if (memClient.Scopes == null) { memClient.Scopes = ""; }
            var splitScopes = memClient.Scopes.Split(",");
            var scopeStringBuilder = new StringBuilder();
            for (var i = 0; i < splitScopes.Length; i++)
            {
                if (i == 0)
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
}
