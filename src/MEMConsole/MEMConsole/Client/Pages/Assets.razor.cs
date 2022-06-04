using MEMConsole.Client.Models;
using System.Text.Json;
namespace MEMConsole.Client.Pages
{
    public partial class Assets
    {
        private string smsRSystem = "";
        
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            var devices = await AdminService.Device.Get();
            var client = ClientFactory.CreateClient("MEMClient");
            smsRSystem = await client.GetStringAsync("wmi/SMS_R_System");
            
            StateHasChanged();
            //JsonSerializer.Deserialize<string>(smsRSystem);
            /*var httpClient = memClient.GetClient();
            var response = await httpClient.GetStringAsync(@"HTTPS://EPHINGADMINCMGZ.EPHINGADMIN.COM/CCM_Proxy_ServerAuth/72057594037927941/AdminService/wmi/SMS_R_System");
            var jsonObj = await JsonSerializer.Deserialize<dynamic>(response);
            smsRSystem = response.ToString();
            StateHasChanged();
            await base.OnAfterRenderAsync(firstRender);*/
        }
    }
}
