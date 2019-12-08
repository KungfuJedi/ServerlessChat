using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Polly;
using Serverless.Domain.Models;

namespace Serverless.Domain.AwsClients
{
    public interface IDynamoDbClient
    {
        Task<IReadOnlyList<Message>> GetRecentMessages(CancellationToken cancellationToken);
    }

    public class DynamoDbClient : IDynamoDbClient
    {
        public async Task<IReadOnlyList<Message>> GetRecentMessages(CancellationToken cancellationToken)
        {
            using (var client = new AmazonDynamoDBClient())
            using (var context = new DynamoDBContext(client))
            {
                var scanOperation = context.ScanAsync<Message>(new List<ScanCondition>(), new DynamoDBOperationConfig
                {
                    ConsistentRead = false
                });

                return (await Policy.Handle<ProvisionedThroughputExceededException>()
                    .WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(1))
                    .ExecuteAndCaptureAsync(() => scanOperation.GetRemainingAsync(cancellationToken)))
                    // Result on Polly, not Task
                    .Result;
            }
        }
    }
}