using System;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Serverless.Domain.Authentication;
using Serverless.Domain.AwsClients;
using Serverless.Domain.Commands;

namespace Serverless.Chat
{
    public static class ChatDependencyContainerBuilder
    {
        public static IServiceProvider ForAuthorizer()
        {
            var serviceProvider = new ServiceCollection();
            serviceProvider.AddTransient<IDynamoDbClient, DynamoDbClient>();
            serviceProvider.AddTransient<IJwtService, JwtService>();

            return serviceProvider.BuildServiceProvider();
        }

        public static IServiceProvider ForConnect()
        {
            var serviceProvider = new ServiceCollection();
            serviceProvider.AddTransient<IDynamoDbClient, DynamoDbClient>();
            serviceProvider.AddTransient<IJwtService, JwtService>();

            return serviceProvider.BuildServiceProvider();
        }

        public static IServiceProvider Build()
        {
            var serviceProvider = new ServiceCollection();
            serviceProvider.AddTransient<IDynamoDbClient, DynamoDbClient>();
            serviceProvider.AddTransient<IApiGatewayClient, ApiGatewayClient>();
            serviceProvider.AddTransient<IJwtService, JwtService>();
            serviceProvider.AddMediatR(typeof(SendMessageCommand).Assembly);

            return serviceProvider.BuildServiceProvider();
        }
    }
}