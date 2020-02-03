using System.Collections.Generic;
using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json;

namespace Serverless.Domain.Extensions
{
    public static class APIGatewayProxyResponseExtensions
    {
        public static APIGatewayProxyResponse WithStatus(this APIGatewayProxyResponse response, HttpStatusCode statusCode)
        {
            response.StatusCode = (int) statusCode;
            return response;
        }

        public static APIGatewayProxyResponse WithJsonBody<T>(this APIGatewayProxyResponse response, T bodyObject)
        {
            response.Body = JsonConvert.SerializeObject(bodyObject);
            return response;
        }

        public static APIGatewayProxyResponse WithEmptyJsonBody(this APIGatewayProxyResponse response)
        {
            response.Body = JsonConvert.SerializeObject(new object());
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