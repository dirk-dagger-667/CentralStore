using Microsoft.EntityFrameworkCore.Storage;

namespace LocalStore.Shared
{
  public interface IService
  {
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task<int> SaveChangesAsync();
  }
}
