using CentralStore.IntegrationTests.Shared;
using CentralStore.LocalStore.Domain;
using CentralStore.Shared;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CentralStore.IntegrationTests
{
  public class LocalStoreApi(RabbitMqFixture rabbitMq) : WebApplicationFactory<LocalStore.Program>
  {
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
      builder.UseEnvironment(SharedConstants.IntegrationTestsEnvironement);

      builder.ConfigureServices((context, services) =>
      {
        services.RemoveAll<LocalStore.Shared.IMassTransitSendResolver>();

        var prefix = context.Configuration?["QueueMetadata:LocalStoreQueueName"]
          ?? throw new ArgumentNullException("QueueMetadata:LocalStoreQueueName configuration is null");
        var queueName = $"{prefix}{TestConstants.StoreId}";

        services.AddScoped<LocalStore.Shared.IMassTransitSendResolver>(provider =>
        {
          var sendEndpointProvider = provider.GetRequiredService<ISendEndpointProvider>();

          return new TestingLocalResolver(sendEndpointProvider, rabbitMq, context.Configuration);
        });

        services.ConfigServicesCustom<LocalStoreDbContext>(builder, "LocalStoreDb", queueName, rabbitMq);
      });
    }
  }

  public class TestingLocalResolver(ISendEndpointProvider sendEndpointProvider,
    RabbitMqFixture rabbitMq,
    IConfiguration configuration) : LocalStore.Shared.IMassTransitSendResolver
  {
    private Uri Resolve(string queueName)
    {
      return new Uri($"{rabbitMq.RabbitMqConnectionString}/{queueName}");
    }

    public async Task<ISendEndpoint> GetSendEndpoint()
    {
      var queueName = configuration?["QueueMetadata:CentralStoreQueueName"]
          ?? throw new ArgumentNullException("QueueMetadata:CentralStoreQueueName configuration is null");

      return await sendEndpointProvider.GetSendEndpoint(Resolve(queueName));
    }
  }
}
