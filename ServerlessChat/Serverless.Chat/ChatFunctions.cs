using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serverless.Chat.Extensions;
using Serverless.Chat.Requests;
using Serverless.Domain.Authentication;
using Serverless.Domain.AwsClients;
using Serverless.Domain.Commands;
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
            var serviceProvider = ChatDependencyContainerBuilder.Build();
            var mediator = serviceProvider.GetService<IMediator>();
            return (await mediator.Send(new SignInCommand
            {
                RequestBody = request.Body
            })).ApiResponse;
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
            var serviceProvider = ChatDependencyContainerBuilder.Build();
            var mediator = serviceProvider.GetService<IMediator>();
            await mediator.Send(new SendWebSocketMessagesCommand
            {
                DynamoEvent = streamEvent,
                EventDataMapper = Message.FromMessageStreamRecord
            });
        }

        public async Task UserUpdated(DynamoDBEvent streamEvent)
        {
            var serviceProvider = ChatDependencyContainerBuilder.Build();
            var mediator = serviceProvider.GetService<IMediator>();
            await mediator.Send(new SendWebSocketMessagesCommand
            {
                DynamoEvent = streamEvent,
                EventDataMapper = Message.FromUserStreamRecord
            });
        }
    }
}