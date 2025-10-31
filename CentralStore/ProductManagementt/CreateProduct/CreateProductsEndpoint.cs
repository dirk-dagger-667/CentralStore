using CentralStore.ProductManagement.Filters;
using CentralStore.Shared;
using CentralStore.Shared.Messages;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CentralStore.ProductManagement.CreateProduct
{
  public class CreateProductsEndpoint : IEndpoint
  {
    private const string Route = "api/products";
    private const string Tag = "Products";

    public void MapEndpoint(WebApplication app)
      => app.MapPost(Route, Handle)
      .WithTags(Tag)
      .AddEndpointFilter<ValidationFilter<CreateProductRequest>>();

    private static async Task<Results<
      Created<CreateProductResponse>,
        ValidationProblem>> Handle(
      [FromBody] CreateProductRequest request,
      IMassTransitSendResolver mtResolver,
      ICreateProductService service)
    {
      var product = service.CreateProduct(request.ToDto(), request.StoreId);
      var endpoint = await mtResolver.GetSendEndpoint(request.StoreId);

      await endpoint.Send(new CreateProductMessage(product.ToDto()));
      await service.SaveChangesAsync();

      return TypedResults.Created($"/{Route}/{product?.Id}", product?.ToResponse());
    }
  }
}
