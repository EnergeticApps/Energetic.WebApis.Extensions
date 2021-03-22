namespace Microsoft.Extensions.DependencyInjection
{
    public class TwitterAuthenticationOptions
    {
        public bool IsActive { get; set; }
        public string ConsumerApiKey { get; set; } = string.Empty;
        public string ConsumerSecret { get; set; } = string.Empty;
    }
}
