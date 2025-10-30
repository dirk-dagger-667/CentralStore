using CentralStore.LocalStore.Domain;
using CentralStore.LocalStore.Shared;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CentralStore.LocalStore.ProductManagement.GetProduct
{
  public class GetProductEndpoint : IEndpoint
  {
    private const string Route = "api/products/{id}/";
    private const string Tag = "Products";

    public void MapEndpoint(WebApplication app)
      => app.MapGet(Route, Handle)
      .WithTags(Tag);

    private static async Task<Results<
      Ok<Product>,
      NotFound>> Handle(
      [FromRoute] Guid id,
      LocalStoreDbContext dbContext)
    {
      var product = await dbContext.Products
        .AsNoTracking()
        .SingleOrDefaultAsync(p => p.Id == id);

      if (product is null)
        return TypedResults.NotFound();

      return TypedResults.Ok(product);
    }
  }
}
