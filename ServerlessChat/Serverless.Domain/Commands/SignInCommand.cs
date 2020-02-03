using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using MediatR;
using Newtonsoft.Json;
using Serverless.Domain.Authentication;
using Serverless.Domain.AwsClients;
using Serverless.Domain.Extensions;
using Serverless.Domain.Models;
using Serverless.Domain.Requests;

namespace Serverless.Domain.Commands
{
    public class SignInCommand : IRequest<SignInCommandResponse>
    {
        public APIGatewayProxyRequest Request { get; set; }
    }

    public class SignInCommandResponse
    {
        public APIGatewayProxyResponse ApiResponse { get; private set; }

        public static SignInCommandResponse BadRequest() => new SignInCommandResponse
        {
            ApiResponse = new APIGatewayProxyResponse()
                .WithStatus(HttpStatusCode.BadRequest)
                .WithCorsHeaders()
        };

        public static SignInCommandResponse Ok(string authToken) => new SignInCommandResponse
        {
            ApiResponse = new APIGatewayProxyResponse()
                .WithStatus(HttpStatusCode.OK)
                .WithJsonBody(new
                {
                    AuthToken = authToken
                })
                .WithCorsHeaders()
        };
    }

    public class SignInCommandHandler : IRequestHandler<SignInCommand, SignInCommandResponse>
    {
        private readonly IDynamoDbClient _dynamoDbClient;
        private readonly IJwtService _jwtService;

        public SignInCommandHandler(IDynamoDbClient dynamoDbClient, IJwtService jwtService)
        {
            _dynamoDbClient = dynamoDbClient;
            _jwtService = jwtService;
        }

        public async Task<SignInCommandResponse> Handle(SignInCommand command, CancellationToken cancellationToken)
        {
            var signInRequest = JsonConvert.DeserializeObject<SignInRequest>(command.Request.Body);
            if (signInRequest == null)
                return SignInCommandResponse.BadRequest();

            var user = new User(signInRequest.UserName);
            await _dynamoDbClient.SaveUser(user);
            return SignInCommandResponse.Ok(_jwtService.GenerateJwt(user));
        }
    }
}