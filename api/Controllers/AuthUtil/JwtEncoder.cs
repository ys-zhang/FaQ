using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;
using api.Models;
using Newtonsoft.Json;

namespace api.Controllers.AuthUtil
{
    
    
    /// <summary>
    /// Class for generating jwt for authentication.
    /// see https://jwt.io/introduction/
    /// </summary>
    public class JwtEncoder
    {
        private readonly string _secret;
        private readonly JwtHeader _header;
        private readonly int _expiration;

        public static readonly string SUBJECT = "FaQApi";

        public JwtEncoder(string secret, int expiration) : this(secret, new JwtHeader(), expiration) { }

        public JwtEncoder(string secret, JwtHeader header, int expiration)
        {
            _secret = secret;
            _header = header;
            _expiration = expiration;
        }
        
        private static string Base64Encode(string text)
        {
            if (text == null) return null;
            var bytes = Encoding.UTF8.GetBytes(text);
            return WebEncoders.Base64UrlEncode(bytes);
        }
        private string EncodeHeader(JwtHeader header) => Base64Encode(JsonConvert.SerializeObject(header));
        private string EncodePayload(JwtPayload payload) => Base64Encode(JsonConvert.SerializeObject(payload));

        public string CreateToken(AdminUser user)
        {
            var payload = EncodePayload(new JwtPayload
            {
                Issuer = "FaQApi",
                Expiration = DateTime.Now.AddSeconds(_expiration),
                Subject = SUBJECT,
                User = user.Username,
                Roles = user.Roles
            });
            var header = EncodeHeader(_header);
            string signature;
            switch (_header.Algorithm)
            {
                case "HS256":
                    using (var alg = new HMACSHA256(Encoding.UTF8.GetBytes(_secret)))
                    {
                        signature = WebEncoders.Base64UrlEncode(alg.ComputeHash(Encoding.UTF8.GetBytes($"{header}.{payload}")));
                    }
                    break;
                default:
                    throw new NotImplementedException($"{_header.Algorithm} not implemented");
            }
            return $"{header}.{payload}.{signature}";
        }
    }

    public class JwtHeader
    {
        [JsonProperty("alg")] public string Algorithm { get; set; } = "HS256";
        [JsonProperty("typ")] public string Type { get; set; } = "JWT";
    }

    public class JwtPayload
    {
        [JsonProperty("iss")]
        public string Issuer { get; set; }
        [JsonProperty("exp")]
        public DateTime Expiration { get; set; }
        [JsonProperty("sub")]
        public string Subject { get; set; }
        [JsonProperty("user")]
        public string User { get; set; }
        public List<AdminUserRole> Roles { get; set; }
    }
}