using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Amazon.ApiGatewayManagementApi;
using Amazon.ApiGatewayManagementApi.Model;
using Amazon.Runtime;
using Newtonsoft.Json;
using Serverless.Domain.Models;

namespace Serverless.Domain.AwsClients
{
    public interface IApiGatewayClient
    {
        Task<bool> PostMessage(string connectionId, Message message);
    }

    public class ApiGatewayClient : IApiGatewayClient
    {
        private readonly AmazonApiGatewayManagementApiClient _apiClient;

        public ApiGatewayClient()
        {
            _apiClient = new AmazonApiGatewayManagementApiClient(new AmazonApiGatewayManagementApiConfig
            {
                ServiceURL = $"https://{Environment.GetEnvironmentVariable("ws_id")}.execute-api.ap-southeast-2.amazonaws.com/{Environment.GetEnvironmentVariable("environment")}"
            });
        }

        public async Task<bool> PostMessage(string connectionId, Message message)
        {
            try
            {
                await _apiClient.PostToConnectionAsync(new PostToConnectionRequest
                {
                    ConnectionId = connectionId,
                    Data = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message)))
                });
            }
            catch (AmazonServiceException e)
            {
                if (e.StatusCode == HttpStatusCode.Gone)
                    return false;
            }
         
            return true;
        }
    }
}