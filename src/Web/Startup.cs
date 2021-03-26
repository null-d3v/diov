using Diov.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Diov.Web
{
    public class Startup
    {
        public Startup(
            IConfiguration configuration,
            IWebHostEnvironment webHostEnvironment)
        {
            Configuration = configuration;
            WebHostEnvironment = webHostEnvironment;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment WebHostEnvironment { get; }

        public void Configure(
            IApplicationBuilder applicationBuilder)
        {
            applicationBuilder.UseSerilogRequestLogging();

            if (WebHostEnvironment.IsDevelopment())
            {
                applicationBuilder.UseDeveloperExceptionPage();
            }
            else
            {
                applicationBuilder
                    .UseExceptionHandler("/error/500");
                applicationBuilder
                    .UseStatusCodePagesWithReExecute("/error/{0}");
            }

            applicationBuilder.UseResponseCompression();
            applicationBuilder.UseCookiePolicy();
            applicationBuilder.UseStaticFiles();
            applicationBuilder.UseRouting();

            applicationBuilder.UseForwardedHeaders();

            applicationBuilder.UseAuthentication();
            applicationBuilder.UseAuthorization();

            applicationBuilder.UseEndpoints(
                endpointRouteBuilder =>
                {
                    endpointRouteBuilder.MapHealthChecks(
                        "/health/live",
                        new HealthCheckOptions
                        {
                            Predicate = (healthCheckRegistration) => false,
                        });
                    endpointRouteBuilder.MapHealthChecks(
                        "/health/ready",
                        new HealthCheckOptions
                        {
                            Predicate = (healthCheckRegistration) =>
                                healthCheckRegistration.Tags.Contains(
                                    Constants.ReadinessHealthCheckTag),
                        });
                    endpointRouteBuilder.MapControllers();
                });
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<JsonSerializerOptions>(
                options =>
                {
                    options.NumberHandling = JsonNumberHandling
                        .AllowReadingFromString;
                    options.PropertyNameCaseInsensitive = true;
                });
            services.Configure<ExternalAuthenticationOptions>(
                Configuration.GetSection("ExternalAuthentication"));

            services.AddSingleton<IDbConnectionFactory>(
                new DbConnectionFactory(
                    Configuration.GetConnectionString("Sql")));
            services.AddTransient<
                IAdminAuthorizationRepository,
                AdminAuthorizationRepository>();
            services.AddTransient<IContentRepository, ContentRepository>();
            services.AddHostedService<MigrationHostedService>();

            services.Configure<ForwardedHeadersOptions>(
                options =>
                {
                    options.ForwardedHeaders =
                        ForwardedHeaders.XForwardedFor |
                        ForwardedHeaders.XForwardedProto;
                    options.KnownNetworks.Clear();
                    options.KnownProxies.Clear();
                });
            services.AddResponseCompression();

            services.AddDataProtection()
                .PersistKeysToStackExchangeRedis(
                    ConnectionMultiplexer.Connect(
                        Configuration.GetConnectionString("Redis")));

            var externalAuthenticationOptions = Configuration
                .GetSection("ExternalAuthentication")
                .Get<ExternalAuthenticationOptions>();
            var authenticationBuilder = services
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
                    externalAuthenticationOptions?.Google.ClientId) &&
                !string.IsNullOrEmpty(
                    externalAuthenticationOptions?.Google.ClientSecret))
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
            services.AddTransient<
                IAuthenticationSchemeProvider,
                IgnoreCaseAuthenticationSchemeProvider>();

            services.AddRouting(
                routeOptions => routeOptions.LowercaseUrls = true);

            services
                .AddHealthChecks()
                .AddRedis(
                    Configuration.GetConnectionString("Redis"),
                    tags: new[] { Constants.ReadinessHealthCheckTag, })
                .AddSqlServer(
                    Configuration.GetConnectionString("Sql"),
                    tags: new[] { Constants.ReadinessHealthCheckTag, });

            var mvcBuilder = services
                .AddControllersWithViews();
            if (WebHostEnvironment.IsDevelopment())
            {
                mvcBuilder.AddRazorRuntimeCompilation();
            }

        }
    }
}