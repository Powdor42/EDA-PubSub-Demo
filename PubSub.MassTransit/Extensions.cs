using CloudEventify.MassTransit;
using MassTransit;
using MassTransit.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PubSub.Core.Handlers;
using PubSub.Core.Models;
using PubSub.Core.Options;
using PubSub.MassTransit.Consumers;

namespace PubSub.MassTransit; 
public static class Extensions {
    public static IServiceCollection AddMassTransitConfiguration(this IServiceCollection services, IConfiguration configuration, PubSubAppMode mode) {
        var config = configuration.GetRequiredSection(nameof(AzureServicebusOptions)).Get<AzureServicebusOptions>();
        if (config == null)
            throw new ArgumentNullException(nameof(config));
        //LogContext.ConfigureCurrentLogContext(Logger);
        services.AddMassTransit((bc) => {
            bc.SetKebabCaseEndpointNameFormatter();
            bc.UsingAzureServiceBus((ctx, cfg) => {
                cfg.Host(new Uri(config.EndPoint));
                cfg.UseCloudEvents()
                            .WithTypes(t =>
                                        t.Map<Order>(nameof(Order))
                                         .Map<CreateOrderCommand>(nameof(CreateOrderCommand)));

                cfg.Message<Order>(x => x.SetEntityName(config.Topic));
                if (mode == PubSubAppMode.Client) {
                    cfg.Message<CreateOrderCommand>(x => x.SetEntityName(config.CommandQueue));
                }
                else {
                    cfg.Message<CreateOrderCommand>(x => x.SetEntityName(config.ListeningQueue));
                }
                cfg.ConfigureEndpoints(ctx);
            });
            //bc.AddConsumers(typeof(MTPubSubService).Assembly);
            if (mode == PubSubAppMode.Client) {
                bc.AddConsumer<OrderConsumer>();
                bc.RegisterConsumer<OrderConsumer>();
            }
            else {
                bc.AddConsumer<CreateOrderConsumer>();
                bc.RegisterConsumer<CreateOrderConsumer>();
            }
        });

        services.AddTransient<IPubSubService, MTPubSubService>();

        return services;
    }

}