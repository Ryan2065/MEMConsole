using Blazored.LocalStorage;
using MEMConsole.Client;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using MEMConsole.Client.Models;
using Microsoft.JSInterop;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddLogging();
var localStorage = (ISyncLocalStorageService)builder.Services.BuildServiceProvider().GetRequiredService<ISyncLocalStorageService>();
var memClientSettings = localStorage.GetItem<Dictionary<string, string>>("MEMConsoleConnection");
if(memClientSettings != null)
{
    try
    {
        if(!string.IsNullOrEmpty(memClientSettings["Authority"]) 
            && !string.IsNullOrEmpty(memClientSettings["ClientId"]) 
            && !string.IsNullOrEmpty(memClientSettings["Scopes"]) 
            && !string.IsNullOrEmpty(memClientSettings["AdminServiceUrl"])
        )
        {
            // Set auth props in our static properties for use in injected services
            if (!memClientSettings["AdminServiceUrl"].EndsWith('/'))
            {
                memClientSettings["AdminServiceUrl"] = memClientSettings["AdminServiceUrl"] + "/";
            }
            MEMAuthenticationState.AuthorizedUrl = memClientSettings["AdminServiceUrl"];
            MEMAuthenticationState.Scopes = memClientSettings["Scopes"];

            // add auth with user-supplied information from the browser
            builder.Services.AddMsalAuthentication(options =>
            {
                options.ProviderOptions.Authentication.Authority = memClientSettings["Authority"];
                options.ProviderOptions.Authentication.ClientId = memClientSettings["ClientId"];
                options.ProviderOptions.Authentication.RedirectUri = @"https://localhost:7075";
                options.ProviderOptions.Authentication.ValidateAuthority = true;
                var splitScopes = memClientSettings["Scopes"].Split(",");
                foreach (var scope in splitScopes)
                {
                    options.ProviderOptions.DefaultAccessTokenScopes.Add(scope);
                }
            });

            // inject the HttpClient that can use the auth headers
            builder.Services.AddScoped<MemConsoleMessageHandler>();
            builder.Services.AddHttpClient("MEMClient",
                client => client.BaseAddress = new Uri(MEMAuthenticationState.AuthorizedUrl))
                .AddHttpMessageHandler<MemConsoleMessageHandler>();

            // Signal to the app we configured auth
            MEMConsole.Client.Models.MEMAuthenticationState.UsingAuthentication = true;
        }
    }
    catch (Exception ex) 
    {
        // In Blazor WASM you can't log from Program.cs - so stashing the error and MEMConnect.razor.cs will log it,
        // since that's loaded on every page and directly handles setting these settings
        MEMConsole.Client.Shared.MEMConnectStatus.ConnectionSetupError = ex;
    }
}
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddHttpClient();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddSingleton(serviceProvider => (IJSInProcessRuntime)serviceProvider.GetRequiredService<IJSRuntime>());
builder.Services.AddTransient<MEMConsoleConnection>();

builder.Services
    .AddBlazorise(options =>
    {
        options.Immediate = true;
    })
    .AddBootstrap5Providers()
    .AddFontAwesomeIcons();


await builder.Build().RunAsync();
