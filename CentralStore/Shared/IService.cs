using Microsoft.EntityFrameworkCore.Storage;

namespace CentralStore.Shared
{
  public interface IService
  {
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task<int> SaveChangesAsync();
  }
}
