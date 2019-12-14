﻿using System;
using Amazon.DynamoDBv2.DataModel;
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
    }
}