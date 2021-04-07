namespace Microsoft.Extensions.DependencyInjection
{
    public class SecurityOptions
    {
        public JwtOptions Jwt { get; set; } = default!;
    }
}