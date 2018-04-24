using Diov.Data;
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
            services.Configure<AuthenticationOptions>(
                Configuration.GetSection("Authentication"));

            services.AddSingleton<IDbConnectionFactory>(
                new DbConnectionFactory(
                    Configuration.GetConnectionString("Sql")));
            services.AddTransient<IContentRepository, ContentRepository>();

            var authenticationOptions = Configuration
                .GetSection("Authentication")
                .Get<AuthenticationOptions>();
            var authenticationBuilder = services
                .AddAuthentication(
                    Constants.LocalAuthenticationScheme)
                .AddCookie(
                    Constants.LocalAuthenticationScheme,
                    cookieOptions =>
                    {
                        cookieOptions.LoginPath = "/login";
                    })
                .AddCookie(
                    Constants.ExternalAuthenticationScheme);
            if (!string.IsNullOrEmpty(
                authenticationOptions?.Google.ClientId) &&
                !string.IsNullOrEmpty(
                authenticationOptions?.Google.ClientSecret))
            {
                authenticationBuilder
                    .AddGoogle(
                        googleOptions =>
                        {
                            googleOptions.ClientId = authenticationOptions
                                .Google.ClientId;
                            googleOptions.ClientSecret = authenticationOptions
                                .Google.ClientSecret;
                            googleOptions.SignInScheme = Constants
                                .ExternalAuthenticationScheme;
                        });
            }

            services.AddMvc();
        }

        public void Configure(
            IApplicationBuilder applicationBuilder,
            IHostingEnvironment hostingEnvironment)
        {
            var migrator = new Migrator(
                Configuration.GetConnectionString("Sql"));
            migrator.Migrate();

            applicationBuilder.UseAuthentication();

            if (hostingEnvironment.IsDevelopment())
            {
                applicationBuilder.UseDeveloperExceptionPage();
            }
            else
            {
                applicationBuilder
                    .UseStatusCodePagesWithReExecute("/error/{0}");

                applicationBuilder.UseRewriter(
                    new RewriteOptions()
                        .AddRedirectToHttps());
            }

            applicationBuilder.UseStaticFiles();

            applicationBuilder.UseMvc();
        }
    }
}