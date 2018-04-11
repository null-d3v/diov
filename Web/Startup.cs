using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Diov.Data;
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
            services.AddSingleton<IDbConnectionFactory>(
                new DbConnectionFactory(
                    Configuration.GetConnectionString("Sql")));
            services.AddTransient<IContentRepository, ContentRepository>();

            services.AddMvc();
        }

        public void Configure(
            IApplicationBuilder applicationBuilder,
            IHostingEnvironment hostingEnvironment)
        {
            var migrator = new Migrator(
                Configuration.GetConnectionString("Sql"));
            migrator.Migrate();

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