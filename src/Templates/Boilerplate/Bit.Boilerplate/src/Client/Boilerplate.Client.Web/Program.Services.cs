﻿//+:cnd:noEmit
//#if (appInsights == true)
using BlazorApplicationInsights;
//#endif
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Boilerplate.Client.Web.Services;
using Boilerplate.Shared;
using Boilerplate.Client.Core;
using Microsoft.Extensions.Options;

namespace Boilerplate.Client.Web;

public static partial class Program
{
    public static void ConfigureServices(this WebAssemblyHostBuilder builder)
    {
        // Services being registered here can get injected in web project only.

        var services = builder.Services;
        var configuration = builder.Configuration;

        services.AddClientWebProjectServices();

        configuration.AddClientConfigurations();

        builder.Logging.AddConfiguration(configuration.GetSection("Logging"));

        Uri.TryCreate(configuration.GetServerAddress(), UriKind.RelativeOrAbsolute, out var serverAddress);

        if (serverAddress!.IsAbsoluteUri is false)
        {
            serverAddress = new Uri(new Uri(builder.HostEnvironment.BaseAddress), serverAddress);
        }

        services.AddSessioned(sp => new HttpClient(sp.GetRequiredService<HttpMessageHandler>()) { BaseAddress = serverAddress });

        //#if (appInsights == true)
        services.AddBlazorApplicationInsights(x =>
        {
            var connectionString = configuration.Get<ClientAppSettings>()!.ApplicationInsights?.ConnectionString;
            if (string.IsNullOrEmpty(connectionString) is false)
            {
                x.ConnectionString = connectionString;
            }
        },
        async appInsights =>
        {
            await appInsights.AddTelemetryInitializer(new()
            {
                Tags = new Dictionary<string, object?>()
                {
                    { "ai.application.ver", typeof(Program).Assembly.GetName().Version!.ToString() }
                }
            });
        });
        //#endif

        services.AddOptions<SharedAppSettings>()
            .Bind(configuration)
            .ValidateOnStart();

        services.AddOptions<ClientAppSettings>()
            .Bind(configuration)
            .ValidateOnStart();

        services.AddTransient(sp => sp.GetRequiredService<IOptionsSnapshot<SharedAppSettings>>().Value);
        services.AddTransient(sp => sp.GetRequiredService<IOptionsSnapshot<ClientAppSettings>>().Value);
    }

    public static void AddClientWebProjectServices(this IServiceCollection services)
    {
        services.AddClientCoreProjectServices();

        // Services being registered here can get injected in both web project and server (during prerendering).

        services.AddTransient<IBitDeviceCoordinator, WebDeviceCoordinator>();
        services.AddTransient<IExceptionHandler, WebExceptionHandler>();
        //#if (notification == true)
        services.AddScoped<IPushNotificationService, WebPushNotificationService>();
        //#endif
    }
}
