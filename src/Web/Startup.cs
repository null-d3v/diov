using Diov.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

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

            applicationBuilder.UseStaticFiles();

            applicationBuilder.UseRouting();

            if (!WebHostEnvironment.IsDevelopment())
            {
                applicationBuilder.UseHsts();
                applicationBuilder.UseHttpsRedirection();
            }

            applicationBuilder.UseAuthentication();
            applicationBuilder.UseAuthorization();

            applicationBuilder.UseEndpoints(
                endpointRouteBuilder =>
                {
                    endpointRouteBuilder.MapControllers();
                });

            var migrator = new Migrator(
                Configuration.GetConnectionString("Sql"));
            migrator.Migrate();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<ExternalAuthenticationOptions>(
                Configuration.GetSection("ExternalAuthentication"));

            services.AddSingleton<IDbConnectionFactory>(
                new DbConnectionFactory(
                    Configuration.GetConnectionString("Sql")));
            services.AddTransient<
                IAdminAuthorizationRepository,
                AdminAuthorizationRepository>();
            services.AddTransient<IContentRepository, ContentRepository>();

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
            var mvcBuilder = services.AddControllersWithViews();
            if (WebHostEnvironment.IsDevelopment())
            {
                mvcBuilder.AddRazorRuntimeCompilation();
            }
        }
    }
}