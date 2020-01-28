using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Serverless.Chat.Extensions;
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
        private readonly IServiceProvider _serviceProvider;

        public ChatFunctions()
        {
            AWSSDKHandler.RegisterXRayForAllServices();
            _serviceProvider = ChatDependencyContainerBuilder.Build();
        }

        public async Task<APIGatewayProxyResponse> GetRecentMessages(APIGatewayProxyRequest request)
        {
            return (await _serviceProvider.GetService<IMediator>().Send(new GetRecentMessagesQuery())).ApiResponse;
        }

        public async Task<APIGatewayProxyResponse> SignIn(APIGatewayProxyRequest request)
        {
            return (await _serviceProvider.GetService<IMediator>()
                .Send(new SignInCommand
                {
                    Request = request
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
            return (await _serviceProvider.GetService<IMediator>()
                    .Send(new SendMessageCommand
                    {
                        Request = request
                    }))
                .ApiResponse;
        }

        public async Task<APIGatewayProxyResponse> Connect(APIGatewayProxyRequest request)
        {
            if (!request.MultiValueHeaders.TryGetValue(Headers.SecurityWebsocketProtocol, out var token))
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
            await _serviceProvider.GetService<IMediator>()
                .Send(new SendWebSocketMessagesCommand
                {
                    DynamoEvent = streamEvent,
                    EventDataMapper = Message.FromMessageStreamRecord
                });
        }

        public async Task UserUpdated(DynamoDBEvent streamEvent)
        {
            await _serviceProvider.GetService<IMediator>()
                .Send(new SendWebSocketMessagesCommand
                {
                    DynamoEvent = streamEvent,
                    EventDataMapper = Message.FromUserStreamRecord
                });
        }
    }
}