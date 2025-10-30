using LocalStore.Domain;
using MassTransit;
using CentralStore.Shared.Dtos;
using CentralStore.Shared.Messages;

namespace LocalStore.ProductManagement.CreateProduct
{
  public record CreateProductRequest(
  string Name,
  string Description,
  decimal Price,
  decimal MinPrice);

  public record CreateProductResponse(Guid Id,
    string Name,
    string Description,
    decimal Price,
    decimal MinPrice,
    DateTime CreatedAt,
    DateTime UpdateAt,
    Guid ConcurrencyToken);

  public static class ProductExtensions
  {
    public static CreateProductResponse ToResponse(this Product product)
      => new CreateProductResponse(Id: product.Id,
        Name: product.Name,
        Description: product.Description,
        Price: product.Price,
        MinPrice: product.MinPrice,
        CreatedAt: product.CreatedAt,
        UpdateAt: product.UpdatedAt,
        ConcurrencyToken: product.ConcurrencyToken);

    public static ProductDto ToDto(this CreateProductRequest request)
      => new ProductDto(Id: NewId.NextSequentialGuid(),
        Name: request.Name,
        Description: request.Description,
        Price: request.Price,
        MinPrice: request.MinPrice,
        CreatedAt: DateTime.UtcNow,
        UpdatedAt: DateTime.UtcNow,
        ConcurrencyToken: Guid.NewGuid()
        );

    public static CreateProductMessage ToMessage(this ProductDto productDto)
      => new CreateProductMessage(CurrentState: productDto);
  }
}
