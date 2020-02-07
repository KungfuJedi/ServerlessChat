﻿using System.Net;
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
    public class SendMessageCommand : IRequest<SendMessageCommandResponse>
    {
        public APIGatewayProxyRequest Request { get; set; }
    }

    public class SendMessageCommandResponse
    {
        public APIGatewayProxyResponse ApiResponse { get; private set; }

        public static SendMessageCommandResponse Unauthorized() => new SendMessageCommandResponse
        {
            ApiResponse = new APIGatewayProxyResponse()
                .WithStatus(HttpStatusCode.Unauthorized)
                .WithEmptyJsonBody()
        };

        public static SendMessageCommandResponse BadRequest() => new SendMessageCommandResponse
        {
            ApiResponse = new APIGatewayProxyResponse()
                .WithStatus(HttpStatusCode.BadRequest)
                .WithEmptyJsonBody()
        };

        public static SendMessageCommandResponse Ok() => new SendMessageCommandResponse
        {
            ApiResponse = new APIGatewayProxyResponse()
                .WithStatus(HttpStatusCode.OK)
                .WithEmptyJsonBody()
        };
    }

    public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, SendMessageCommandResponse>
    {
        private readonly IJwtService _jwtService;
        private readonly IDynamoDbClient _dynamoDbClient;

        public SendMessageCommandHandler(IJwtService jwtService, IDynamoDbClient dynamoDbClient)
        {
            _jwtService = jwtService;
            _dynamoDbClient = dynamoDbClient;
        }

        public async Task<SendMessageCommandResponse> Handle(SendMessageCommand command, CancellationToken cancellationToken)
        {
            var sendMessageRequest = JsonConvert.DeserializeObject<SendMessageRequest>(command.Request.Body);
            if (sendMessageRequest == null || string.IsNullOrEmpty(sendMessageRequest.AuthToken))
                return SendMessageCommandResponse.BadRequest();

            var userId = _jwtService.VerifyJwt(sendMessageRequest.AuthToken);
            if (!userId.HasValue || !await _dynamoDbClient.CheckUserExists(userId.Value))
                return SendMessageCommandResponse.Unauthorized();

            if (string.IsNullOrEmpty(sendMessageRequest.Content))
                return SendMessageCommandResponse.BadRequest();

            var userName = _jwtService.GetClaim(sendMessageRequest.AuthToken, Claims.UserName);
            await _dynamoDbClient.SaveMessage(userName, sendMessageRequest.Content);

            return SendMessageCommandResponse.Ok();
        }
    }
}