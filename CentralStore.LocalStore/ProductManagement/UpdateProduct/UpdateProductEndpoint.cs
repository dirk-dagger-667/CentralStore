using LocalStore.ProductManagent.Filters;
using LocalStore.Shared;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using CentralStore.Shared.Messages;
using Microsoft.AspNetCore.Mvc;

namespace LocalStore.ProductManagement.UpdateProduct
{
  public class UpdateProductEndpoint : IEndpoint
  {
    private const string Route = "api/products/{id}/";
    private const string Tag = "Products";

    public void MapEndpoint(WebApplication app)
      => app.MapPut(Route, Handle)
      .WithTags(Tag)
      .AddEndpointFilter<ValidationFilter<UpdateProductRequest>>();

    private static async Task<Results<NoContent, NotFound, Conflict>> Handle(
      [FromRoute] Guid id,
      [FromBody] UpdateProductRequest request,
      ISendEndpointProvider sendEndpointProvider,
      IOptions<QueueMetadata> options,
      IConfiguration config,
      IUpdateProductService service)
    {
      if (await service.IsConflictAsync(request.Id, request.ConcurrencyToken))
        return TypedResults.Conflict();

      using (var trans = await service.BeginTransactionAsync())
      {
        try
        {
          var previousState = await service.GetByIdAsync(request.Id);
          var result = await service.UpdateProductAsync(request.ToDto());

          if (result == 0 || previousState is null)
          {
            await trans.RollbackAsync();
            return TypedResults.NotFound();
          }

          var currentState = await service.GetByIdAsync(request.Id);

          if (currentState is null)
          {
            await trans.RollbackAsync();
            return TypedResults.NotFound();
          }

          var storeId = config[options.Value.StoreIdConfigKey];
          var queueName = $"{options.Value.LocalStoreQueueName}";
          var endpoint = await sendEndpointProvider
                .GetSendEndpoint(new Uri($"rabbitmq://{config["RabbitMQ:Host"]}/{queueName}"));

          await endpoint.Send(new UpdateProductMessage(previousState.ToDto(),
            currentState!.ToDto()),
              mContext => mContext.Headers.Set(options.Value.StoreIdHeaderKey, storeId));

          await service.SaveChangesAsync();
          await trans.CommitAsync();
        }
        catch (Exception)
        {
          await trans.RollbackAsync();
          throw;
        }
      }

      return TypedResults.NoContent();
    }
  }
}
