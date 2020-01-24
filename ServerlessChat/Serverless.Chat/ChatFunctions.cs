using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serverless.Chat.Extensions;
using Serverless.Chat.Requests;
using Serverless.Domain.Authentication;
using Serverless.Domain.AwsClients;
using Serverless.Domain.Models;
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
                .WithBody(JsonConvert.SerializeObject(new {Messages = messages}))
                .WithCorsHeaders();
        }

        public async Task<APIGatewayProxyResponse> SignIn(APIGatewayProxyRequest request)
        {
            var signInRequest = JsonConvert.DeserializeObject<SignInRequest>(request.Body);
            if (signInRequest == null)
                return new APIGatewayProxyResponse()
                    .WithStatus(HttpStatusCode.BadRequest)
                    .WithCorsHeaders();

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
                }))
                .WithCorsHeaders();
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
                    .WithStatus(HttpStatusCode.BadRequest)
                    .WithCorsHeaders();

            var serviceProvider = ChatDependencyContainerBuilder.ForSendMessage();
            var jwtService = serviceProvider.GetService<IJwtService>();
            var userName = jwtService.GetClaim(request.Headers["Authorization"], Claims.UserName);
            var dynamoClient = serviceProvider.GetService<IDynamoDbClient>();
            await dynamoClient.SaveMessage(userName, sendMessageRequest.Content);

            return new APIGatewayProxyResponse()
                .WithStatus(HttpStatusCode.OK)
                .WithCorsHeaders();
        }

        public async Task<APIGatewayProxyResponse> Connect(APIGatewayProxyRequest request)
        {
            if (!request.MultiValueHeaders.TryGetValue("Sec-WebSocket-Protocol", out var token))
                return new APIGatewayProxyResponse()
                    .WithStatus(HttpStatusCode.Unauthorized);

            var serviceProvider = ChatDependencyContainerBuilder.ForConnect();

            var connectionId = request.RequestContext.ConnectionId;

            var jwtService = serviceProvider.GetService<IJwtService>();
            var userId = jwtService.VerifyJwt(token.First());
            if (!userId.HasValue)
                return new APIGatewayProxyResponse()
                    .WithStatus(HttpStatusCode.Unauthorized);

            var dynamoClient = serviceProvider.GetService<IDynamoDbClient>();
            await dynamoClient.SaveConnectionId(connectionId, userId.Value);

            return new APIGatewayProxyResponse()
                .WithStatus(HttpStatusCode.OK);
        }

        public async Task MessageUpdated(DynamoDBEvent streamEvent)
        {
            var serviceProvider = ChatDependencyContainerBuilder.ForMessageUpdated();
            var dynamoClient = serviceProvider.GetService<IDynamoDbClient>();
            var users = await dynamoClient.GetUsers();
            foreach (var streamRecord in streamEvent.Records)
            {
                if (streamRecord.EventName != OperationType.INSERT)
                    return;

                var message = Message.NewMessageFromStreamRecord(streamRecord.Dynamodb.NewImage);
                var apiClient = serviceProvider.GetService<IApiGatewayClient>();
                foreach (var user in users.ToList())
                {
                    if (!await apiClient.PostMessage(user.ConnectionId, message))
                        await dynamoClient.DeleteUser(user.Id);
                }
                    
            }
        }

        public async Task UserUpdated(DynamoDBEvent streamEvent)
        {
            var serviceProvider = ChatDependencyContainerBuilder.ForUserUpdated();
            var dynamoClient = serviceProvider.GetService<IDynamoDbClient>();
            var users = await dynamoClient.GetUsers();
            foreach (var streamRecord in streamEvent.Records)
            {
                Message message;
                if (streamRecord.EventName == OperationType.INSERT)
                    message = Message.UserHasJoinedFromStreamRecord(streamRecord.Dynamodb.NewImage);
                else if (streamRecord.EventName == OperationType.REMOVE)
                    message = Message.UserHasLeftFromStreamRecord(streamRecord.Dynamodb.OldImage);
                else
                    return;

                var apiClient = serviceProvider.GetService<IApiGatewayClient>();
                foreach (var user in users.ToList())
                {
                    if (!await apiClient.PostMessage(user.ConnectionId, message))
                        await dynamoClient.DeleteUser(user.Id);
                }
            }
        }
    }
}