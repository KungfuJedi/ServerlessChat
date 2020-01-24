using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using MediatR;
using Newtonsoft.Json;
using Serverless.Chat.Extensions;
using Serverless.Domain.Authentication;
using Serverless.Domain.AwsClients;
using Serverless.Domain.Requests;

namespace Serverless.Domain.Commands
{
    public class SignInCommand : IRequest<SignInCommandResponse>
    {
        public string RequestBody { get; set; }
    }

    public class SignInCommandResponse
    {
        public APIGatewayProxyResponse ApiResponse { get; set; }
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
            var signInRequest = JsonConvert.DeserializeObject<SignInRequest>(command.RequestBody);
            if (signInRequest == null)
                return new SignInCommandResponse
                {
                    ApiResponse = new APIGatewayProxyResponse()
                        .WithStatus(HttpStatusCode.BadRequest)
                        .WithCorsHeaders()
                };

            var user = await _dynamoDbClient.SignIn(signInRequest.UserName);
            return new SignInCommandResponse
            {
                ApiResponse = new APIGatewayProxyResponse()
                    .WithStatus(HttpStatusCode.OK)
                    .WithBody(JsonConvert.SerializeObject(new
                    {
                        AuthToken = _jwtService.GenerateJwt(user)
                    }))
                    .WithCorsHeaders()
            };
        }
    }
}