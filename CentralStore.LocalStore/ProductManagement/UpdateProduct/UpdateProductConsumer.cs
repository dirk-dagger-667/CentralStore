using LocalStore.Shared;
using MassTransit;
using Microsoft.Extensions.Options;
using CentralStore.Shared.Messages;

namespace LocalStore.ProductManagement.UpdateProduct
{
  public class UpdateProductConsumer(IUpdateProductService service,
    ISendEndpointProvider sendEndpointProvider,
    IOptions<QueueMetadata> options,
    IConfiguration config) : IConsumer<UpdateProductMessage>
  {
    // Send to central store queue
    public async Task Consume(ConsumeContext<UpdateProductMessage> context)
    {
      var storeId = config[options.Value.StoreIdConfigKey];

      var queueName = $"{options.Value.CentralStoreQueueName}";
      var endpoint = await sendEndpointProvider
            .GetSendEndpoint(new Uri($"rabbitmq://{config["RabbitMQ:Host"]}/{queueName}"));

      try
      {
        if (await service.IsConflictAsync(
          context.Message.CurrentState.Id,
          context.Message.PreviousState.ConcurrencyToken))
        {
          await endpoint.Send(new UpdateFailedMessage(context.Message.PreviousState),
            mContext => mContext.Headers.Set(options.Value.StoreIdHeaderKey, storeId));
          await service.SaveChangesAsync();
          return;
        }

        var updateRslt = await service.UpdateProductMqAsync(context.Message.CurrentState);

        if (updateRslt == 0)
        {
          await endpoint.Send(new UpdateFailedMessage(context.Message.PreviousState),
            mContext => mContext.Headers.Set(options.Value.StoreIdHeaderKey, storeId));
          await service.SaveChangesAsync();
          return;
        }
      }
      catch (Exception ex)
      {
        await endpoint.Send(new UpdateFailedMessage(context.Message.PreviousState),
            mContext => mContext.Headers.Set(options.Value.StoreIdHeaderKey,
            config[options.Value.StoreIdConfigKey]));
        await service.SaveChangesAsync();
      }
    }
  }
}
