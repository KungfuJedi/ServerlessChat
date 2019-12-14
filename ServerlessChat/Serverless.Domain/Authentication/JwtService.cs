using System;
using System.Collections.Generic;
using System.Linq;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using Newtonsoft.Json;
using Serverless.Domain.Models;

namespace Serverless.Domain.Authentication
{
    public interface IJwtService
    {
        string GenerateJwt(User user);
        Guid? VerifyJwt(string token);
        string GetClaim(string token, string claimName);
    }

    public class JwtService : IJwtService
    {
        public string GenerateJwt(User user)
        {
            var payload = new Dictionary<string, object>
            {
                { Claims.UserId, user.Id },
                { Claims.UserName, user.UserName }
            };

            IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
            IJsonSerializer serializer = new JsonNetSerializer();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);

            return encoder.Encode(payload, GetSecret());
        }

        public Guid? VerifyJwt(string token)
        {
            try
            {
                return DecodeJwt(token).TryGetValue(Claims.UserId, out var userIdString) &&
                       Guid.TryParse(userIdString, out var userId)
                    ? userId
                    : (Guid?) null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public string GetClaim(string token, string claimName)
        {
            return DecodeJwt(token).TryGetValue(claimName, out var claimValue)
                ? claimValue
                : null;
        }

        private static Dictionary<string, string> DecodeJwt(string token)
        {
            try
            {
                IJsonSerializer serializer = new JsonNetSerializer();
                IDateTimeProvider provider = new UtcDateTimeProvider();
                IJwtValidator validator = new JwtValidator(serializer, provider);
                IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
                IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder);

                var json = decoder.Decode(token, GetSecret(), verify: true);
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            }
            catch (Exception)
            {
                return new Dictionary<string, string>();
            }
        }

        private static string GetSecret()
        {
            // Don't actually do this 👍
            return string.Join("", Enumerable.Range(0, 30).Select(i => i.ToString()));
        }
    }
}