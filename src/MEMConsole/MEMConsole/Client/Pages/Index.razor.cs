using MEMConsole.Client.Models;
using Microsoft.AspNetCore.Components;

namespace MEMConsole.Client.Pages
{
    public partial class Index
    {
        private string ClientIdScript { get; set; } = "";
        [Inject] public MEMConsoleConnection MemClient { get; set; } = default!;
        protected override async Task OnInitializedAsync()
        {
            await MemClient.Load();
            
            ClientIdScript = "Import-Module Az.Resources" + Environment.NewLine
                             + $"$TenantId = '{MemClient.TenantId}'" + Environment.NewLine
                             + $"$ServerAppId = '{MemClient.ServerAppId}'" + Environment.NewLine
                             + "" + Environment.NewLine
                             + "$MEMConsoleUrl = 'https://localhost:7075'" + Environment.NewLine
                             + "" + Environment.NewLine
                             + "Connect-AzAccount -Tenant $TenantId" + Environment.NewLine
                             + "" + Environment.NewLine
                             + "$ServerApp = Get-AzAdApplication -AppId $ServerAppId" + Environment.NewLine
                             + "$ServerAppScopeId = ''" + Environment.NewLine
                             + "foreach($oauthScope in $ServerApp.Api.Oauth2PermissionScope){" + Environment.NewLine
                             + "    if($oauthScope.Value -eq 'user_impersonation'){" + Environment.NewLine
                             + "        $ServerAppScopeId = $oauthScope.Id" + Environment.NewLine
                             + "    }" + Environment.NewLine
                             + "}" + Environment.NewLine
                             + "" + Environment.NewLine
                             + "$AzureADAppParams = @{" + Environment.NewLine
                             + "    'DisplayName' = 'MEMConsoleAzureApp'" + Environment.NewLine
                             + "    'AvailableToOtherTenants' = $false" + Environment.NewLine
                             + "    'SPARedirectUri' = @($MEMConsoleUrl, \"$($MEMConsoleUrl)/Login\")" + Environment.NewLine
                             + "    'Description' = 'Application to give MEMConsole.com access to ConfigMgr Admin Service App'" + Environment.NewLine
                             + "    'RequiredResourceAccess' = @{" + Environment.NewLine
                             + "        'ResourceAccess' = @(" + Environment.NewLine
                             + "            @{" + Environment.NewLine
                             + "                'Id' = $ServerAppScopeId" + Environment.NewLine
                             + "                'Type' = 'Scope'" + Environment.NewLine
                             + "            }" + Environment.NewLine
                             + "        )" + Environment.NewLine
                             + "        'ResourceAppId' = $ServerAppId" + Environment.NewLine
                             + "    }" + Environment.NewLine
                             + "}" + Environment.NewLine
                             + "" + Environment.NewLine
                             + "New-AzADApplication @AzureADAppParams" + Environment.NewLine;
            StateHasChanged();
        }
    }
}
