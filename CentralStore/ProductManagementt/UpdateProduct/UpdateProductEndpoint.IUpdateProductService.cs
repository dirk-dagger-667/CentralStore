using CentralStore.Domain;
using CentralStore.Shared;
using CentralStore.Shared.Dtos;

namespace CentralStore.ProductManagement.UpdateProduct
{
  public interface IUpdateProductService : IService
  {
    Task<bool> IsConflictAsync(Guid id, Guid concurrencyToken);
    Task<int> UpdateProductApiAsync(ProductDto dto, Guid storeId);
    Task<int> UpdateProductMqAsync(ProductDto dto, Guid storeId);
    Task<Product?> GetById(Guid id);
  }
}
