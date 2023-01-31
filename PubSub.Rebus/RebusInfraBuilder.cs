using Azure.Core;
using System.Net.Http.Headers;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using PubSub.Core.Interfaces;
using PubSub.Core.Models;
using Rebus.Bus;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace PubSub.Rebus;

public class RebusInfraBuilder : IPubSubInfraBuilder {

    public async Task Build(IEnumerable<Type> subscriptionTopicTypes, PubSubAppMode mode) {
        var host = Host.CreateDefaultBuilder().ConfigureServices((context, services) => {
            services.AddLogging(c => c.AddConsole());
            services.Configure<LoggerFilterOptions>(cf => cf.MinLevel = LogLevel.Debug);

            services.AddRebusConfiguration(context.Configuration, PubSubAppMode.Infra);
        }).Build();

        host.Start();
        var bus = host.Services.GetRequiredService<IBus>();

        foreach (var topicType in subscriptionTopicTypes) {
            bus.Subscribe(topicType);
        }
        
        //Provide read/write access to the queue

        if (mode == PubSubAppMode.Server) {
            //Provide access to the Topics (Write)
        }
    }
}
