using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Ben.Demystifier
{
    public class DemystifierMiddleware
    {
        private readonly RequestDelegate _next;
        public DemystifierMiddleware(
            RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                throw ex.Demystify();
            }
        }
    }
}