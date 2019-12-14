using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serverless.Chat.Extensions;
using Serverless.Chat.Requests;
using Serverless.Domain.Authentication;
using Serverless.Domain.AwsClients;
using JsonSerializer = Amazon.Lambda.Serialization.Json.JsonSerializer;

[assembly: LambdaSerializer(typeof(JsonSerializer))]

namespace Serverless.Chat
{
    public class ChatFunctions
    {
        public async Task<APIGatewayProxyResponse> GetRecentMessages(APIGatewayProxyRequest request)
        {
            var serviceProvider = ChatDependencyContainerBuilder.ForRecentMessages();
            var dynamoClient = serviceProvider.GetService<IDynamoDbClient>();
            var messages = await dynamoClient.GetRecentMessages();

            return new APIGatewayProxyResponse()
                .WithStatus(HttpStatusCode.OK)
                .WithBody(JsonConvert.SerializeObject(new {Messages = messages}));
        }

        public async Task<APIGatewayProxyResponse> SignIn(APIGatewayProxyRequest request)
        {
            var signInRequest = JsonConvert.DeserializeObject<SignInRequest>(request.Body);
            if (signInRequest == null)
                return new APIGatewayProxyResponse()
                    .WithStatus(HttpStatusCode.BadRequest);

            var serviceProvider = ChatDependencyContainerBuilder.ForSignIn();
            var dynamoClient = serviceProvider.GetService<IDynamoDbClient>();
            var user = await dynamoClient.SignIn(signInRequest.UserName);

            var jwtService = serviceProvider.GetService<IJwtService>();
            var token = jwtService.GenerateJwt(user);

            return new APIGatewayProxyResponse()
                .WithStatus(HttpStatusCode.OK)
                .WithBody(JsonConvert.SerializeObject(new
                {
                    AuthToken = token
                }));
        }
    }
}