using CentralStore.LocalStore.Shared;
using MassTransit;
using Microsoft.Extensions.Options;
using CentralStore.Shared.Messages;

namespace CentralStore.LocalStore.ProductManagement.CreateProduct
{
  public class CreateProductConsumer(ICreateProductService service,
    IOptions<QueueMetadata> options,
    IConfiguration config,
    IMassTransitSendResolver mtResolver) : IConsumer<CreateProductMessage>
  {
    //Send to central store queue
    public async Task Consume(ConsumeContext<CreateProductMessage> context)
    {
      try
      {
        var createRslt = service.CreateProduct(context.Message.CurrentState);
        await service.SaveChangesAsync();
      }
      catch (Exception)
      {
        var storeId = config[options.Value.StoreIdConfigKey];
        var endpoint = await mtResolver.GetSendEndpoint();

        await endpoint.Send(new CreationFailedMessage(context.Message.CurrentState.Id),
          mContext => mContext.Headers.Set(options.Value.StoreIdHeaderKey, storeId));
        await service.SaveChangesAsync();
      }
    }
  }
}
