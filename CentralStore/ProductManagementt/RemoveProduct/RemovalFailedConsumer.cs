using MassTransit;
using CentralStore.Shared.Messages;

namespace CentralStore.ProductManagement.RemoveProduct
{
  public class RemovalFailedConsumer(IRemoveProductService service) : IConsumer<RemovalFailedMessage>
  {
    public async Task Consume(ConsumeContext<RemovalFailedMessage> context)
    {
      // Get the store id that you need so that you add it to the product store
      var removeRslt = await service.CreateProductAsync(context.Message.PreviousState);
    }
  }
}
