﻿using System;
using Microsoft.Extensions.DependencyInjection;
using Serverless.Domain.Authentication;
using Serverless.Domain.AwsClients;

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
    }
}