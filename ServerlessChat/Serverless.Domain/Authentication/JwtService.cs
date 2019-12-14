using System.Collections.Generic;
using System.Linq;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using Serverless.Domain.Models;

namespace Serverless.Domain.Authentication
{
    public interface IJwtService
    {
        string GenerateJwt(User user);
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

        private static string GetSecret()
        {
            // Don't actually do this 👍
            return string.Join("", Enumerable.Range(0, 30).Select(i => i.ToString()));
        }
    }
}