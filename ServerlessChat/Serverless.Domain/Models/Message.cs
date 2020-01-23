using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Serverless.Domain.Constants;

namespace Serverless.Domain.Models
{
    [DynamoDBTable(DynamoDbTables.Messages)]
    public class Message
    {
        [DynamoDBHashKey]
        [DynamoDBProperty(AttributeName = "MessageId")]
        public Guid Id { get; set; }

        [DynamoDBProperty(storeAsEpoch:true)]
        public DateTime ExpiresOnUtc { get; set; }

        [DynamoDBProperty]
        public string Content { get; set; }

        [DynamoDBProperty]
        public string AuthorName { get; set; }

        [DynamoDBIgnore]
        public bool IsSystemMessage => string.IsNullOrEmpty(AuthorName);

        public Message()
        {
            
        }

        public Message(string content, string authorName)
        {
            Id = Guid.NewGuid();
            Content = content;
            AuthorName = authorName;
            ExpiresOnUtc = DateTime.UtcNow.AddMinutes(15);
        }

        public static Message FromStreamRecord(Dictionary<string, AttributeValue> streamRecord)
        {
            var message = new Message();
            if (streamRecord.TryGetValue(nameof(Content), out var content))
                message.Content = content.S;

            if (streamRecord.TryGetValue(nameof(AuthorName), out var authorName))
                message.AuthorName = authorName.S;

            return message;
        }
    }
}