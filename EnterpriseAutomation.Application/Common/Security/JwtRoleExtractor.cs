using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Application.Common.Security
{
    public static class JwtRoleExtractor
    {
        public static List<string> ExtractRoles(string jwtToken, string clientId = null)
        {
            if (string.IsNullOrWhiteSpace(jwtToken))
                return new List<string>();

            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwtToken);
            var roles = new List<string>();

            // Realm Roles → realm_access.roles
            var realmAccess = token.Claims.FirstOrDefault(c => c.Type == "realm_access");
            if (realmAccess != null)
            {
                var parsed = JObject.Parse(realmAccess.Value);
                var realmRoles = parsed["roles"]?.ToObject<List<string>>();
                if (realmRoles != null)
                    roles.AddRange(realmRoles);
            }

            // Client Roles → resource_access[clientId].roles
            if (!string.IsNullOrEmpty(clientId))
            {
                var resourceAccess = token.Claims.FirstOrDefault(c => c.Type == "resource_access");
                if (resourceAccess != null)
                {
                    var parsed = JObject.Parse(resourceAccess.Value);
                    var clientRoles = parsed[clientId]?["roles"]?.ToObject<List<string>>();
                    if (clientRoles != null)
                        roles.AddRange(clientRoles);
                }
            }

            return roles.Distinct().ToList();
        }
    }
}
