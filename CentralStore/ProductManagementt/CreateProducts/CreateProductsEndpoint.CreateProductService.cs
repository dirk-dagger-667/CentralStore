using CentralStore.Domain;
using CentralStore.Shared;
using Microsoft.EntityFrameworkCore;
using CentralStore.Shared.Dtos;

namespace CentralStore.ProductManagementt.CreateProducts
{
  public class CreateProductsService : ServiceBase, ICreateProductService
  {
    private CentralStoreDbContext _dbContext;

    public CreateProductsService(CentralStoreDbContext dbContext) : base(dbContext) 
    {
      _dbContext = dbContext;
    }

    public Product CreateProduct(ProductDto dto, Guid storeId)
    {
      var product = dto.ToEntity();
      product.StoreId = storeId;
      _dbContext.Products.Add(product);

      return product;
    }

    public async Task<int> RemoveProductAsync(Guid id)
      => await _dbContext.Products.Where(p => p.Id == id)
      .ExecuteDeleteAsync();
  }
}
