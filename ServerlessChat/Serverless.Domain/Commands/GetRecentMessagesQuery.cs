using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using MediatR;
using Newtonsoft.Json;
using Serverless.Chat.Extensions;
using Serverless.Domain.AwsClients;
using Serverless.Domain.Models;

namespace Serverless.Domain.Commands
{
    public class GetRecentMessagesQuery : IRequest<GetRecentMessagesQueryResponse>
    {
    }

    public class GetRecentMessagesQueryResponse
    {
        public APIGatewayProxyResponse ApiResponse { get; set; }

        public static GetRecentMessagesQueryResponse Ok(IEnumerable<Message> messages) => new GetRecentMessagesQueryResponse
        {
            ApiResponse = new APIGatewayProxyResponse()
                .WithStatus(HttpStatusCode.OK)
                .WithBody(JsonConvert.SerializeObject(new { Messages = messages }))
                .WithCorsHeaders()
        };
    }

    public class GetRecentMessagesQueryHandler : IRequestHandler<GetRecentMessagesQuery, GetRecentMessagesQueryResponse>
    {
        private readonly IDynamoDbClient _dynamoDbClient;

        public GetRecentMessagesQueryHandler(IDynamoDbClient dynamoDbClient)
        {
            _dynamoDbClient = dynamoDbClient;
        }

        public async Task<GetRecentMessagesQueryResponse> Handle(GetRecentMessagesQuery request, CancellationToken cancellationToken)
        {
            var messages = await _dynamoDbClient.GetRecentMessages();
            return GetRecentMessagesQueryResponse.Ok(messages);
        }
    }
}