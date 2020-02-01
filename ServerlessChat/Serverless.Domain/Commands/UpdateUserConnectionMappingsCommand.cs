using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.SQSEvents;
using MediatR;
using Newtonsoft.Json;
using Serverless.Domain.AwsClients;
using Serverless.Domain.Models;

namespace Serverless.Domain.Commands
{
    public class UpdateUserConnectionMappingsCommand : IRequest
    {
        public SQSEvent SqsEvent { get; set; }
    }

    public class UpdateUserConnectionMappingsCommandHandler : IRequestHandler<UpdateUserConnectionMappingsCommand>
    {
        private readonly IDynamoDbClient _dynamoDbClient;

        public UpdateUserConnectionMappingsCommandHandler(IDynamoDbClient dynamoDbClient)
        {
            _dynamoDbClient = dynamoDbClient;
        }

        public async Task<Unit> Handle(UpdateUserConnectionMappingsCommand request, CancellationToken cancellationToken)
        {
            var mappings = await _dynamoDbClient.GetUserConnectionMappings();
            var messages = request.SqsEvent.Records.Select(record => JsonConvert.DeserializeObject<UpdateUserConnectionMappingQueueMessage>(record.Body));
            foreach (var message in messages)
                if (message.IsDeletion)
                    mappings.RemoveMapping(message.UserConnectionMapping);
                else
                    mappings.AddMapping(message.UserConnectionMapping);

            await _dynamoDbClient.SaveUserConnectionMappings(mappings);

            return Unit.Value;
        }
    }
}