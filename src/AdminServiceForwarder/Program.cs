using AdminServiceForwarder;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;

var config = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile("appsettings.json")
                 .Build();

var adminServiceForwarderSettings = config.GetSection("AdminServiceForwarderSettings").Get<AdminServiceForwarderSettings>();

var builder = WebApplication.CreateBuilder(args);

var CORSOrigins = adminServiceForwarderSettings.AllowedCORSOrigins;
var enableWindowsAuth = adminServiceForwarderSettings.EnableWindowsAuth;

if(CORSOrigins.Length > 0)
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("_adminServiceCorsPolicy", policy =>
        {
            policy.WithOrigins(CORSOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        });
    });
}

if (enableWindowsAuth)
{
    builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
       .AddNegotiate();
    builder.Services.AddAuthorization(options =>
    {
        // By default, all incoming requests will be authorized according to the default policy.
        options.FallbackPolicy = options.DefaultPolicy;
    });
}

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton(typeof(AdminServiceForwarderSettings), adminServiceForwarderSettings);
builder.Services.AddScoped<IASForwarder, ASForwarder>();

var certToUse = ASGetCertificate.GetCertificate();
if(certToUse == null) { throw new ApplicationException("Could not find a valid certificate. Please specify a cert in settings or put one in the LocalSystem My cert store."); }

builder.WebHost.ConfigureKestrel((context, serverOptions) =>
{
    var kestrelSection = context.Configuration.GetSection("Kestrel");
    serverOptions.Configure(kestrelSection);
    serverOptions.ConfigureHttpsDefaults(def => def.ServerCertificate = certToUse);
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();
app.UseCors("_adminServiceCorsPolicy");

if (enableWindowsAuth)
{
    app.UseAuthentication();
    app.UseAuthorization();
}

if (enableWindowsAuth)
{
    app.MapGet("/AdminService/{*adminServiceGetAll}", [Authorize] async (string adminServiceGetAll, IASForwarder forwarder) =>
    {
        return await forwarder.RunRequestAsync(adminServiceGetAll);
    });

    app.MapPost("/AdminService/{*adminServiceGetAll}", [Authorize] async (string adminServiceGetAll, IASForwarder forwarder) =>
    {
        return await forwarder.RunRequestAsync(adminServiceGetAll);
    });

    app.MapPut("/AdminService/{*adminServiceGetAll}", [Authorize] async (string adminServiceGetAll, IASForwarder forwarder) =>
    {
        return await forwarder.RunRequestAsync(adminServiceGetAll);
    });

    app.MapDelete("/AdminService/{*adminServiceGetAll}", [Authorize] async (string adminServiceGetAll, IASForwarder forwarder) =>
    {
        return await forwarder.RunRequestAsync(adminServiceGetAll);
    });
}
else
{
    app.MapGet("/AdminService/{*adminServiceGetAll}", async (string adminServiceGetAll, IASForwarder forwarder) =>
    {
        return await forwarder.RunRequestAsync(adminServiceGetAll);
    });

    app.MapPost("/AdminService/{*adminServiceGetAll}", async (string adminServiceGetAll, IASForwarder forwarder) =>
    {
        return await forwarder.RunRequestAsync(adminServiceGetAll);
    });

    app.MapPut("/AdminService/{*adminServiceGetAll}", async (string adminServiceGetAll, IASForwarder forwarder) =>
    {
        return await forwarder.RunRequestAsync(adminServiceGetAll);
    });


    app.MapDelete("/AdminService/{*adminServiceGetAll}", async (string adminServiceGetAll, IASForwarder forwarder) =>
    {
        return await forwarder.RunRequestAsync(adminServiceGetAll);
    });
}



app.Run();

