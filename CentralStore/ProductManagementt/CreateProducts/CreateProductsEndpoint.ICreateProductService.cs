using CentralStore.Domain;
using CentralStore.Shared;
using CentralStore.Shared.Dtos;

namespace CentralStore.ProductManagement.CreateProducts
{
  public interface ICreateProductService : IService
  {
    Product CreateProduct(ProductDto dto, Guid storeId);
    Task<int> RemoveProductAsync(Guid id);
  }
}
