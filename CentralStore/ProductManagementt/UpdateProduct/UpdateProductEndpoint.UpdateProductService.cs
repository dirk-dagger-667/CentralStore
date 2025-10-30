using CentralStore.Domain;
using CentralStore.Shared;
using Microsoft.EntityFrameworkCore;
using CentralStore.Shared.Dtos;

namespace CentralStore.ProductManagement.UpdateProduct
{
  public class UpdateProductService : ServiceBase, IUpdateProductService
  {
    private CentralStoreDbContext _dbContext;

    public UpdateProductService(CentralStoreDbContext dbContext) : base(dbContext)
    {
      _dbContext = dbContext;
    }

    public async Task<bool> IsConflictAsync(Guid id, Guid concurrencyToken)
      => await _dbContext.Products
        .AsNoTracking()
        .AnyAsync(p => p.Id == id
        && p.ConcurrencyToken != concurrencyToken);

    public async Task<Product?> GetById(Guid id)
      => await _dbContext.Products
        .AsNoTracking()
      .SingleOrDefaultAsync(p => p.Id == id);

    public async Task<int> UpdateProductApiAsync(ProductDto dto, Guid storeId)
      => await _dbContext.Products
        .Where(p => p.Id == dto.Id)
        .ExecuteUpdateAsync(setters => setters
        .SetProperty(p => p.Name, dto.Name)
        .SetProperty(p => p.Description, dto.Description)
        .SetProperty(p => p.Price, dto.Price)
        .SetProperty(p => p.MinPrice, dto.MinPrice)
        .SetProperty(p => p.UpdatedAt, DateTime.UtcNow)
        .SetProperty(p => p.StoreId, storeId)
        .SetProperty(p => p.ConcurrencyToken, Guid.NewGuid()));

    public async Task<int> UpdateProductMqAsync(ProductDto dto, Guid storeId)
      => await _dbContext.Products
        .Where(p => p.Id == dto.Id)
        .ExecuteUpdateAsync(setters => setters
        .SetProperty(p => p.Name, dto.Name)
        .SetProperty(p => p.Description, dto.Description)
        .SetProperty(p => p.Price, dto.Price)
        .SetProperty(p => p.MinPrice, dto.MinPrice)
        .SetProperty(p => p.CreatedAt, dto.CreatedAt)
        .SetProperty(p => p.UpdatedAt, dto.UpdatedAt)
        .SetProperty(p => p.StoreId, storeId)
        .SetProperty(p => p.ConcurrencyToken, dto.ConcurrencyToken));
  }
}