using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using MediatR;
using Newtonsoft.Json;
using Serverless.Domain.Authentication;
using Serverless.Domain.AwsClients;
using Serverless.Domain.Extensions;
using Serverless.Domain.Requests;

namespace Serverless.Domain.Commands
{
    public class RegisterConnectionCommand : IRequest<RegisterConnectionCommandResponse>
    {
        public APIGatewayProxyRequest Request { get; set; }
    }

    public class RegisterConnectionCommandResponse
    {
        public APIGatewayProxyResponse ApiResponse { get; set; }

        public static RegisterConnectionCommandResponse Unauthorized() => new RegisterConnectionCommandResponse
        {
            ApiResponse = new APIGatewayProxyResponse()
                .WithStatus(HttpStatusCode.Unauthorized)
                .WithCorsHeaders()
                .WithEmptyJsonBody()
        };

        public static RegisterConnectionCommandResponse Ok() => new RegisterConnectionCommandResponse
        {
            ApiResponse = new APIGatewayProxyResponse()
                .WithStatus(HttpStatusCode.OK)
                .WithCorsHeaders()
                .WithEmptyJsonBody()
        };
    }

    public class RegisterConnectionCommandHandler : IRequestHandler<RegisterConnectionCommand, RegisterConnectionCommandResponse>
    {
        private readonly IJwtService _jwtService;
        private readonly IDynamoDbClient _dynamoDbClient;

        public RegisterConnectionCommandHandler(IJwtService jwtService, IDynamoDbClient dynamoDbClient)
        {
            _jwtService = jwtService;
            _dynamoDbClient = dynamoDbClient;
        }

        public async Task<RegisterConnectionCommandResponse> Handle(RegisterConnectionCommand command, CancellationToken cancellationToken)
        {
            var registerConnectionRequest = JsonConvert.DeserializeObject<RegisterConnectionRequest>(command.Request.Body);
            if (registerConnectionRequest == null || string.IsNullOrEmpty(registerConnectionRequest.AuthToken))
                return RegisterConnectionCommandResponse.Unauthorized();

            var connectionId = command.Request.RequestContext.ConnectionId;

            var userId = _jwtService.VerifyJwt(registerConnectionRequest.AuthToken);
            if (!userId.HasValue)
                return RegisterConnectionCommandResponse.Unauthorized();

            await _dynamoDbClient.SaveConnectionId(connectionId, userId.Value);

            return RegisterConnectionCommandResponse.Ok();
        }
    }
}