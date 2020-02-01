using System;
using System.Threading.Tasks;
using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;

namespace Serverless.Domain.AwsClients
{
    public interface ISqsClient
    {
        Task SendMessage(object message);
    }

    public class SqsClient : ISqsClient
    {
        public async Task SendMessage(object message)
        {
            using (var client = new AmazonSQSClient(RegionEndpoint.APSoutheast2))
                await client.SendMessageAsync(new SendMessageRequest
                {
                    QueueUrl = Environment.GetEnvironmentVariable("updateUserConnectionList_queue_url"),
                    MessageGroupId = "updateUserConnectionList_queue_url",
                    MessageBody = JsonConvert.SerializeObject(message)
                });
        }
    }
}