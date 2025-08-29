using System.Net.Http.Headers;

namespace FrontendProyecto.Infrastructure
{
    

    public class JwtSessionHandler : DelegatingHandler
      {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JwtSessionHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWT");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, ct);
        }
    }


}
