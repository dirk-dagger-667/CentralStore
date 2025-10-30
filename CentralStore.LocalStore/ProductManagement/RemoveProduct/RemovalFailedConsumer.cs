using CentralStore.LocalStore.Shared;
using MassTransit;
using Microsoft.Extensions.Options;
using CentralStore.Shared.Messages;

namespace CentralStore.LocalStore.ProductManagement.RemoveProduct
{
  public class RemovalFailedConsumer(IRemoveProductService service) : IConsumer<RemovalFailedMessage>
  {
    public async Task Consume(ConsumeContext<RemovalFailedMessage> context)
    {
      var removeRslt = await service.CreateProductAsync(context.Message.PreviousState);
    }
  }
}
