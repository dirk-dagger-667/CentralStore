using CentralStore.LocalStore.ProductManagent.Filters;
using CentralStore.LocalStore.Shared;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using CentralStore.Shared.Messages;
using Microsoft.AspNetCore.Mvc;

namespace CentralStore.LocalStore.ProductManagement.UpdateProduct
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
      IOptions<QueueMetadata> options,
      IConfiguration config,
      IUpdateProductService service,
      IMassTransitSendResolver uriResolver)
    {
      // Send to Central Store

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
          var endpoint = await uriResolver.GetSendEndpoint();

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
