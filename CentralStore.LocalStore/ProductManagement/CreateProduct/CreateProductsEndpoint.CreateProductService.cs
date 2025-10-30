using CentralStore.LocalStore.Domain;
using CentralStore.LocalStore.Shared;
using Microsoft.EntityFrameworkCore;
using CentralStore.Shared.Dtos;

namespace CentralStore.LocalStore.ProductManagement.CreateProduct
{
  public class CreateProductsService : ServiceBase, ICreateProductService
  {
    private LocalStoreDbContext _dbContext;

    public CreateProductsService(LocalStoreDbContext dbContext) : base(dbContext) 
    {
      _dbContext = dbContext;
    }

    public Product CreateProduct(ProductDto dto)
    {
      var product = dto.ToEntity();
      _dbContext.Products.Add(product);

      return product;
    }

    public async Task<int> RemoveProductAsync(Guid id)
      => await _dbContext.Products.Where(p => p.Id == id)
      .ExecuteDeleteAsync();
  }
}
