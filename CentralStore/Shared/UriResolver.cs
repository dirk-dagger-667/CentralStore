using MassTransit;
using Microsoft.Extensions.Options;

namespace CentralStore.Shared
{
  public class EndpointUriResolver(IWebHostEnvironment env,
    IConfiguration config,
    IOptions<QueueMetadata> options,
    ISendEndpointProvider sendEndpointProvider) : IMassTransitSendResolver
  {
    private Uri Resolve(string queueName, Guid storeId)
    {
      if (env.IsEnvironment(SharedConstants.IntegrationTestsEnvironement))
      {
        return new Uri($"rabbitmq://127.0.0.1:8766/{queueName}{storeId}");
      }

      var rabbitHost = config["QueueMetadata:Host"] ?? "localhost";
      return new Uri($"rabbitmq://{rabbitHost}/{queueName}{storeId}");
    }

    public async Task<ISendEndpoint> GetSendEndpoint(Guid storeId)
    {
      var queueName = $"{options.Value.LocalStoreQueueName}";
      return await sendEndpointProvider.GetSendEndpoint(Resolve(queueName, storeId));
    }
  }

  public interface IMassTransitSendResolver
  {
    Task<ISendEndpoint> GetSendEndpoint(Guid storeId);
  }
}
