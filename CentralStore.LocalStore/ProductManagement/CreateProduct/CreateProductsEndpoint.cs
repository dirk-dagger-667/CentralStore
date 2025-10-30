using LocalStore.ProductManagent.Filters;
using LocalStore.Shared;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using CentralStore.Shared.Messages;

namespace LocalStore.ProductManagement.CreateProduct
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
      var product = service.CreateProduct(request.ToDto());

      var storeId = config[options.Value.StoreIdConfigKey];

      var queueName = $"{options.Value.CentralStoreQueueName}";
      var endpoint = await sendEndpointProvider
            .GetSendEndpoint(new Uri($"rabbitmq://{config["RabbitMQ:Host"]}/{queueName}"));

      await endpoint.Send(new CreateProductMessage(product.ToDto()),
        mContext => mContext.Headers.Set(options.Value.StoreIdHeaderKey, storeId));

      await service.SaveChangesAsync();

      return TypedResults.Created($"/{Route}/{product?.Id}", product?.ToResponse());
    }
  }
}
