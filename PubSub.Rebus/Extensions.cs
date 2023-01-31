using Azure.Identity;
using CloudEventify.Rebus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PubSub.Core.Handlers;
using PubSub.Core.Models;
using PubSub.Core.Options;
using PubSub.Rebus.Handlers;
using Rebus.Config;
using Rebus.Handlers;
using Rebus.Retry.Simple;
using Rebus.Routing.TypeBased;
using Rebus.Topic;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace PubSub.Rebus;


public static class Extensions {
    public static IServiceCollection AddRebusConfiguration(this IServiceCollection services, IConfiguration configuration, PubSubAppMode mode) {
        var config = configuration.GetRequiredSection(nameof(AzureServicebusOptions)).Get<AzureServicebusOptions>();
        if (config == null)
            throw new ArgumentNullException(nameof(config));

        services.AddSingleton(config);

        if (mode != PubSubAppMode.Server) {
            services.AddTransient<IHandleMessages<Order>, OrderMessageHandler>();
        }
        else if (mode != PubSubAppMode.Client) {
            services.AddTransient<IHandleMessages<CreateOrderCommand>, OrderCommandMessageHandler>();
        }

        services.AddRebus((configure, provider) => configure
                    .Logging(l => l.ColoredConsole((global::Rebus.Logging.LogLevel)LogLevel.Debug))
                    .Options(o => {
                        o.SetNumberOfWorkers(1);
                        o.InjectMessageId();
                        //o.UseCustomTypeNameForTopicName();
                        o.Decorate<ITopicNameConvention>(_ => new CustomMappingTopicNamingConvention(new Dictionary<Type, string> {
                            {typeof(Order),config.Topic }
                        }));
                        o.SimpleRetryStrategy(errorQueueAddress: config.ErrorQueueName);
                        if (mode == PubSubAppMode.Infra) { o.SetNumberOfWorkers(0); }
                    })
                    .Serialization(s => s.UseCloudEvents()
                                        .AddWithShortName<Order>()
                                        .AddWithShortName<CreateOrderCommand>()
                                  )
                    .Routing(r =>
                    {
                        var z = r.TypeBased();
                        if (!string.IsNullOrEmpty(config.CommandQueue)) {
                            z.Map<CreateOrderCommand>(config.CommandQueue);
                        }
                        z.Map<Order>(config.Topic);
                    })
                    .Transport(t => {
                        var azTransport = t.UseAzureServiceBus($"Endpoint={config.EndPoint}", config.ListeningQueue, new DefaultAzureCredential())
                                    .AutomaticallyRenewPeekLock()
                                    .SetDuplicateDetectionHistoryTimeWindow(new TimeSpan(0, 2, 0));

                        if (mode != PubSubAppMode.Infra) {
                            azTransport
                                    .DoNotCheckQueueConfiguration()
                                    .DoNotCreateQueues();
                        }
                    })
                );

        services.AddTransient<IPubSubService, RebusPubSubService>();
        return services;
    }
}