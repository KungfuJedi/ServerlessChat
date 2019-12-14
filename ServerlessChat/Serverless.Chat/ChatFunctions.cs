using System;
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

        public async Task<APIGatewayCustomAuthorizerResponse> Authorize(APIGatewayCustomAuthorizerRequest request)
        {
            var token = request.AuthorizationToken;
            if (string.IsNullOrEmpty(token))
                throw new UnauthorizedAccessException();

            var serviceProvider = ChatDependencyContainerBuilder.ForAuthorizer();
            var jwtService = serviceProvider.GetService<IJwtService>();
            var userId = jwtService.VerifyJwt(token);

            if (!userId.HasValue)
                throw new UnauthorizedAccessException();

            var dynamoClient = serviceProvider.GetService<IDynamoDbClient>();
            if (!await dynamoClient.CheckUserExists(userId.Value))
                throw new UnauthorizedAccessException();

            return new APIGatewayCustomAuthorizerResponse()
                .WithPrincipal(userId.ToString())
                .WithPolicyAllowingArn(request.MethodArn);
        }

        public async Task<APIGatewayProxyResponse> SendMessage(APIGatewayProxyRequest request)
        {
            var sendMessageRequest = JsonConvert.DeserializeObject<SendMessageRequest>(request.Body);
            if (sendMessageRequest == null || string.IsNullOrEmpty(sendMessageRequest.Content))
                return new APIGatewayProxyResponse()
                    .WithStatus(HttpStatusCode.BadRequest);

            var serviceProvider = ChatDependencyContainerBuilder.ForSignIn();
            var jwtService = serviceProvider.GetService<IJwtService>();
            var userName = jwtService.GetClaim(request.Headers["Authorization"], Claims.UserName);
            var dynamoClient = serviceProvider.GetService<IDynamoDbClient>();
            await dynamoClient.SaveMessage(userName, sendMessageRequest.Content);

            return new APIGatewayProxyResponse()
                .WithStatus(HttpStatusCode.OK);
        }
    }
}