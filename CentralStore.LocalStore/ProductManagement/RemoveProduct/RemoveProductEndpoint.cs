using CentralStore.LocalStore.Shared;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using CentralStore.Shared.Messages;
using Microsoft.AspNetCore.Mvc;

namespace CentralStore.LocalStore.ProductManagement.RemoveProduct
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
      IOptions<QueueMetadata> options,
      IConfiguration config,
      IRemoveProductService service,
      IMassTransitSendResolver mtResolver)
    {
      var previousState = await service.GetProductAsync(request);

      if (previousState is null)
        return TypedResults.Conflict();

      var result = await service.RemoveProductAsync(request.Id);

      using (var trans = await service.BeginTransactionAsync())
      {
        try
        {
          if (result == 0)
            return TypedResults.NotFound();

          var storeId = config[options.Value.StoreIdConfigKey];
          var endpoint = await mtResolver.GetSendEndpoint();
          await endpoint.Send(new RemoveProductMessage(previousState.ToDto()),
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
