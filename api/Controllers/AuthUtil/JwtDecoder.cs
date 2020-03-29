using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace api.Controllers.AuthUtil
{
    public class JwtDecoder
    {
        private readonly byte[] _secret;

        public JwtDecoder(string secret)
        {
            _secret = Encoding.UTF8.GetBytes(secret);
        }
        
        public bool Verify(string token)
        {
            string[] jwtSegments = token.Split(".");
            if (jwtSegments.Length != 3) return false;
            string header = jwtSegments[0];
            string payload = jwtSegments[1];
            string signature = jwtSegments[2];
            var alg = JsonConvert.DeserializeObject<JwtHeader>(Encoding.UTF8.GetString(Base64UrlTextEncoder.Decode(header))).Algorithm;
            string sigCalc;
            switch (alg)
            {
                case "HS256":
                    sigCalc = WebEncoders.Base64UrlEncode(new HMACSHA256(_secret).ComputeHash(Encoding.UTF8.GetBytes($"{header}.{payload}")));
                    break;
                default:
                    throw new NotImplementedException($"Algorithm {alg} not implemented");
            }
            return signature == sigCalc;
        }

        public bool Verify(HttpRequest request)
        {
            var token = ExtractToken(request);
            if (token == null) return false;
            return Verify(token);
        }

        private static string ExtractToken(HttpRequest request)
        {
            if (request.Headers.TryGetValue("Authorization", out var headerValues))
            {
                if (headerValues.Count == 0) return null;
                var value = headerValues[0];
                if (!value.StartsWith("Bearer")) return null;
                return value.Trim();
            }
            return null;
        }

        public static JwtPayload ParsePayload(HttpRequest request)
            => ParsePayload(ExtractToken(request));
        
        private static JwtPayload ParsePayload(string token)
        {
            var jwtSegments = token.Split(".");
            if (jwtSegments.Length != 3) return null;
            var payload = jwtSegments[1];
            return JsonConvert.DeserializeObject<JwtPayload>(
                Encoding.UTF8.GetString(Base64UrlTextEncoder.Decode(payload)));
        }
    }
}