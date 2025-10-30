using CentralStore.LocalStore.Shared;
using MassTransit;
using Microsoft.Extensions.Options;
using CentralStore.Shared.Messages;

namespace CentralStore.LocalStore.ProductManagement.UpdateProduct
{
  public class UpdateProductConsumer(IUpdateProductService service,
    IOptions<QueueMetadata> options,
    IConfiguration config,
    IMassTransitSendResolver uriResolver) : IConsumer<UpdateProductMessage>
  {
    // Send to central store queue
    public async Task Consume(ConsumeContext<UpdateProductMessage> context)
    {
      var storeId = config[options.Value.StoreIdConfigKey];
      var endpoint = await uriResolver.GetSendEndpoint();

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
      catch (Exception)
      {
        await endpoint.Send(new UpdateFailedMessage(context.Message.PreviousState),
            mContext => mContext.Headers.Set(options.Value.StoreIdHeaderKey, storeId));
        await service.SaveChangesAsync();
      }
    }
  }
}
