using CentralStore.Domain;
using Microsoft.EntityFrameworkCore.Storage;

namespace CentralStore.Shared
{
  public abstract class ServiceBase(CentralStoreDbContext dbContext) : IService
  {
    public async Task<IDbContextTransaction> BeginTransactionAsync()
      => await dbContext.Database.BeginTransactionAsync();

    public async Task<int> SaveChangesAsync()
      => await dbContext.SaveChangesAsync();
  }
}
