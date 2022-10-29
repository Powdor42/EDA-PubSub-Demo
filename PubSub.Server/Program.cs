using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PubSub.Core.Handlers;
using PubSub.Core.Interfaces;
using PubSub.Core.Models;
using PubSub.Core.Options;
using PubSub.MassTransit;
using PubSub.Rebus;

var mode = "Rebus";
Console.WriteLine($"Pubsub Server (Demo App) mode={mode}");

AzureServicebusOptions? azConfig = default;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) => {
        services.AddLogging(c => c.AddConsole());
        services.Configure<LoggerFilterOptions>(cf => cf.MinLevel = LogLevel.Debug);
        services.AddTransient<IOrderCommandHandler, OrderCommandHandler>();
        azConfig = context.Configuration.GetRequiredSection(nameof(AzureServicebusOptions)).Get<AzureServicebusOptions>();
        if (mode == "Rebus") {
            services.AddRebusConfiguration(azConfig, PubSubAppMode.Server);
        }
        else {
            services.AddMassTransitConfiguration(azConfig, PubSubAppMode.Server);
        }
    }).Build();

host.Start();


var logger = host.Services.GetRequiredService<ILogger<Program>>();

var pubSubService = host.Services.GetRequiredService<IPubSubService>();

logger.LogInformation("Configuration:\r\n{azConfig}", azConfig);
logger.LogInformation($"{nameof(Order)} objects are published with CloudEvent type: {nameof(Order)}");


Console.WriteLine("Press Q to quit");
Console.WriteLine($"Press 1..9 to Create an order based on the file order-x.json file and and publish it to topic: {azConfig?.Topic}");
var lastCommand = string.Empty;
while (lastCommand != "q") {
    lastCommand = Console.ReadKey().KeyChar.ToString().ToLower();
    if (lastCommand != "q") {
        var filename = $"order-{lastCommand}.json";
        logger.LogInformation($"Publishing {nameof(Order)} based on file {filename} to topic: {azConfig?.Topic}");
        var order = (Order)JsonConvert.DeserializeObject<Order>(File.ReadAllText(filename));
        await pubSubService.Publish(order!);
    }
}
Console.WriteLine("Bye!");
