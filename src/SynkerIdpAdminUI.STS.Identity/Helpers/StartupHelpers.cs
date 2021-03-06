﻿namespace SynkerIdpAdminUI.STS.Identity.Helpers
{
    using IdentityModel;
    using IdentityServer4.Services;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.HttpOverrides;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Localization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json.Linq;
    using Serilog;
    using StsServerIdentity.Resources;
    using SynkerIdpAdminUI.STS.Identity.Configuration;
    using SynkerIdpAdminUI.STS.Identity.Configuration.Constants;
    using SynkerIdpAdminUI.STS.Identity.Filters;
    using SynkerIdpAdminUI.STS.Identity.Models.Models;
    using SynkerIdpAdminUI.STS.Identity.Resources;
    using SynkerIdpAdminUI.STS.Identity.Services;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Security.Claims;
    using ILogger = Microsoft.Extensions.Logging.ILogger;

    public static class StartupHelpers
    {
        public static void AddMvcLocalization(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddLocalization(opts => opts.ResourcesPath = ConfigurationConsts.ResourcesPath);
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
            services.AddSingleton<LocService>();
            services.AddTransient<IEmailSender, EmailSender>();
            services.Configure<RequestLocalizationOptions>(
                options =>
                {
                    var supportedCultures = new List<CultureInfo>
                        {
                            new CultureInfo("en-US"),
                            new CultureInfo("de-CH"),
                            new CultureInfo("fr-CH"),
                            new CultureInfo("it-CH")
                        };

                    options.DefaultRequestCulture = new RequestCulture(culture: "en-US", uiCulture: "en-US");
                    options.SupportedCultures = supportedCultures;
                    options.SupportedUICultures = supportedCultures;

                    var providerQuery = new LocalizationQueryProvider
                    {
                        QureyParamterName = "ui_locales"
                    };

                    options.RequestCultureProviders.Insert(0, providerQuery);
                });

            services.AddMvc(options =>
            {
                options.Filters.Add(new SecurityHeadersAttribute());
            })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddViewLocalization()
                .AddDataAnnotationsLocalization(options =>
                {
                    options.DataAnnotationLocalizerProvider = (type, factory) =>
                    {
                        var assemblyName = new AssemblyName(typeof(SharedResource).GetTypeInfo().Assembly.FullName);
                        return factory.Create("SharedResource", assemblyName.Name);
                    };
                });
        }

        public static void UseSecurityHeaders(this IApplicationBuilder app)
        {
            app.UseHsts(hsts => hsts.MaxAge(365).IncludeSubdomains());
            app.UseXContentTypeOptions();
            app.UseReferrerPolicy(opts => opts.NoReferrer());
            app.UseXXssProtection(options => options.EnabledWithBlockMode());

            var optionsFH = new ForwardedHeadersOptions()
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
                RequireHeaderSymmetry = false
            };

            optionsFH.KnownNetworks.Clear();
            optionsFH.KnownProxies.Clear();
            app.UseForwardedHeaders(optionsFH);

            app.UseXfo(options => options.SameOrigin());
        }

        public static void AddAuthenticationServices<TContext, TUserIdentity, TUserIdentityRole>(this IServiceCollection services, IHostingEnvironment hostingEnvironment, IConfiguration configuration, ILogger logger) where TContext : DbContext
            where TUserIdentity : class where TUserIdentityRole : class
        {
            var connectionString = configuration.GetConnectionString(ConfigurationConsts.AdminConnectionStringKey);
            var stsAuthConfig = configuration.GetSection(nameof(StsAuthentificationConfiguration));

            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            services.AddIdentity<TUserIdentity, TUserIdentityRole>()
                .AddEntityFrameworkStores<TContext>()
                .AddErrorDescriber<StsIdentityErrorDescriber>()
                .AddDefaultTokenProviders();

            services.AddTransient<IProfileService, SynkerProfileService>();

            services.Configure<IISOptions>(iis =>
            {
                iis.AuthenticationDisplayName = "Windows";
                iis.AutomaticAuthentication = false;
            }).Configure<StsAuthentificationConfiguration>(stsAuthConfig);

            var builder = services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
            }).AddAspNetIdentity<TUserIdentity>()
              .AddConfigurationStore(options =>
              {
                  options.ConfigureDbContext = b => b.UseNpgsql(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly));
              })
              .AddOperationalStore(options =>
              {
                  options.ConfigureDbContext = b => b.UseNpgsql(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly));
                  options.EnableTokenCleanup = true;
              });

            builder.AddCustomSigningCredential(configuration, logger);
            builder.AddCustomValidationKey(configuration, logger);
            builder.AddProfileService<SynkerProfileService>();

            var authConfig = configuration.GetSection(nameof(StsAuthentificationConfiguration)).Get<StsAuthentificationConfiguration>();
            var authenticationBuilder = services.AddAuthentication(options =>
            {
                options.DefaultScheme = "cookie";
            });

            authenticationBuilder.AddCookie("cookie");

            if (authConfig.Google != null)
            {
                authenticationBuilder.AddGoogle("Google", options =>
                   {
                       //options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                       options.ClientId = authConfig.Google.ClientId;
                       options.ClientSecret = authConfig.Google.Secret;

                       options.Scope.Add("profile");

                       //options.ClaimActions.Clear();
                       //options.ClaimActions.MapJsonKey(JwtClaimTypes.Subject, "id");
                       options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
                       options.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "given_Name");
                       options.ClaimActions.MapJsonKey(ClaimTypes.Surname, "family_Name");
                       options.ClaimActions.MapJsonKey(JwtClaimTypes.Picture, "picture");
                       options.ClaimActions.MapJsonKey(ClaimTypes.Locality, "locale");
                       options.ClaimActions.MapJsonKey(JwtClaimTypes.Profile, "urn:google:profile");
                       options.ClaimActions.MapCustomJson(JwtClaimTypes.Gender, jobject =>
                          jobject.TryGetValue("gender", StringComparison.InvariantCultureIgnoreCase, out JToken gender)
                               ? gender.Value<string>().Equals("male", StringComparison.InvariantCultureIgnoreCase) ? "Mr" : "Mrs"
                               : string.Empty
                           );

                       options.SaveTokens = true;
                   });
            }
        }

        public static void AddDbContexts<TContext>(this IServiceCollection services, IConfiguration configuration)
            where TContext : DbContext
        {
            var connectionString = configuration.GetConnectionString(ConfigurationConsts.AdminConnectionStringKey);
            services.AddDbContext<TContext>(options => options.UseNpgsql(connectionString));
        }

        public static void UseMvcLocalizationServices(this IApplicationBuilder app)
        {
            var options = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(options.Value);
        }

        public static void AddLogging(
            this IApplicationBuilder app,
            ILoggerFactory loggerFactory,
            IConfiguration configuration)
        {
            loggerFactory.AddSerilog();
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
        }
    }
}
