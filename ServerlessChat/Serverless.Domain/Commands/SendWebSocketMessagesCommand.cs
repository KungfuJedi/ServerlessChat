using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.Lambda.DynamoDBEvents;
using LanguageExt;
using MediatR;
using Serverless.Domain.AwsClients;
using Serverless.Domain.Models;
using Unit = MediatR.Unit;

namespace Serverless.Domain.Commands
{
    public class SendWebSocketMessagesCommand : IRequest
    {
        public DynamoDBEvent DynamoEvent { get; set; }
        public Func<DynamoDBEvent.DynamodbStreamRecord, Option<Message>> EventDataMapper { get; set; }
    }

    public class SendWebSocketMessagesCommandHandler : IRequestHandler<SendWebSocketMessagesCommand, Unit>
    {
        private readonly IDynamoDbClient _dynamoDbClient;
        private readonly IApiGatewayClient _apiGatewayClient;

        public SendWebSocketMessagesCommandHandler(IDynamoDbClient dynamoDbClient, IApiGatewayClient apiGatewayClient)
        {
            _dynamoDbClient = dynamoDbClient;
            _apiGatewayClient = apiGatewayClient;
        }

        public async Task<Unit> Handle(SendWebSocketMessagesCommand command, CancellationToken cancellationToken)
        {
            var users = await _dynamoDbClient.GetUsers();
            foreach (var streamRecord in command.DynamoEvent.Records)
            {
                if (streamRecord.EventName != OperationType.INSERT)
                    return Unit.Value;

                await command.EventDataMapper(streamRecord)
                    .MatchAsync(async message =>
                        {
                            foreach (var user in users.ToList())
                                if (!await _apiGatewayClient.PostMessage(user.ConnectionId, message))
                                    await _dynamoDbClient.DeleteUser(user.Id);

                            return LanguageExt.Unit.Default;
                        },
                        () => Task.FromResult(LanguageExt.Unit.Default));
            }

            return Unit.Value;
        }
    }
}