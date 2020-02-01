using System;
using Amazon.DynamoDBv2.DataModel;
using Serverless.Domain.Constants;

namespace Serverless.Domain.Models
{
    [DynamoDBTable(DynamoDbTables.Users)]
    public class User
    {
        [DynamoDBHashKey]
        public Guid UserId { get; set; }

        [DynamoDBProperty]
        public string UserName { get; set; }

        [DynamoDBProperty]
        public string ConnectionId { get; set; }

        public User()
        {
            
        }

        public User(string userName)
        {
            UserId = Guid.NewGuid();
            UserName = userName;
        }
    }
}