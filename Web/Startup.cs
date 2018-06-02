using Diov.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Diov.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

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

            services.AddMvc();
            services.AddRouting(
                routeOptions => routeOptions.LowercaseUrls = true);
        }

        public void Configure(
            IApplicationBuilder applicationBuilder,
            IHostingEnvironment hostingEnvironment)
        {
            var migrator = new Migrator(
                Configuration.GetConnectionString("Sql"));
            migrator.Migrate();

            applicationBuilder.UseHsts();

            applicationBuilder.UseAuthentication();

            if (hostingEnvironment.IsDevelopment())
            {
                applicationBuilder.UseDeveloperExceptionPage();
            }
            else
            {
                applicationBuilder
                    .UseStatusCodePagesWithReExecute("/error/{0}");

                applicationBuilder.UseHttpsRedirection();
            }

            applicationBuilder.UseStaticFiles();

            applicationBuilder.UseMvc();
        }
    }
}