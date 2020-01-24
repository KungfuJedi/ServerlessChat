using System;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Serverless.Domain.Authentication;
using Serverless.Domain.AwsClients;
using Serverless.Domain.Models;

namespace Serverless.Chat
{
    public static class ChatDependencyContainerBuilder
    {
        public static IServiceProvider ForRecentMessages()
        {
            var serviceProvider = new ServiceCollection();
            serviceProvider.AddScoped<IDynamoDbClient, DynamoDbClient>();

            return serviceProvider.BuildServiceProvider();
        }

        public static IServiceProvider ForSignIn()
        {
            var serviceProvider = new ServiceCollection();
            serviceProvider.AddTransient<IDynamoDbClient, DynamoDbClient>();
            serviceProvider.AddTransient<IJwtService, JwtService>();

            return serviceProvider.BuildServiceProvider();
        }

        public static IServiceProvider ForAuthorizer()
        {
            var serviceProvider = new ServiceCollection();
            serviceProvider.AddTransient<IDynamoDbClient, DynamoDbClient>();
            serviceProvider.AddTransient<IJwtService, JwtService>();

            return serviceProvider.BuildServiceProvider();
        }

        public static IServiceProvider ForSendMessage()
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

        public static IServiceProvider ForMessageUpdated()
        {
            var serviceProvider = new ServiceCollection();
            serviceProvider.AddTransient<IDynamoDbClient, DynamoDbClient>();
            serviceProvider.AddTransient<IApiGatewayClient, ApiGatewayClient>();

            return serviceProvider.BuildServiceProvider();
        }

        public static IServiceProvider ForUserUpdated()
        {
            var serviceProvider = new ServiceCollection();
            serviceProvider.AddTransient<IDynamoDbClient, DynamoDbClient>();
            serviceProvider.AddTransient<IApiGatewayClient, ApiGatewayClient>();

            return serviceProvider.BuildServiceProvider();
        }

        public static IServiceProvider Build()
        {
            var serviceProvider = new ServiceCollection();
            serviceProvider.AddTransient<IDynamoDbClient, DynamoDbClient>();
            serviceProvider.AddTransient<IApiGatewayClient, ApiGatewayClient>();
            serviceProvider.AddTransient<IJwtService, JwtService>();
            serviceProvider.AddMediatR(typeof(User).Assembly);

            return serviceProvider.BuildServiceProvider();
        }
    }
}