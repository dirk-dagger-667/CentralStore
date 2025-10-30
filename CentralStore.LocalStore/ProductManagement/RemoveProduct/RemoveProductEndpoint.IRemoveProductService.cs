using CentralStore.LocalStore.Domain;
using CentralStore.LocalStore.Shared;
using CentralStore.Shared.Dtos;

namespace CentralStore.LocalStore.ProductManagement.RemoveProduct
{
  public interface IRemoveProductService : IService
  {
    Task<Product?> GetProductAsync(RemoveProductRequest request);
    Task<int> RemoveProductAsync(Guid productId);
    Task<Product> CreateProductAsync(ProductDto dto);
  }
}
