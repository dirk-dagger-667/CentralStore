using CentralStore.Shared.Dtos;

namespace CentralStore.Domain
{
  public class Product : ProductBase
  {
    public Guid StoreId { get; set; }
  }
}
