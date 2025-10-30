using CentralStore.Shared;
using MassTransit;
using Microsoft.Extensions.Options;
using CentralStore.Shared.Messages;

namespace CentralStore.ProductManagement.UpdateProduct
{
  public class UpdateProductConsumer(IUpdateProductService service,
    IOptions<QueueMetadata> options,
    IMassTransitSendResolver mtResolver) : IConsumer<UpdateProductMessage>
  {
    //Based on the store id send to the correct localstore queue
    public async Task Consume(ConsumeContext<UpdateProductMessage> context)
    {
      Guid.TryParse(context.GetHeader(options.Value.StoreIdHeaderKey), out var storeId);
      var endpoint = await mtResolver.GetSendEndpoint(storeId);

      try
      {
        if (await service.IsConflictAsync(
          context.Message.CurrentState.Id,
          context.Message.PreviousState.ConcurrencyToken))
        {
          await endpoint.Send(new UpdateFailedMessage(context.Message.PreviousState));
          await service.SaveChangesAsync();
          return;
        }

        var updateRslt = await service.UpdateProductMqAsync(context.Message.CurrentState, storeId);

        if (updateRslt == 0)
        {
          await endpoint.Send(new UpdateFailedMessage(context.Message.PreviousState));
          await service.SaveChangesAsync();
        }
      }
      catch (Exception)
      {
        await endpoint.Send(new UpdateFailedMessage(context.Message.PreviousState));
        await service.SaveChangesAsync();
      }
    }
  }
}
