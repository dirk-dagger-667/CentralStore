using CentralStore.Shared;
using CentralStore.Shared.Messages;
using MassTransit;
using Microsoft.Extensions.Options;

namespace CentralStore.ProductManagement.UpdateProduct
{
  public class UpdateFailedConsumer(IUpdateProductService service,
    IOptions<QueueMetadata> options) : IConsumer<UpdateFailedMessage>
  {
    //Send to the central store queue
    public async Task Consume(ConsumeContext<UpdateFailedMessage> context)
    {
      Guid.TryParse(context.GetHeader(options.Value.StoreIdHeaderKey), out var storeId);
      var updateRslt = await service.UpdateProductMqAsync(context.Message.PreviousState, storeId);
    }
  }
}
