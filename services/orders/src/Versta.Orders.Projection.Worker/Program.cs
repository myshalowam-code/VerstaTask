using Versta.Orders.Infrastructure;
using Versta.Orders.Infrastructure.Messaging;
using Versta.Orders.Projection.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddReadModel(builder.Configuration);
builder.Services.Configure<KafkaOptions>(builder.Configuration.GetSection(KafkaOptions.SectionName));
builder.Services.AddHostedService<OrderProjectionWorker>();

await builder.Build().RunAsync();
