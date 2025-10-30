using Microsoft.EntityFrameworkCore.Storage;

namespace CentralStore.LocalStore.Shared
{
  public interface IService
  {
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task<int> SaveChangesAsync();
  }
}
