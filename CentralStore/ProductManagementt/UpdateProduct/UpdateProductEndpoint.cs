using CentralStore.ProductManagement.Filters;
using CentralStore.Shared;
using CentralStore.Shared.Messages;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CentralStore.ProductManagement.UpdateProduct
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
      IMassTransitSendResolver mtResolver,
      IUpdateProductService service)
    {
      if (await service.IsConflictAsync(request.Id, request.ConcurrencyToken))
        return TypedResults.Conflict();

      using (var trans = await service.BeginTransactionAsync())
      {
        try
        {
          var previousState = await service.GetById(request.Id);
          var result = await service.UpdateProductApiAsync(request.ToDto(), request.StoreId);

          if (result == 0 || previousState is null)
          {
            await trans.RollbackAsync();
            return TypedResults.NotFound();
          }

          var currentState = await service.GetById(request.Id);

          if (currentState is null)
          {
            await trans.RollbackAsync();
            return TypedResults.NotFound();
          }

          var endpoint = await mtResolver.GetSendEndpoint(request.StoreId);

          await endpoint.Send(new UpdateProductMessage(previousState.ToDto(), currentState.ToDto()));

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
