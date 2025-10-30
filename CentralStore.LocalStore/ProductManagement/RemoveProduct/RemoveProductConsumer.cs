using CentralStore.LocalStore.Shared;
using MassTransit;
using Microsoft.Extensions.Options;
using CentralStore.Shared.Messages;

namespace CentralStore.LocalStore.ProductManagement.RemoveProduct
{
  public class RemoveProductConsumer(IRemoveProductService service,
    IOptions<QueueMetadata> options,
    IConfiguration config,
    IMassTransitSendResolver mtResolver) : IConsumer<RemoveProductMessage>
  {
    //Send to central store queue
    public async Task Consume(ConsumeContext<RemoveProductMessage> context)
    {
      var storeId = config[options.Value.StoreIdConfigKey];
      var endpoint = await mtResolver.GetSendEndpoint();

      try
      {
        var removeRslt = await service.RemoveProductAsync(context.Message.PreviousState.Id);
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
