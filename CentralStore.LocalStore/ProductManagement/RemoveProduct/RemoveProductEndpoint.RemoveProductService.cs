using CentralStore.LocalStore.Domain;
using CentralStore.LocalStore.Shared;
using Microsoft.EntityFrameworkCore;
using CentralStore.Shared.Dtos;

namespace CentralStore.LocalStore.ProductManagement.RemoveProduct
{
  public class RemoveProductService : ServiceBase, IRemoveProductService
  {
    private readonly LocalStoreDbContext _dbContext;

    public RemoveProductService(LocalStoreDbContext dbContext) : base(dbContext)
    {
      _dbContext = dbContext;
    }

    public async Task<Product?> GetProductAsync(RemoveProductRequest request)
      => await _dbContext.Products
        .AsNoTracking()
        .Where(p => p.Id == request.Id
        && p.ConcurrencyToken == request.ConcurrencyToken)
        .SingleOrDefaultAsync();

    public async Task<int> RemoveProductAsync(Guid productId)
      => await _dbContext.Products
        .Where(p => p.Id == productId)
        .ExecuteDeleteAsync();

    public async Task<Product> CreateProductAsync(ProductDto dto)
    {
      var product = dto.ToEntity();
      _dbContext.Products.Add(product);
      await _dbContext.SaveChangesAsync();

      return product;
    }
  }
}
