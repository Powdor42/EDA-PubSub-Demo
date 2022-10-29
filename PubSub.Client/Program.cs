using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PubSub.Core.Handlers;
using PubSub.Core.Models;
using PubSub.Core.Options;
using PubSub.MassTransit;
using PubSub.Rebus;

var mode = "Rebus";

Console.WriteLine($"Pubsub Client (Demo App) mode={mode}");

AzureServicebusOptions? azConfig = default;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) => {
        services.AddLogging(c => c.AddConsole());
        services.Configure<LoggerFilterOptions>(cf => cf.MinLevel = LogLevel.Debug);
        services.AddTransient<IOrderHandler, OrderHandler>();
        azConfig = context.Configuration.GetRequiredSection(nameof(AzureServicebusOptions)).Get<AzureServicebusOptions>();
        if (mode == "Rebus") {
            services.AddRebusConfiguration(azConfig, PubSubAppMode.Client);
        }
        else {
            services.AddMassTransitConfiguration(azConfig, PubSubAppMode.Client);
        }
    }).Build();

host.Start();


var logger = host.Services.GetRequiredService<ILogger<Program>>();

var pubSubService = host.Services.GetRequiredService<IPubSubService>();

await pubSubService.Subcribe<Order>(azConfig!.Topic);

logger.LogInformation("Configuration:\r\n{azConfig}", azConfig);
logger.LogInformation($"{nameof(Order)} objects are published with CloudEvent type: {nameof(Order)}");

Console.WriteLine("Press Q to quit");
Console.WriteLine($"Press 1..9 to Create an order based on the file create-order-x.json file and Send it to queue: {azConfig?.Topic}");
var lastCommand = string.Empty;
while (lastCommand != "q") {
    lastCommand = Console.ReadKey().KeyChar.ToString().ToLower();
    if (lastCommand != "q") {
        var filename = $"create-order-{lastCommand}.json";
        Console.WriteLine($"Sending {nameof(CreateOrderCommand)} based on file {filename} to {azConfig?.CommandQueue}");
        var createOrderCommand = (CreateOrderCommand)JsonConvert.DeserializeObject<CreateOrderCommand>(File.ReadAllText(filename));
        await pubSubService.Send(createOrderCommand!);
    }
}
Console.WriteLine("Bye!");
