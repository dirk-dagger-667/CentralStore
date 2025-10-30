using MassTransit;
using CentralStore.Shared.Messages;

namespace CentralStore.LocalStore.ProductManagement.CreateProduct
{
  public class CreationFailedConsumer(ICreateProductService service) : IConsumer<CreationFailedMessage>
  {
    public async Task Consume(ConsumeContext<CreationFailedMessage> context)
    {
      var createRslt = await service.RemoveProductAsync(context.Message.ProductId);
    }
  }
}
