namespace Serverless.Domain.Models
{
    public class UpdateUserConnectionMappingQueueMessage
    {
        public bool IsDeletion { get; set; }
        public UserConnectionMapping UserConnectionMapping { get; set; }
    }
}