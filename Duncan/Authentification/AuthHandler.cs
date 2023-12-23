using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Duncan.Authentification;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;


public class AuthHandler : AuthenticationHandler<AuthOptions>
{
    public AuthHandler(
     IOptionsMonitor<AuthOptions> options,
     ILoggerFactory logger,
     UrlEncoder encoder,
     ISystemClock clock) : base(options, logger, encoder, clock)
    {
    }

    protected async override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string authHeader = Request.Headers["Authorization"];
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.Fail("Missing or invalid Authorization header");
        }

        string headerData = authHeader.Substring("Basic ".Length).Trim();
        string decodedData = Encoding.UTF8.GetString(Convert.FromBase64String(headerData));
        string[] credentials = decodedData.Split(':', 2);

        if (credentials[0] == "admin" && credentials[1] == "password")
        {
            var claims = new[]
            {
            new Claim(ClaimTypes.Name, "admin"),
            new Claim(ClaimTypes.Role, "admin")
        };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        } 
        else
        {
                var claims = new[]
                {
                 new Claim(ClaimTypes.Name, "shardUser"),
                   new Claim(ClaimTypes.Role, "shard")
             };

                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return AuthenticateResult.Success(ticket);
        }
    }
}