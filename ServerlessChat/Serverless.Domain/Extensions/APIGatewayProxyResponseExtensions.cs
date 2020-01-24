using System.Collections.Generic;
using System.Net;
using Amazon.Lambda.APIGatewayEvents;

namespace Serverless.Chat.Extensions
{
    public static class APIGatewayProxyResponseExtensions
    {
        public static APIGatewayProxyResponse WithStatus(this APIGatewayProxyResponse response, HttpStatusCode statusCode)
        {
            response.StatusCode = (int) statusCode;
            return response;
        }

        public static APIGatewayProxyResponse WithBody(this APIGatewayProxyResponse response, string body)
        {
            response.Body = body;
            return response;
        }

        public static APIGatewayProxyResponse WithCorsHeaders(this APIGatewayProxyResponse response)
        {
            if (response.Headers == null)
                response.Headers = new Dictionary<string, string>();

            response.Headers.Add("Access-Control-Allow-Origin", "*");
            return response;
        }
    }
}