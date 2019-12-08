﻿using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serverless.Chat.Extensions;
using Serverless.Domain.AwsClients;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace Serverless.Chat
{
    public class ChatFunctions
    {
        public async Task<APIGatewayProxyResponse> GetRecentMessages(APIGatewayProxyRequest request, CancellationToken cancellationToken)
        {
            var serviceProvider = ChatDependencyContainerBuilder.ForRecentMessages();
            var dynamoClient = serviceProvider.GetService<IDynamoDbClient>();
            var messages = await dynamoClient.GetRecentMessages(cancellationToken);

            return new APIGatewayProxyResponse()
                .WithStatus(HttpStatusCode.OK)
                .WithBody(JsonConvert.SerializeObject(messages));
        }
    }
}