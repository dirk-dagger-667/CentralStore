using CentralStore.LocalStore.Domain;
using CentralStore.LocalStore.Shared;
using CentralStore.Shared.Dtos;

namespace CentralStore.LocalStore.ProductManagement.CreateProduct
{
  public interface ICreateProductService : IService
  {
    Product CreateProduct(ProductDto request);
    Task<int> RemoveProductAsync(Guid id);
  }
}
