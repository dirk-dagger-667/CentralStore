using CentralStore.LocalStore.Domain;
using MassTransit;

namespace CentralStore.LocalStore
{
  public static class MassTransitConfig
  {
    public static IServiceCollection ConfigureMassTransit(this WebApplicationBuilder builder)
      => builder.Services.AddMassTransit(busConfig =>
      {
        busConfig.SetKebabCaseEndpointNameFormatter();
        busConfig.AddEntityFrameworkOutbox<LocalStoreDbContext>(options =>
        {
          options.QueryDelay = TimeSpan.FromSeconds(10);
          options.UseSqlServer();
          options.UseBusOutbox();
        });

        busConfig.AddConsumers(typeof(Program).Assembly);

        busConfig.UsingRabbitMq((context, config) =>
        {
          config.Host(builder.Configuration["QueueMetadata:Host"]);

          var storeId = Environment.GetEnvironmentVariable("STORE_ID");
          var queueName = $"{builder.Configuration["QueueMetadata:LocalStoreQueueName"]}{storeId}";

          config.ReceiveEndpoint(queueName, e =>
          {
            e.ConfigureConsumers(context);
          });
        });
      });
  }
}
