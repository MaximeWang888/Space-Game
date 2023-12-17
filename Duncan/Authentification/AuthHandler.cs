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
        // Pour récupérer les données codées dans le header de la requête
        string authHeader = Request.Headers["Authorization"];
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Basic "))
        {
            // Gérer l'absence ou l'invalidité de l'en-tête Authorization
            return AuthenticateResult.Fail("Missing or invalid Authorization header");
        }

        string headerData = authHeader.Substring("Basic ".Length).Trim();
        // On décode les données
        string decodedData = Encoding.UTF8.GetString(Convert.FromBase64String(headerData));
        // Vu qu'elles sont de la forme username:password, on split
        string[] credentials = decodedData.Split(':', 2);

        // Et ensuite voilà un code de base pour déclarer un utilisateur admin
        if (credentials[0] == "admin" && credentials[1] == "password")
        {
            var claims = new[]
            {
            // On peut définir un nom et un rôle, ici c'est surtout le rôle qui nous intéresse
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

        return AuthenticateResult.Fail("Invalid credentials format");
    }
}