using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Energetic.WebApis;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AspNetCoreIdentityServiceCollectionExtensions
    {
        public static IServiceCollection AddAspNetCoreIdentity
            <TDbContext, TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken, TUserRepository, TRoleRepository, TUserClaimsPrincipalFactory>
            (this IServiceCollection services)
            where TUser : IdentityUser<TKey>
            where TRole : IdentityRole<TKey>
            where TUserClaimsPrincipalFactory : class
            where TUserClaim : IdentityUserClaim<TKey>
            where TUserRole : IdentityUserRole<TKey>
            where TUserLogin : IdentityUserLogin<TKey>
            where TRoleClaim : IdentityRoleClaim<TKey>
            where TUserToken : IdentityUserToken<TKey>
            where TUserRepository : class, IUserStore<TUser>
            where TRoleRepository : class, IRoleStore<TRole>
            where TKey : IEquatable<TKey>
            where TDbContext : IdentityDbContext<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken>
        {
            var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            services.Configure<AccountsOptions>(configuration.GetSection(ConfigurationKeys.AccountsAppSettingsKey));

            serviceProvider = services.BuildServiceProvider();
            var accountsOptions = serviceProvider.GetRequiredService<IOptions<AccountsOptions>>().Value;

            services
            .AddIdentity<TUser, TRole>()
            .AddSignInManager<SignInManager<TUser>>()
            .AddUserManager<UserManager<TUser>>()
            .AddRoleManager<RoleManager<TRole>>()
            .AddClaimsPrincipalFactory<TUserClaimsPrincipalFactory>()
            .AddUserStore<TUserRepository>()
            .AddRoleStore<TRoleRepository>()
            .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = new PathString(accountsOptions.AccessDeniedPath);
                options.LoginPath = accountsOptions.LoginUrl; //TODO: How to make API versioning work?
                options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
                //options.Cookie.Name = accountsOptions.Cookie.CookieName;
                //options.Cookie.HttpOnly = accountsOptions.Cookie.CookieHttpOnly;
                options.SlidingExpiration = accountsOptions.Cookie.SlidingExpiration;
                options.ExpireTimeSpan = accountsOptions.Cookie.ExpireTimeSpan;
            });

            return services;
        }
    }
}
