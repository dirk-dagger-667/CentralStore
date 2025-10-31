using MassTransit;
using Microsoft.Extensions.Options;

namespace CentralStore.Shared
{
  public class EndpointUriResolver(IOptions<QueueMetadata> options,
    ISendEndpointProvider sendEndpointProvider) : IMassTransitSendResolver
  {
    private Uri Resolve(string queueName, Guid storeId)
    {
      var rabbitHost = options.Value.Host ?? "localhost";
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
