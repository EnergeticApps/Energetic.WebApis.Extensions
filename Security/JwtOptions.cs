using System;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public class JwtOptions
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string Authority { get; set; } = string.Empty;
    }
}