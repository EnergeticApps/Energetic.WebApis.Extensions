using Energetic.WebApis;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    //TODO refactor this class. It's so ugly
    public static class SecurityServiceCollectionExtensions
    {
        public static AuthenticationBuilder AddJwtAuthentication(this IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            services.Configure<AccountsOptions>(configuration.GetSection(ConfigurationKeys.AccountsAppSettingsKey));
            services.Configure<AuthenticationProviderOptions>(configuration.GetSection(ConfigurationKeys.AuthenticationProvidersAppSettingsKey));

            var accountsOptions = serviceProvider.GetRequiredService<IOptions<AccountsOptions>>().Value;

            serviceProvider = services.BuildServiceProvider();

            return services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = accountsOptions.Jwts.Issuer,
                        ValidAudience = accountsOptions.Jwts.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(accountsOptions.Jwts.SecurityKey))
                    };
                });
        }

        public static IServiceCollection AddSecurityCertificates(this IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            services.Configure<SecurityCertificateOptions>(configuration.GetSection(ConfigurationKeys.SecurityCertificateAppSettingsKey));

            //TODO: Check if we need to inject the SecurityCertificateService
            return services; //.AddSingleton<SecurityCertificateService>();
        }

        public static IServiceCollection AddAndConfigureCors(this IServiceCollection services)
        {
            //TODO: Allow any registered client app to use any HTTP verb of this API. Refuse access to all others
            //TODO: This should come out of appSettings.json
            return services.AddCors(setupAction =>
            setupAction.AddDefaultPolicy(configurePolicy =>
            {
                //TODO: restrict the origin to only the clients of this API that are registered in the auth server.
                configurePolicy.AllowAnyOrigin();
                configurePolicy.AllowAnyMethod();
            }));
        }

        public static IIdentityServerBuilder AddIdentityServer4Authentication(this IServiceCollection services, IHostEnvironment environment, Type migrationsAssemblyMarkerType, string connectionString)
        {
            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)

            //TODO: This should come out of AppSettings.json. And it should be specified for each API version
            .AddIdentityServerAuthentication(options =>
            {
                // base-address of your identityserver
                options.Authority = "https://localhost:44386/";

                // name of the API resource
                options.ApiName = "CanLearnApi";
                options.ApiSecret = "pN1@Y$0Fhj5K";
            });

            var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            services.Configure<AuthenticationProvidersOptions>(
                configuration.GetSection(ConfigurationKeys.AuthenticationProvidersAppSettingsKey));

            string migrationsAssemblyName = migrationsAssemblyMarkerType.Assembly.GetName().Name ??
                throw new InvalidOperationException($"Could not ascertain the migrations assembly name based off the type {migrationsAssemblyMarkerType.FullName}");

            serviceProvider = services.BuildServiceProvider();
            var securityCertificateOptions = serviceProvider.GetRequiredService<IOptions<SecurityCertificateOptions>>().Value;
            var accountsOptions = serviceProvider.GetRequiredService<IOptions<AccountsOptions>>().Value;

            return services.AddIdentityServer(options =>
            {
                options.UserInteraction.LoginUrl = accountsOptions.LoginUrl;
                options.UserInteraction.LogoutUrl = accountsOptions.LogoutUrl;
            })

            .AddSigningCredential(LoadSecurityCertificate(securityCertificateOptions, environment))
            .AddConfigurationStore(
                options =>
                options.ConfigureDbContext = builder =>
                builder.UseSqlServer(connectionString, sqlOptions => sqlOptions.MigrationsAssembly(migrationsAssemblyName)))

            .AddOperationalStore(options =>
                options.ConfigureDbContext = builder =>
                builder.UseSqlServer(connectionString, sqlOptions => sqlOptions.MigrationsAssembly(migrationsAssemblyName)));
        }

        private static X509Certificate2 LoadSecurityCertificate(SecurityCertificateOptions options, IHostEnvironment environment)
        {
            // This blog post by Ben Cull explains how I created this certificate
            // https://benjii.me/2017/06/creating-self-signed-certificate-identity-server-azure/
            // I ran the command below in Package Manager Console
            // set OPENSSL_CONF = C:\Program Files\OpenSSL - Win64\bin\openssl.cfg
            // Then I ran this one
            // openssl req -x509 -newkey rsa:4096 -sha256 -nodes -keyout erpmi.key -out erpmi.crt -subj "/CN=erpmi.com" -days 3650
            // Then this one
            // openssl pkcs12 -export -out erpmi.pfx -inkey erpmi.key -in erpmi.crt -certfile erpmi.crt
            // When prompted, I created and verified this new password
            // i230^KhB^FbD
            // Should probably remove the password from the source code and hide it!
            // Here is some background information about TLS HTTPS and certs
            // https://www.smashingmagazine.com/2017/06/guide-switching-http-https/
            // http://www.steves-internet-guide.com/ssl-certificates-explained/
            // This code below about certificates should load the cert I just created into the application

            X509Certificate2? cert = TryLoadingCertFromRegistry(options);

            if (cert is null)
            {
                cert = TryLoadingCertFromFile(options, environment);

                if (cert is null)
                {
                    throw new IOException($"Could not find certificate {options.Thumbprint} in the registry or in a file in the local environment.");
                }
            }

            return cert;
        }

        private static X509Certificate2? TryLoadingCertFromFile(SecurityCertificateOptions certInfo, IHostEnvironment environment)
        {
            X509Certificate2? result = null;
            string path = Path.Combine(environment.ContentRootPath, certInfo.FileName);

            try
            {
                result = new X509Certificate2(path, certInfo.Password);
            }
            catch (CryptographicException e)
            {
                if (!e.Message.ToLowerInvariant().Contains("cannot find"))
                {
                    throw e;
                }
            }

            return result;
        }

        private static X509Certificate2? TryLoadingCertFromRegistry(SecurityCertificateOptions certInfo)
        {
            X509Certificate2? result = null;

            using (X509Store certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser))
            {
                certStore.Open(OpenFlags.ReadOnly);

                X509Certificate2Collection certCollection = certStore.Certificates.Find(
                    X509FindType.FindByThumbprint,
                    certInfo.Thumbprint,
                    false);

                // Get the first cert with the thumbprint
                if (certCollection.Count > 0)
                {
                    result = certCollection[0];
                }
            }

            return result;
        }
    }
}