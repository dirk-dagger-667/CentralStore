using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CentralStore.IntegrationTests.Shared
{
  public static class IServiceColletionExtensions
  {
    public static void ConfigServicesCustom<TDbContext>(this IServiceCollection services,
      IWebHostBuilder builder,
      string dbName,
      string queueName,
      RabbitMqFixture rabbitMq) where TDbContext : DbContext
    {
      services.AddDbContext<TDbContext>(options =>
          options.UseInMemoryDatabase(dbName));

      services.AddMassTransitTestHarness(x =>
      {
        x.SetKebabCaseEndpointNameFormatter();

        x.SetTestTimeouts(TimeSpan.FromSeconds(5));

        x.AddConsumers(typeof(TDbContext).Assembly);
        x.UsingRabbitMq((context, cfg) =>
        {
          cfg.Host(rabbitMq.RabbitMqConnectionString, h =>
          {
            h.Username("guest");
            h.Password("guest");
          });

          cfg.ReceiveEndpoint(queueName, e =>
          {
            e.ConfigureConsumers(context);
          });
        });
      });
    }
  }
}
