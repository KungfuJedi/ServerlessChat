using System;
using System.Collections.Generic;
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
        Task<IReadOnlyList<Message>> GetRecentMessages();
    }

    public class DynamoDbClient : IDynamoDbClient
    {
        public async Task<IReadOnlyList<Message>> GetRecentMessages()
        {
            using (var client = new AmazonDynamoDBClient())
            using (var context = CreateDynamoDbContext(client))
            {
                var scanOperation = context.ScanAsync<Message>(new List<ScanCondition>());

                return (await Policy.Handle<ProvisionedThroughputExceededException>()
                    .WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(1))
                    .ExecuteAndCaptureAsync(() => scanOperation.GetRemainingAsync()))
                    // Result on Polly, not Task
                    .Result;
            }
        }

        private static DynamoDBContext CreateDynamoDbContext(AmazonDynamoDBClient client)
        {
            return new DynamoDBContext(client, new DynamoDBContextConfig()
            {
                ConsistentRead = false,
                TableNamePrefix = $"{Environment.GetEnvironmentVariable("environment")}-"
            });
        }
    }
}