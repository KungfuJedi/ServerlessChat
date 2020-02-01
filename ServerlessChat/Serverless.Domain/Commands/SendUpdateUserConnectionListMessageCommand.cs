using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.DynamoDBEvents;
using LanguageExt;
using MediatR;
using Serverless.Domain.AwsClients;
using Serverless.Domain.Models;
using Unit = MediatR.Unit;

namespace Serverless.Domain.Commands
{
    public class SendUpdateUserConnectionListMessageCommand : IRequest
    {
        public DynamoDBEvent DynamoEvent { get; set; }
    }

    public class SendUpdateUserConnectionListMessageCommandHandler : IRequestHandler<SendUpdateUserConnectionListMessageCommand>
    {
        private readonly ISqsClient _sqsClient;

        private readonly IEnumerable<OperationType> _validOperationTypes =
            new List<OperationType> {OperationType.MODIFY, OperationType.REMOVE};

        public SendUpdateUserConnectionListMessageCommandHandler(ISqsClient sqsClient)
        {
            _sqsClient = sqsClient;
        }

        public async Task<Unit> Handle(SendUpdateUserConnectionListMessageCommand messageCommand, CancellationToken cancellationToken)
        {
            foreach (var record in messageCommand.DynamoEvent.Records.Where(r => _validOperationTypes.Contains(r.EventName)))
            {
                var recordData = record.EventName == OperationType.MODIFY
                    ? record.Dynamodb.NewImage
                    : record.Dynamodb.OldImage;

                await MapFromStreamRecord(recordData)
                    .Map(mapping => new UpdateUserConnectionMappingQueueMessage
                    {
                        IsDeletion = record.EventName == OperationType.REMOVE,
                        UserConnectionMapping = mapping
                    })
                    .IfSomeAsync(async message => await _sqsClient.SendMessage(message));
            }

            return Unit.Value;
        }

        private Option<UserConnectionMapping> MapFromStreamRecord(IDictionary<string, AttributeValue> record)
        {
            if (!record.TryGetValue(nameof(User.UserId), out var userIdString) || !Guid.TryParse(userIdString.S, out var userId))
                return Option<UserConnectionMapping>.None;

            if (!record.TryGetValue(nameof(User.ConnectionId), out var connectionId))
                return Option<UserConnectionMapping>.None;

            return Option<UserConnectionMapping>.Some(new UserConnectionMapping
            {
                UserId = userId,
                ConnectionId = connectionId.S
            });
        }
    }
}