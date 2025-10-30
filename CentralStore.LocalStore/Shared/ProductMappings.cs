using CentralStore.LocalStore.Domain;
using CentralStore.Shared.Dtos;

namespace CentralStore.LocalStore.Shared
{
  public static class ProductMappings
  {
    public static ProductDto ToDto(this Product product)
      => new ProductDto(Id: product.Id,
        Name: product.Name,
        Description: product.Description,
        Price: product.Price,
        MinPrice: product.MinPrice,
        CreatedAt: product.CreatedAt,
        UpdatedAt: product.UpdatedAt,
        ConcurrencyToken: product.ConcurrencyToken);

    public static Product ToEntity(this ProductDto productDto)
      => new Product()
      {
        Id = productDto.Id,
        Name = productDto.Name,
        Description = productDto.Description,
        Price = productDto.Price,
        MinPrice = productDto.MinPrice,
        CreatedAt = productDto.CreatedAt,
        UpdatedAt = productDto.UpdatedAt,
        ConcurrencyToken = productDto.ConcurrencyToken
      };
  }
}
