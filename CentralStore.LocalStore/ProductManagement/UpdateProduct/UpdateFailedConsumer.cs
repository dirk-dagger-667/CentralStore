using MassTransit;
using CentralStore.Shared.Messages;

namespace LocalStore.ProductManagement.UpdateProduct
{
  public class UpdateFailedConsumer(IUpdateProductService service) : IConsumer<UpdateFailedMessage>
  {
    //Send to the central store queue
    public async Task Consume(ConsumeContext<UpdateFailedMessage> context)
    {
      var updateRslt = await service.UpdateProductAsync(context.Message.PreviousState);
    }
  }
}
