using MassTransit;
using CentralStore.Shared.Messages;
using CentralStore.ProductManagementt.CreateProducts;

namespace CentralStore.ProductManagement.CreateProduct
{
  public class CreationFailedConsumer(ICreateProductService service) : IConsumer<CreationFailedMessage>
  {
    public async Task Consume(ConsumeContext<CreationFailedMessage> context)
    {
      var createRslt = await service.RemoveProductAsync(context.Message.ProductId);
    }
  }
}
