using Microsoft.AspNetCore.Authentication.Cookies;

namespace Microsoft.Extensions.DependencyInjection
{
    public class AccountsOptions
    {
        public string LoginUrl { get; set; } = string.Empty;
        public string LogoutUrl { get; set; } = string.Empty;
        public bool AllowLocalLogin { get; set; }
        public bool AllowRememberLogin { get; set; }
        public int RememberMeDurationDays { get; set; }
        public bool ShowLogoutPrompt { get; set; }
        public bool AutomaticRedirectAfterSignOut { get; set; }
        public string WindowsAuthenticationSchemeName { get; set; } = "Windows";
        // if user uses windows auth, should we load the Groups from windows
        public bool IncludeWindowsGroups = false;
        public string InvalidCredentialsErrorMessage { get; set; } = "Invalid credentials";
        public string InvalidReturnUrlErrorMessage { get; set; } = "Invalid return URL";
        public string AccessDeniedPath { get; set; } = "/";
        public CookieAuthenticationOptions Cookie { get; set; } = default!;
        public JwtOptions Jwts { get; set; } = new JwtOptions();
    }
}
