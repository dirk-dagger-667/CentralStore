using CentralStore.Shared;
using MassTransit;
using Microsoft.Extensions.Options;
using CentralStore.Shared.Messages;
using CentralStore.ProductManagementt.CreateProducts;

namespace CentralStore.ProductManagement.CreateProduct
{
  public class CreateProductConsumer(ICreateProductService service,
    ISendEndpointProvider sendEndpointProvider,
    IConfiguration config,
    IOptions<QueueMetadata> options) : IConsumer<CreateProductMessage>
  {
    //Based on the store id header send to the correct local store queue
    public async Task Consume(ConsumeContext<CreateProductMessage> context)
    {
      Guid.TryParse(context.GetHeader(options.Value.StoreIdHeaderKey), out var storeId);
      
      try
      {
        var createRslt = service.CreateProduct(context.Message.CurrentState, storeId);
        await service.SaveChangesAsync();
      }
      catch (Exception)
      {

        var queueName = $"{options.Value.LocalStoreQueueName}{storeId}";
        var endpoint = await sendEndpointProvider
              .GetSendEndpoint(new Uri($"rabbitmq://{config["RabbitMQ:Host"]}/{queueName}"));

        await endpoint.Send(new CreationFailedMessage(context.Message.CurrentState.Id));
        await service.SaveChangesAsync();
      }
    }
  }
}
