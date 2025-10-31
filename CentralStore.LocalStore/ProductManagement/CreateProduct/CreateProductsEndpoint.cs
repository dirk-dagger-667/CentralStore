using CentralStore.LocalStore.ProductManagent.Filters;
using CentralStore.LocalStore.Shared;
using CentralStore.Shared.Messages;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CentralStore.LocalStore.ProductManagement.CreateProduct
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
      IOptions<QueueMetadata> options,
      IConfiguration config,
      ICreateProductService service,
      IMassTransitSendResolver mtResolver)
    {
      var product = service.CreateProduct(request.ToDto());
      var storeId = config[options.Value.StoreIdConfigKey];
      var endpoint = await mtResolver.GetSendEndpoint();

      await endpoint.Send(new CreateProductMessage(product.ToDto()),
        mContext => mContext.Headers.Set(options.Value.StoreIdHeaderKey, storeId));

      await service.SaveChangesAsync();

      return TypedResults.Created($"/{Route}/{product?.Id}", product?.ToResponse());
    }
  }
}
