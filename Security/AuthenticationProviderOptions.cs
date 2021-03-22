namespace Microsoft.Extensions.DependencyInjection
{
    public class AuthenticationProviderOptions
    {
        public bool IsActive { get; set; }
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
    }
}
