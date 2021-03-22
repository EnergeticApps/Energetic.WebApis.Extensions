namespace Microsoft.Extensions.DependencyInjection
{
    public class SecurityCertificateOptions
    {
        public string FileName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Thumbprint { get; set; } = string.Empty;
    }
}
