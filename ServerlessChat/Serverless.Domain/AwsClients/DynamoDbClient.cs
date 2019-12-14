using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Serverless.Domain.Models;

namespace Serverless.Domain.AwsClients
{
    public interface IDynamoDbClient
    {
        Task<IReadOnlyList<Message>> GetRecentMessages();
        Task<User> SignIn(string userName);
    }

    public class DynamoDbClient : IDynamoDbClient
    {
        public async Task<IReadOnlyList<Message>> GetRecentMessages()
        {
            using (var client = new AmazonDynamoDBClient())
            using (var context = CreateDynamoDbContext(client))
            {
                var scanOperation = context.ScanAsync<Message>(new List<ScanCondition>());
                return await scanOperation.GetRemainingAsync();
            }
        }

        public async Task<User> SignIn(string userName)
        {
            using (var client = new AmazonDynamoDBClient())
            using (var context = CreateDynamoDbContext(client))
            {
                var user = new User(userName);
                await context.SaveAsync(user);
                return user;
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