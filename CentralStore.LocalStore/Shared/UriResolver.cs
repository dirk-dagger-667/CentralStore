using MassTransit;
using Microsoft.Extensions.Options;

namespace CentralStore.LocalStore.Shared
{
  public class EndpointUriResolver(IOptions<QueueMetadata> options,
    ISendEndpointProvider sendEndpointProvider) : IMassTransitSendResolver
  {
    public async Task<ISendEndpoint> GetSendEndpoint()
    {
      var queueName = $"{options.Value.CentralStoreQueueName}";
      return await sendEndpointProvider.GetSendEndpoint(Resolve(queueName));
    }

    private Uri Resolve(string queueName)
    {
      var rabbitHost = options.Value.Host ?? "localhost";
      return new Uri($"rabbitmq://{rabbitHost}/{queueName}");
    }
  }

  public interface IMassTransitSendResolver
  {
    Task<ISendEndpoint> GetSendEndpoint();
  }
}
