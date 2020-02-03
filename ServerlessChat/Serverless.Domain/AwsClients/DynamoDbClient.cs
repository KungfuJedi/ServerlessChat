using System;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using LanguageExt;
using Serverless.Domain.Models;

namespace Serverless.Domain.AwsClients
{
    public interface IDynamoDbClient
    {
        Task SaveUser(User user);
        Task<bool> CheckUserExists(Guid userId);
        Task SaveMessage(string userName, string content);
        Task SaveConnectionId(string connectionId, Guid userId);
        Task DeleteUser(Guid userId);
        Task<UserConnectionMappings> GetUserConnectionMappings();
        Task SaveUserConnectionMappings(UserConnectionMappings mappings);
    }

    public class DynamoDbClient : IDynamoDbClient
    {
        public async Task SaveUser(User user)
        {
            await SaveAsync(user);
        }

        public async Task<bool> CheckUserExists(Guid userId)
        {
            return await TryLoadAsync<User>(userId)
                .MatchAsync(user => Task.FromResult(true),
                    () => Task.FromResult(false));
        }

        public async Task SaveConnectionId(string connectionId, Guid userId)
        {
            await TryLoadAsync<User>(userId)
                .IfSomeAsync(async user =>
                {
                    user.ConnectionId = connectionId;
                    await SaveAsync(user);
                });
        }

        public async Task SaveMessage(string userName, string content)
        {
            await SaveAsync(new Message(content, userName));
        }

        public async Task DeleteUser(Guid userId)
        {
            await DeleteAsync<User>(userId);
        }

        public async Task<UserConnectionMappings> GetUserConnectionMappings()
        {
            return (await TryLoadAsync<UserConnectionMappings>(UserConnectionMappings.IdValue))
                .Match(mapping => mapping,
                    () => new UserConnectionMappings());
        }

        public async Task SaveUserConnectionMappings(UserConnectionMappings mappings)
        {
            await SaveAsync(mappings);
        }

        private static DynamoDBContext CreateDynamoDbContext(IAmazonDynamoDB client)
        {
            return new DynamoDBContext(client, new DynamoDBContextConfig
            {
                ConsistentRead = false,
                TableNamePrefix = $"{Environment.GetEnvironmentVariable("environment")}-"
            });
        }

        private static async Task<Option<T>> TryLoadAsync<T>(object key) where T : new()
        {
            using (var client = new AmazonDynamoDBClient())
            using (var context = CreateDynamoDbContext(client))
                try
                {
                    return await context.LoadAsync<T>(key);
                }
                catch
                {
                    return Option<T>.None;
                }
        }

        private static async Task SaveAsync<T>(T entity)
        {
            using (var client = new AmazonDynamoDBClient())
            using (var context = CreateDynamoDbContext(client))
                await context.SaveAsync(entity);

        }

        private static async Task DeleteAsync<T>(object id)
        {
            using (var client = new AmazonDynamoDBClient())
            using (var context = CreateDynamoDbContext(client))
                await context.DeleteAsync<T>(id);
        }
    }
}