using LocalStore.Shared;
using MassTransit;
using Microsoft.Extensions.Options;
using CentralStore.Shared.Messages;

namespace LocalStore.ProductManagement.CreateProduct
{
  public class CreateProductConsumer(ICreateProductService service,
    ISendEndpointProvider sendEndpointProvider,
    IOptions<QueueMetadata> options,
    IConfiguration config) : IConsumer<CreateProductMessage>
  {
    //Send to central store queue
    public async Task Consume(ConsumeContext<CreateProductMessage> context)
    {
      try
      {
        var createRslt = service.CreateProduct(context.Message.CurrentState);
        await service.SaveChangesAsync();
      }
      catch (Exception ex)
      {
        var storeId = config[options.Value.StoreIdConfigKey];

        var queueName = $"{options.Value.CentralStoreQueueName}";
        var endpoint = await sendEndpointProvider
              .GetSendEndpoint(new Uri($"rabbitmq://{config["RabbitMQ:Host"]}/{queueName}"));

        await endpoint.Send(new CreationFailedMessage(context.Message.CurrentState.Id),
          mContext => mContext.Headers.Set(options.Value.StoreIdHeaderKey, storeId));
        await service.SaveChangesAsync();
      }
    }
  }
}
