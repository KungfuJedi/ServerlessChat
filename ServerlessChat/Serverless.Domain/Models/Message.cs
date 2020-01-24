using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.DynamoDBEvents;
using LanguageExt;
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

        public static Option<Message> FromMessageStreamRecord(DynamoDBEvent.DynamodbStreamRecord streamRecord)
        {
            Message MapNewMessage(Dictionary<string, AttributeValue> attributes)
            {
                var message = new Message();
                if (attributes.TryGetValue(nameof(Content), out var content))
                    message.Content = content.S;

                if (attributes.TryGetValue(nameof(AuthorName), out var authorName))
                    message.AuthorName = authorName.S;

                return message;
            }

            if (streamRecord.EventName == OperationType.INSERT)
                return MapNewMessage(streamRecord.Dynamodb.NewImage);

            return Option<Message>.None;
        }

        public static Option<Message> FromUserStreamRecord(DynamoDBEvent.DynamodbStreamRecord streamRecord)
        {
            Message MapNewUserMessage(Dictionary<string, AttributeValue> attributes)
            {
                var message = new Message();

                if (attributes.TryGetValue(nameof(User.UserName), out var userName))
                    message.Content = $"{userName.S} has joined.";

                return message;
            }

            Message MapDeletedUserMessage(Dictionary<string, AttributeValue> attributes)
            {
                var message = new Message();

                if (attributes.TryGetValue(nameof(User.UserName), out var userName))
                    message.Content = $"{userName.S} has left.";

                return message;
            }

            if (streamRecord.EventName == OperationType.INSERT)
                return MapNewUserMessage(streamRecord.Dynamodb.NewImage);
            
            if (streamRecord.EventName == OperationType.REMOVE)
                return MapDeletedUserMessage(streamRecord.Dynamodb.OldImage);
            
            return Option<Message>.None;
        }
    }
}