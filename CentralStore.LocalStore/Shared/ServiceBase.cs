using CentralStore.LocalStore.Domain;
using Microsoft.EntityFrameworkCore.Storage;

namespace CentralStore.LocalStore.Shared
{
  public abstract class ServiceBase(LocalStoreDbContext dbContext) : IService
  {
    public async Task<IDbContextTransaction> BeginTransactionAsync()
      => await dbContext.Database.BeginTransactionAsync();

    public async Task<int> SaveChangesAsync()
      => await dbContext.SaveChangesAsync();
  }
}
