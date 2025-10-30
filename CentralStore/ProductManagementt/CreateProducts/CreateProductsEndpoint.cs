using CentralStore.ProductManagent.Filters;
using CentralStore.Shared;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using CentralStore.Shared.Messages;
using CentralStore.ProductManagementt.CreateProducts;

namespace CentralStore.ProductManagementt.CreateProducts
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
      NotFound,
        ValidationProblem>> Handle(
      [FromBody] CreateProductRequest request,
      ISendEndpointProvider sendEndpointProvider,
      IOptions<QueueMetadata> options,
      IConfiguration config,
      ICreateProductService service)
    {
      var product = service.CreateProduct(request.ToDto(), request.StoreId);

      var queueName = $"{options.Value.LocalStoreQueueName}{request.StoreId}";
      var endpoint = await sendEndpointProvider
            .GetSendEndpoint(new Uri($"rabbitmq://{config["RabbitMQ:Host"]}/{queueName}"));

      await endpoint.Send(new CreateProductMessage(product.ToDto()));
      await service.SaveChangesAsync();

      return TypedResults.Created($"/{Route}/{product?.Id}", product?.ToResponse());
    }
  }
}
