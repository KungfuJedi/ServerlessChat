namespace Serverless.Domain.Requests
{
    public class SendMessageRequest
    {
        public string AuthToken { get; set; }
        public string Content { get; set; }
    }
}