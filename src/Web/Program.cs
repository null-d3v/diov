﻿using Diov.Data;
using Diov.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Caching.Distributed;
using Serilog;
using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebMarkupMin.AspNetCore8;

#pragma warning disable CA1031

try
{
    Log.Logger = new LoggerConfiguration()
        .CreateBootstrapLogger();

    var webApplicationBuilder = WebApplication
        .CreateBuilder(args);
    webApplicationBuilder.Configuration
        .AddJsonFile("appsettings.json", false, false)
        .AddEnvironmentVariables();

    if (!webApplicationBuilder.Environment.IsDevelopment())
    {
        webApplicationBuilder.Environment.WebRootFileProvider =
            new NoWatchFileProvider(
                webApplicationBuilder
                    .Environment
                    .WebRootFileProvider);
    }

    webApplicationBuilder.Host
        .UseSerilog(
            (context, loggerConfiguration) =>
                loggerConfiguration
                    .ReadFrom.Configuration(
                        context.Configuration));

    webApplicationBuilder.Services
        .AddSingleton(webApplicationBuilder.Configuration);

    webApplicationBuilder.Services
        .AddOptions();
    webApplicationBuilder.Services
        .Configure<DistributedCacheEntryOptions>(
            options =>
            {
                options.AbsoluteExpirationRelativeToNow =
                    TimeSpan.FromDays(1);
            });
    webApplicationBuilder.Services
        .Configure<ExternalAuthenticationOptions>(
        webApplicationBuilder.Configuration
            .GetSection("ExternalAuthentication"));
    webApplicationBuilder.Services
        .Configure<JsonSerializerOptions>(
            options =>
            {
                options.NumberHandling = JsonNumberHandling
                    .AllowReadingFromString;
                options.PropertyNameCaseInsensitive = true;
            });

    webApplicationBuilder.Services
        .AddSingleton<IDbConnectionFactory>(
            new DbConnectionFactory(
                webApplicationBuilder.Configuration
                    .GetConnectionString("Sql")!));
    webApplicationBuilder.Services
        .AddTransient<
            IAdminAuthorizationRepository,
            AdminAuthorizationRepository>();
    webApplicationBuilder.Services
        .AddTransient<IContentRepository, ContentRepository>();
    webApplicationBuilder.Services
        .AddHostedService<MigrationHostedService>();

    webApplicationBuilder.Services
        .AddL1L2RedisCache(
            options =>
            {
                options.Configuration = webApplicationBuilder.Configuration
                    .GetConnectionString("Redis");
                options.InstanceName = Constants.RedisInstanceName;
            });
    webApplicationBuilder.Services
        .AddTransient<
            IContentAccessor,
            DistributedCacheContentAccessor>();

    webApplicationBuilder.Services
        .Configure<ForwardedHeadersOptions>(
            options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor |
                    ForwardedHeaders.XForwardedProto;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });
    webApplicationBuilder.Services
        .AddResponseCompression();
    webApplicationBuilder.Services
        .AddWebOptimizer(
            assetPipeline =>
            {
                assetPipeline.MinifyCssFiles(
                    "css/**/*.css");
                assetPipeline.MinifyJsFiles(
                    "js/**/*.js");
                assetPipeline.AddCssBundle(
                    "/css/site.min.css",
                    "css/**/*.css");
                assetPipeline.AddJavaScriptBundle(
                    "/js/site.min.js",
                    "js/**/*.js");
            },
            options =>
            {
                options.EnableDiskCache = false;
            });
    webApplicationBuilder.Services
        .AddWebMarkupMin(
            options =>
            {
                options.AllowMinificationInDevelopmentEnvironment = true;
                options.AllowCompressionInDevelopmentEnvironment = true;
            })
        .AddHtmlMinification()
        .AddHttpCompression();

    webApplicationBuilder.Services
        .AddDataProtection()
        .PersistKeysToStackExchangeRedis(
            () => ConnectionMultiplexer
                .Connect(
                    webApplicationBuilder.Configuration
                        .GetConnectionString("Redis")!)
                .GetDatabase(),
                $"{Constants.RedisInstanceName}DataProtection-Keys");

    var externalAuthenticationOptions = webApplicationBuilder.Configuration
        .GetSection("ExternalAuthentication")
        .Get<ExternalAuthenticationOptions>();
    var authenticationBuilder = webApplicationBuilder.Services
        .AddAuthentication(
            Constants.LocalAuthenticationScheme)
        .AddCookie(
            Constants.LocalAuthenticationScheme,
            cookieOptions =>
            {
                cookieOptions.LoginPath = "/auth/login";
            })
        .AddCookie(
            Constants.ExternalAuthenticationScheme);
    if (!string.IsNullOrEmpty(
            externalAuthenticationOptions?.Google?.ClientId) &&
        !string.IsNullOrEmpty(
            externalAuthenticationOptions?.Google?.ClientSecret))
    {
        authenticationBuilder
            .AddGoogle(
                googleOptions =>
                {
                    googleOptions.ClientId = externalAuthenticationOptions
                        .Google.ClientId;
                    googleOptions.ClientSecret = externalAuthenticationOptions
                        .Google.ClientSecret;
                    googleOptions.SignInScheme = Constants
                        .ExternalAuthenticationScheme;
                });
    }
    webApplicationBuilder.Services
        .AddTransient<
            IAuthenticationSchemeProvider,
            IgnoreCaseAuthenticationSchemeProvider>();

    webApplicationBuilder.Services
        .AddRouting(
            routeOptions =>
            {
                routeOptions.LowercaseUrls = true;
            });

    webApplicationBuilder.Services
        .AddHealthChecks()
        .AddRedis(
            webApplicationBuilder.Configuration
                .GetConnectionString("Redis")!,
            tags: [ Constants.ReadinessHealthCheckTag, ])
        .AddSqlServer(
            webApplicationBuilder.Configuration
                .GetConnectionString("Sql")!,
            tags: [ Constants.ReadinessHealthCheckTag, ]);

    var mvcBuilder = webApplicationBuilder.Services
        .AddControllersWithViews();
    if (webApplicationBuilder.Environment.IsDevelopment())
    {
        mvcBuilder.AddRazorRuntimeCompilation();
    }

    using var webApplication = webApplicationBuilder
        .Build();

    webApplication
        .UseSerilogRequestLogging();

    if (webApplicationBuilder.Environment.IsDevelopment())
    {
        webApplication
            .UseDeveloperExceptionPage();
    }
    else
    {
        webApplication
            .UseExceptionHandler("/error/500");
        webApplication
            .UseStatusCodePagesWithReExecute("/error/{0}");
    }

    webApplication
        .UseResponseCompression()
        .UseWebOptimizer()
        .UseWebMarkupMin()
        .UseStaticFiles(
            new StaticFileOptions
            {
                ServeUnknownFileTypes = true,
            })
        .UseRouting();

    webApplication
        .UseForwardedHeaders();

    webApplication
        .UseAuthentication()
        .UseAuthorization();

    webApplication
        .MapHealthChecks(
            "/health/live",
            new HealthCheckOptions
            {
                Predicate = (healthCheckRegistration) => false,
            });
    webApplication
        .MapHealthChecks(
        "/health/ready",
            new HealthCheckOptions
            {
                Predicate = (healthCheckRegistration) =>
                    healthCheckRegistration.Tags.Contains(
                        Constants.ReadinessHealthCheckTag),
            });
    webApplication
        .MapControllers();

    await webApplication
        .RunAsync()
        .ConfigureAwait(false);
    return 0;
}
catch (Exception exception)
{
    Log.Fatal(
        exception,
        "Host terminated unexpectedly");
    return 1;
}
finally
{
    await Log
        .CloseAndFlushAsync()
        .ConfigureAwait(false);
}

#pragma warning restore CA1031