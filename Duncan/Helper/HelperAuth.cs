

using System.Text;

namespace Duncan.Helper
{
    public class HelperAuth
    {
        public static bool isAdmin(HttpRequest request)
        {
            var authorizationBase64 = request.Headers.Authorization.ToString().Replace("Basic ", "");
            var authorizationByte = Convert.FromBase64String(authorizationBase64);
            var authorization = Encoding.UTF8.GetString(authorizationByte);
            return authorization is "admin:password";
        }
        public static bool isFakeRemoteUser(HttpRequest request)
        {
            var authorizationBase64 = request.Headers.Authorization.ToString().Replace("Basic ", "");
            var authorizationByte = Convert.FromBase64String(authorizationBase64);
            var authorization = Encoding.UTF8.GetString(authorizationByte);
            return authorization is "shard-fake-remote:caramba";
        }
}
}