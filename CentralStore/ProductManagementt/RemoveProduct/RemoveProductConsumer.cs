using CentralStore.Shared;
using MassTransit;
using Microsoft.Extensions.Options;
using CentralStore.Shared.Messages;

namespace CentralStore.ProductManagement.RemoveProduct
{
  public class RemoveProductConsumer(IRemoveProductService service,
    IMassTransitSendResolver mtResolver,
    IOptions<QueueMetadata> options) : IConsumer<RemoveProductMessage>
  {
    //Based on the store id header send to the correct local store queue
    public async Task Consume(ConsumeContext<RemoveProductMessage> context)
    {
      Guid.TryParse(context.GetHeader(options.Value.StoreIdHeaderKey), out var storeId);
      var endpoint = await mtResolver.GetSendEndpoint(storeId);

      try
      {
        var removeRslt = await service.RemoveProductAsync(context.Message.PreviousState.Id);
      }
      catch (Exception)
      {
        await endpoint.Send(new RemovalFailedMessage(context.Message.PreviousState));
        await service.SaveChangesAsync();
      }
    }
  }
}
