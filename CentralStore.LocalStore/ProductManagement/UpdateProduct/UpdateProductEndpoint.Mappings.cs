using CentralStore.Shared.Dtos;
using CentralStore.Shared.Messages;

namespace LocalStore.ProductManagement.UpdateProduct
{
  public record UpdateProductRequest(Guid Id,
  string Name,
  string Description,
  decimal Price,
  decimal MinPrice,
  Guid ConcurrencyToken);

  public static class UpdateProductMappings
  {
    public static ProductDto ToDto(this UpdateProductRequest request)
      => new ProductDto(
        Id: request.Id,
        Name: request.Name,
        Description: request.Description,
        Price: request.Price,
        MinPrice: request.MinPrice,
        CreatedAt: DateTime.UtcNow, // or fetch from DB if updating
        UpdatedAt: DateTime.UtcNow,
        ConcurrencyToken: request.ConcurrencyToken);

    public static UpdateProductMessage ToMessage(ProductDto previousState, ProductDto currentState)
      => new UpdateProductMessage(previousState, currentState);
  }
}
