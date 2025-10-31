using CentralStore.Domain;
using CentralStore.IntegrationTests.Shared;
using CentralStore.Shared;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CentralStore.IntegrationTests
{
  public class CentralStoreApi(RabbitMqFixture rabbitMq) : WebApplicationFactory<Program>
  {
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
      builder.UseEnvironment(SharedConstants.IntegrationTestsEnvironement);

      builder.ConfigureServices((context, services) =>
      {
        services.RemoveAll<IMassTransitSendResolver>();

        var consumerQueueName = context.Configuration?["QueueMetadata:CentralStoreQueueName"]
          ?? throw new ArgumentNullException("QueueMetadata:CentralStoreQueueName configuration is null");

        services.AddScoped<IMassTransitSendResolver>(provider =>
        {
          var sendEndpointProvider = provider.GetRequiredService<ISendEndpointProvider>();

          return new TestingCentralResolver(sendEndpointProvider, rabbitMq, context.Configuration);
        });

        services.ConfigServicesCustom<CentralStoreDbContext>(builder, "CentralStoreDb", consumerQueueName, rabbitMq);
      });
    }
  }

  public class TestingCentralResolver(ISendEndpointProvider sendEndpointProvider,
    RabbitMqFixture rabbitMq,
    IConfiguration config) : IMassTransitSendResolver
  {
    public async Task<ISendEndpoint> GetSendEndpoint(Guid storeId)
    {
      var queueName = config?["QueueMetadata:LocalStoreQueueName"]
          ?? throw new ArgumentNullException("QueueMetadata:LocalStoreQueueName configuration is null");

      return await sendEndpointProvider.GetSendEndpoint(Resolve(queueName, storeId));
    }

    private Uri Resolve(string queueName, Guid storeId)
    {
      return new Uri($"{rabbitMq.RabbitMqConnectionString}/{queueName}{storeId}");
    }
  }
}
