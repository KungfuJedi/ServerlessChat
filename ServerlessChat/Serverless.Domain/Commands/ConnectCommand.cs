using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using MediatR;
using Serverless.Chat.Extensions;
using Serverless.Domain.Authentication;
using Serverless.Domain.AwsClients;

namespace Serverless.Domain.Commands
{
    public class ConnectCommand : IRequest<ConnectCommandResponse>
    {
        public APIGatewayProxyRequest Request { get; set; }
    }

    public class ConnectCommandResponse
    {
        public APIGatewayProxyResponse ApiResponse { get; set; }

        public static ConnectCommandResponse Unauthorized() => new ConnectCommandResponse
        {
            ApiResponse = new APIGatewayProxyResponse()
                .WithStatus(HttpStatusCode.Unauthorized)
                .WithCorsHeaders()
        };

        public static ConnectCommandResponse Ok() => new ConnectCommandResponse
        {
            ApiResponse = new APIGatewayProxyResponse()
                .WithStatus(HttpStatusCode.OK)
                .WithCorsHeaders()
        };
    }

    public class ConnectCommandHandler : IRequestHandler<ConnectCommand, ConnectCommandResponse>
    {
        private readonly IJwtService _jwtService;
        private readonly IDynamoDbClient _dynamoDbClient;

        public ConnectCommandHandler(IJwtService jwtService, IDynamoDbClient dynamoDbClient)
        {
            _jwtService = jwtService;
            _dynamoDbClient = dynamoDbClient;
        }

        public async Task<ConnectCommandResponse> Handle(ConnectCommand command, CancellationToken cancellationToken)
        {
            if (!command.Request.MultiValueHeaders.TryGetValue(Headers.SecurityWebsocketProtocol, out var token))
                return ConnectCommandResponse.Unauthorized();

            var connectionId = command.Request.RequestContext.ConnectionId;

            var userId = _jwtService.VerifyJwt(token.First());
            if (!userId.HasValue)
                return ConnectCommandResponse.Unauthorized();

            await _dynamoDbClient.SaveConnectionId(connectionId, userId.Value);

            return ConnectCommandResponse.Ok();
        }
    }
}