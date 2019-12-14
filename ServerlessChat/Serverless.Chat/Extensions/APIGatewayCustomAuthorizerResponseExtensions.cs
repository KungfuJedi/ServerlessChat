using System;
using System.Collections.Generic;
using System.Text;
using Amazon.Lambda.APIGatewayEvents;

namespace Serverless.Chat.Extensions
{
    public static class APIGatewayCustomAuthorizerResponseExtensions
    {
        public static APIGatewayCustomAuthorizerResponse WithPrincipal(this APIGatewayCustomAuthorizerResponse response,
            string principal)
        {
            response.PrincipalID = principal;
            return response;
        }

        public static APIGatewayCustomAuthorizerResponse WithPolicyAllowingArn(
            this APIGatewayCustomAuthorizerResponse response, string arn)
        {
            response.PolicyDocument = new APIGatewayCustomAuthorizerPolicy
            {
                Version = "2012-10-17",
                Statement = new List<APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement>
                {
                    new APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement
                    {
                        Action = new HashSet<string> {"execute-api:Invoke"},
                        Effect = "Allow",
                        Resource = new HashSet<string> {arn}
                    }
                }
            };
            return response;
        }
    }
}
