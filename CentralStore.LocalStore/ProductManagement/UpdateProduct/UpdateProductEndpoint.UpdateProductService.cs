using LocalStore.Domain;
using LocalStore.Shared;
using Microsoft.EntityFrameworkCore;
using CentralStore.Shared.Dtos;

namespace LocalStore.ProductManagement.UpdateProduct
{
  public class UpdateProductService : ServiceBase, IUpdateProductService
  {
    private LocalStoreDbContext _dbContext;

    public UpdateProductService(LocalStoreDbContext dbContext) : base(dbContext)
    {
      _dbContext = dbContext;
    }

    public async Task<bool> IsConflictAsync(Guid id, Guid concurrencyToken)
    => await _dbContext.Products
        .AsNoTracking()
        .AnyAsync(p => p.Id == id
        && p.ConcurrencyToken != concurrencyToken);

    public async Task<Product?> GetByIdAsync(Guid id)
    => await _dbContext.Products
        .AsNoTracking()
        .Where(p => p.Id == id)
      .SingleOrDefaultAsync();

    public async Task<int> UpdateProductAsync(ProductDto dto)
      => await _dbContext.Products
        .Where(p => p.Id == dto.Id)
        .ExecuteUpdateAsync(setters => setters
        .SetProperty(p => p.Name, dto.Name)
        .SetProperty(p => p.Description, dto.Description)
        .SetProperty(p => p.Price, dto.Price)
        .SetProperty(p => p.MinPrice, dto.MinPrice)
        .SetProperty(p => p.UpdatedAt, dto.UpdatedAt)
        .SetProperty(p => p.ConcurrencyToken, Guid.NewGuid()));

    public async Task<int> UpdateProductMqAsync(ProductDto dto)
      => await _dbContext.Products
        .Where(p => p.Id == dto.Id)
        .ExecuteUpdateAsync(setters => setters
        .SetProperty(p => p.Name, dto.Name)
        .SetProperty(p => p.Description, dto.Description)
        .SetProperty(p => p.Price, dto.Price)
        .SetProperty(p => p.MinPrice, dto.MinPrice)
        .SetProperty(p => p.CreatedAt, dto.CreatedAt)
        .SetProperty(p => p.UpdatedAt, dto.UpdatedAt)
        .SetProperty(p => p.ConcurrencyToken, dto.ConcurrencyToken));
  }
}
