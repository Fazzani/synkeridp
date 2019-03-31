using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Skoruba.IdentityServer4.Admin.BusinessLogic.Extensions;
using Skoruba.IdentityServer4.Admin.BusinessLogic.Identity.Dtos.Identity;
using Skoruba.IdentityServer4.Admin.BusinessLogic.Identity.Extensions;
using SynkerIdpAdminUI.Admin.Configuration.Interfaces;
using Skoruba.IdentityServer4.Admin.EntityFramework.DbContexts;
using Skoruba.IdentityServer4.Admin.EntityFramework.Identity.Entities.Identity;
using SynkerIdpAdminUI.Admin.Helpers;
using System.IO;
using Serilog;

namespace SynkerIdpAdminUI.Admin
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }
        public Microsoft.Extensions.Logging.ILogger Logger { get; set; }
        public IHostingEnvironment HostingEnvironment { get; }

        public Startup(IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables();

            if (env.IsDevelopment())
            {
                builder.AddUserSecrets<Startup>();
            }

            Configuration = builder.Build();

            HostingEnvironment = env;

            Logger = loggerFactory.CreateLogger<Startup>();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

            services.ConfigureRootConfiguration(Configuration);
            var rootConfiguration = services.BuildServiceProvider().GetService<IRootConfiguration>();

            services.AddDbContexts<AdminDbContext>(HostingEnvironment, Configuration);
            services.AddAuthenticationServices<AdminDbContext, UserIdentity, UserIdentityRole>(HostingEnvironment, rootConfiguration.AdminConfiguration);
            services.AddMvcExceptionFilters();

            services.AddAdminServices<AdminDbContext>();

            services.AddAdminAspNetIdentityServices<AdminDbContext, UserDto<int>, int, RoleDto<int>, int, int, int,
                                UserIdentity, UserIdentityRole, int, UserIdentityUserClaim, UserIdentityUserRole,
                                UserIdentityUserLogin, UserIdentityRoleClaim, UserIdentityUserToken>();

            services.AddMvcLocalization();
            services.AddAuthorizationPolicies();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.AddLogging(loggerFactory, Configuration);

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            app.Map("/liveness", lapp => lapp.Run(async ctx => ctx.Response.StatusCode = 200));

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseSecurityHeaders();
            app.UseStaticFiles();
            app.ConfigureAuthentificationServices(env);
            app.ConfigureLocalization();

            app.UseMvc(routes =>
            {
                routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}