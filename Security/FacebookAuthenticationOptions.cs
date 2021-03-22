namespace Microsoft.Extensions.DependencyInjection
{
    public class FacebookAuthenticationOptions
    {
        public bool IsActive { get; set; }
        public string AppId { get; set; } = string.Empty;
        public string AppSecret { get; set; } = string.Empty;
    }
}
