using CentralStore.LocalStore.Domain;
using CentralStore.LocalStore.Shared;
using CentralStore.Shared.Dtos;

namespace CentralStore.LocalStore.ProductManagement.UpdateProduct
{
  public interface IUpdateProductService : IService
  {
    Task<int> UpdateProductAsync(ProductDto dto);
    Task<int> UpdateProductMqAsync(ProductDto dto);
    Task<Product?> GetByIdAsync(Guid id);
    Task<bool> IsConflictAsync(Guid id, Guid concurrencyToken);
  }
}
