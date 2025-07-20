using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Application.Common.Security
{
    public static class JwtRoleExtractor
    {
        public static List<string> ExtractRoles(string jwtToken, string clientId = null, List<string> targetRoles = null)
        {
            if (string.IsNullOrWhiteSpace(jwtToken))
                return new List<string>();

            try
            {
                var parts = jwtToken.Split('.');
                if (parts.Length != 3)
                    return new List<string>();

                var payload = parts[1];
                var jsonBytes = Convert.FromBase64String(PadBase64(payload));
                var json = System.Text.Encoding.UTF8.GetString(jsonBytes);
                var jObj = JObject.Parse(json);

                var roles = new List<string>();

                // Realm Roles
                var realmRoles = jObj["realm_access"]?["roles"]?.ToObject<List<string>>();
                if (realmRoles != null)
                    roles.AddRange(realmRoles);

                // Client Roles
                if (!string.IsNullOrEmpty(clientId))
                {
                    var clientRoles = jObj["resource_access"]?[clientId]?["roles"]?.ToObject<List<string>>();
                    if (clientRoles != null)
                        roles.AddRange(clientRoles);
                }

                // اگر لیستی از نقش‌های هدف داده شده، فقط همون‌ها رو برگردون
                if (targetRoles != null && targetRoles.Any())
                    return roles.Intersect(targetRoles).Distinct().ToList();

                // در غیر این‌صورت همه نقش‌ها
                return roles.Distinct().ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        private static string PadBase64(string base64)
        {
            int mod4 = base64.Length % 4;
            if (mod4 == 2) return base64 + "==";
            if (mod4 == 3) return base64 + "=";
            if (mod4 == 1) throw new FormatException("Invalid Base64Url string.");
            return base64;
        }
    }
}
