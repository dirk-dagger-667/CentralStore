using CentralStore.Shared;
using MassTransit;
using Microsoft.Extensions.Options;
using CentralStore.Shared.Messages;

namespace CentralStore.ProductManagement.RemoveProduct
{
  public class RemoveProductConsumer(IRemoveProductService service,
    ISendEndpointProvider sendEndpointProvider,
    IConfiguration config,
    IOptions<QueueMetadata> options) : IConsumer<RemoveProductMessage>
  {
    //Based on the store id header send to the correct local store queue
    public async Task Consume(ConsumeContext<RemoveProductMessage> context)
    {
      Guid.TryParse(context.GetHeader(options.Value.StoreIdHeaderKey), out var storeId);

      var queueName = $"{options.Value.LocalStoreQueueName}{storeId}";
      var endpoint = await sendEndpointProvider
            .GetSendEndpoint(new Uri($"rabbitmq://{config["RabbitMQ:Host"]}/{queueName}"));

      try
      {
        var removeRslt = await service.RemoveProductAsync(context.Message.PreviousState.Id);

        if (removeRslt == 0)
        {
          await endpoint.Send(new RemovalFailedMessage(context.Message.PreviousState));
          await service.SaveChangesAsync();
        }
      }
      catch (Exception ex)
      {
        await endpoint.Send(new RemovalFailedMessage(context.Message.PreviousState));
        await service.SaveChangesAsync();
      }
    }
  }
}
