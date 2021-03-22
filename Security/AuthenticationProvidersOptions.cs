namespace Microsoft.Extensions.DependencyInjection
{
    public class AuthenticationProvidersOptions
    {
        public FacebookAuthenticationOptions Facebook { get; set; } = default!;
        public AuthenticationProviderOptions Github { get; set; } = default!;
        public AuthenticationProviderOptions Google { get; set; } = default!;
        public AuthenticationProviderOptions Instagram { get; set; } = default!;
        public AuthenticationProviderOptions LinkedIn { get; set; } = default!;
        public AuthenticationProviderOptions Microsoft { get; set; } = default!;
        public TwitterAuthenticationOptions Twitter { get; set; } = default!;
    }
}
