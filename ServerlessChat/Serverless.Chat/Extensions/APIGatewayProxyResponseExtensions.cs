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
    }
}