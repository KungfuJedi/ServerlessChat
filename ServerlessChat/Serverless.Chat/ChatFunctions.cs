using System;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Amazon.Lambda.SQSEvents;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
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
            return (await _serviceProvider.GetService<IMediator>()
                    .Send(new ConnectCommand
                    {
                        Request = request
                    }))
                .ApiResponse;
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
            var mediator = _serviceProvider.GetService<IMediator>();
            await Task.WhenAll(
                mediator.Send(new SendUpdateUserConnectionListMessageCommand
                {
                    DynamoEvent = streamEvent
                }),
                mediator.Send(new SendWebSocketMessagesCommand
                {
                    DynamoEvent = streamEvent,
                    EventDataMapper = Message.FromUserStreamRecord
                })
            );
        }

        public async Task UpdateUserConnectionList(SQSEvent sqsEvent)
        {
            var mediator = _serviceProvider.GetService<IMediator>();
            await mediator.Send(new UpdateUserConnectionMappingsCommand
            {
                SqsEvent = sqsEvent
            });
        }
    }
}