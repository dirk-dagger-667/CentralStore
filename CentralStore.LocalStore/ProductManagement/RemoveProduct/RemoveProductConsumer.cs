using LocalStore.Shared;
using MassTransit;
using Microsoft.Extensions.Options;
using CentralStore.Shared.Messages;

namespace LocalStore.ProductManagement.RemoveProduct
{
  public class RemoveProductConsumer(IRemoveProductService service,
    ISendEndpointProvider sendEndpointProvider,
    IOptions<QueueMetadata> options,
    IConfiguration config) : IConsumer<RemoveProductMessage>
  {
    //Send to central store queue
    public async Task Consume(ConsumeContext<RemoveProductMessage> context)
    {
      var storeId = config[options.Value.StoreIdConfigKey];

      var queueName = $"{options.Value.CentralStoreQueueName}";
      var endpoint = await sendEndpointProvider
            .GetSendEndpoint(new Uri($"rabbitmq://{config["RabbitMQ:Host"]}/{queueName}"));

      try
      {
        var removeRslt = await service.RemoveProductAsync(context.Message.PreviousState.Id);

        if(removeRslt == 0)
        {
          await endpoint.Send(new RemovalFailedMessage(context.Message.PreviousState),
          mContext => mContext.Headers.Set(options.Value.StoreIdHeaderKey, storeId));
          await service.SaveChangesAsync();
        }
      }
      catch (Exception)
      {
        await endpoint.Send(new RemovalFailedMessage(context.Message.PreviousState),
          mContext => mContext.Headers.Set(options.Value.StoreIdHeaderKey, storeId));
        await service.SaveChangesAsync();
      }
    }
  }
}
