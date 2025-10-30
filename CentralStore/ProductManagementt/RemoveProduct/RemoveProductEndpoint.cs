using CentralStore.Shared;
using CentralStore.Shared.Messages;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CentralStore.ProductManagement.RemoveProduct
{
  public class RemoveProductEndpoint : IEndpoint
  {
    private const string Route = "api/products/{id}/";
    private const string Tag = "Products";

    public void MapEndpoint(WebApplication app)
      => app.MapDelete(Route, Handle)
      .WithTags(Tag);

    private static async Task<Results<NoContent, NotFound, Conflict>> Handle(
      [FromRoute] Guid id,
      [FromBody] RemoveProductRequest request,
      IMassTransitSendResolver mtResolver,
      IRemoveProductService service)
    {
      var previousState = await service.GetProductAsync(request);

      if (previousState is null)
        return TypedResults.Conflict();

      using (var trans = await service.BeginTransactionAsync())
      {
        try
        {
          var result = await service.RemoveProductAsync(request.Id);

          if (result == 0)
            return TypedResults.NotFound();

          var endpoint = await mtResolver.GetSendEndpoint(request.StoreId);

          await endpoint.Send(new RemoveProductMessage(previousState.ToDto()));

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
