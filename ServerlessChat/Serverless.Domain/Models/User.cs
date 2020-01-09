using System;
using Amazon.DynamoDBv2.DataModel;
using Serverless.Domain.Constants;

namespace Serverless.Domain.Models
{
    [DynamoDBTable(DynamoDbTables.Users)]
    public class User
    {
        [DynamoDBHashKey]
        [DynamoDBProperty(AttributeName = "UserId")]
        public Guid Id { get; set; }

        [DynamoDBProperty]
        public string UserName { get; set; }

        [DynamoDBProperty]
        public string ConnectionId { get; set; }

        public User()
        {
            
        }

        public User(string userName)
        {
            Id = Guid.NewGuid();
            UserName = userName;
        }
    }
}