using System;

namespace Serverless.Domain.Models
{
    public class UserConnectionMapping
    {
        public Guid UserId { get; set; }
        public string ConnectionId { get; set; }
    }
}