using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace UJStudentGorvenanceStudentWeb.Helper
{


    public class JwtMiddleware(RequestDelegate next)
    {
        public async Task Invoke(HttpContext context)
        {
            var token = context.Session.GetString("Token");
            if (!string.IsNullOrEmpty(token))
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                var claims = jwtToken.Claims.Select(claim => new Claim(claim.Type, claim.Value)).ToList();

                var identity = new ClaimsIdentity(claims, "jwt");
                var principal = new ClaimsPrincipal(identity);

                context.User = principal;
            }

            await next(context);
        }
    }
}