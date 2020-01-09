using System;
using System.Collections.Generic;
using System.Linq;
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
        Task<bool> CheckUserExists(Guid userId);
        Task SaveMessage(string userName, string content);
        Task SaveConnectionId(string connectionId, Guid userId);
    }

    public class DynamoDbClient : IDynamoDbClient
    {
        public async Task<IReadOnlyList<Message>> GetRecentMessages()
        {
            using (var client = new AmazonDynamoDBClient())
            using (var context = CreateDynamoDbContext(client))
            {
                return await context
                    .ScanAsync<Message>(new List<ScanCondition>())
                    .GetRemainingAsync();
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

        public async Task<bool> CheckUserExists(Guid userId)
        {
            using (var client = new AmazonDynamoDBClient())
            using (var context = CreateDynamoDbContext(client))
                return await context.LoadAsync<User>(userId) != null;
        }

        public async Task SaveConnectionId(string connectionId, Guid userId)
        {
            using (var client = new AmazonDynamoDBClient())
            using (var context = CreateDynamoDbContext(client))
            {
                var user = await context.LoadAsync<User>(userId);
                if (user == null)
                    return;

                user.ConnectionId = connectionId;
                await context.SaveAsync(user);
            }
        }

        public async Task SaveMessage(string userName, string content)
        {
            using (var client = new AmazonDynamoDBClient())
            using (var context = CreateDynamoDbContext(client))
                await context.SaveAsync(new Message(content, userName));
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