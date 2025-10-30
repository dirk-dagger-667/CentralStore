using LocalStore.Domain;
using LocalStore.Shared;
using CentralStore.Shared.Dtos;

namespace LocalStore.ProductManagement.CreateProduct
{
  public interface ICreateProductService : IService
  {
    Product CreateProduct(ProductDto request);
    Task<int> RemoveProductAsync(Guid id);
  }
}
