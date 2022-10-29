using Azure.Identity;
using CloudEventify.Rebus;
using Microsoft.Extensions.DependencyInjection;
using PubSub.Core.Handlers;
using PubSub.Core.Models;
using PubSub.Core.Options;
using PubSub.Rebus.Handlers;
using Rebus.Config;
using Rebus.Handlers;
using Rebus.Logging;
using Rebus.Retry.Simple;
using Rebus.Routing.TypeBased;
using Rebus.Topic;

namespace PubSub.Rebus;


public static class Extensions {
    public static IServiceCollection AddRebusConfiguration(this IServiceCollection services, AzureServicebusOptions? config, PubSubAppMode mode) {
        if (config == null)
            throw new ArgumentNullException(nameof(config));

        //services.AutoRegisterHandlersFromAssemblyOf<SimpleTopicNamingConvention>();
        if (mode == PubSubAppMode.Client) {
            services.AddTransient<IHandleMessages<Order>, OrderMessageHandler>();
        }
        else if (mode == PubSubAppMode.Server) {
            services.AddTransient<IHandleMessages<CreateOrderCommand>, OrderCommandMessageHandler>();
        }

        services.AddRebus((configure, provider) => configure
                        .Logging(l => l.ColoredConsole(LogLevel.Debug))

                        .UseCloudEvents(c => c
                            .WithSource(new Uri(config.SourceUri))
                            .WithTypes(t => t
                                .Map<Order>(nameof(Order))
                                .Map<CreateOrderCommand>(nameof(CreateOrderCommand))))

                        .Transport(t => t
                            .UseAzureServiceBus($"Endpoint={config.EndPoint}", config.ListeningQueue, new DefaultAzureCredential())
                            .AutomaticallyRenewPeekLock()
                                .SetDuplicateDetectionHistoryTimeWindow(new TimeSpan(0, 2, 0)))
                                .Options(c => c.SimpleRetryStrategy(errorQueueAddress: config.ErrorQueueName))

                        .Options(o => o.Decorate<ITopicNameConvention>(_ => new SimpleTopicNamingConvention()))

                        .Routing(r => {
                            var z = r.TypeBased();
                            if (!string.IsNullOrEmpty(config.CommandQueue)) {
                                z.Map<CreateOrderCommand>(config.CommandQueue);
                            }
                            z.Map<Order>(config.Topic);
                        }));

        services.AddTransient<IPubSubService, RebusPubSubService>();
        return services;
    }
}