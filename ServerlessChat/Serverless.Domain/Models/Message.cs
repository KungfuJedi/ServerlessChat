using System;
using Amazon.DynamoDBv2.DataModel;
using Serverless.Domain.Constants;

namespace Serverless.Domain.Models
{
    [DynamoDBTable(DynamoDbTables.Messages)]
    public class Message
    {
        [DynamoDBHashKey]
        public string Id { get; }

        [DynamoDBRangeKey]
        public DateTime CreatedOnUtc { get; }

        [DynamoDBProperty]
        public DateTime ExpiresOnUtc => CreatedOnUtc.Add(TimeSpan.FromMinutes(15));

        [DynamoDBProperty]
        public string Content { get; }

        [DynamoDBProperty]
        public string AuthorName { get; }

        [DynamoDBIgnore]
        public bool IsSystemMessage => string.IsNullOrEmpty(AuthorName);

        private Message(string id, string content, string authorName)
        {
            Id = id;
            CreatedOnUtc = DateTime.UtcNow;
            Content = content;
            AuthorName = authorName;
        }

        public static Message Create(string content, string authorName)
        {
            return new Message(Guid.NewGuid().ToString(), content, authorName);
        }
    }
}