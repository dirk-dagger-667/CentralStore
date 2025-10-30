using CentralStore.Shared.Dtos;
using CentralStore.Shared.Messages;

namespace LocalStore.ProductManagement.RemoveProduct
{
  public record RemoveProductRequest(Guid Id,
    Guid ConcurrencyToken);

  public static class RemoveProductMappings
  {
    public static RemoveProductMessage ToMessage(this ProductDto productDto)
      => new RemoveProductMessage(PreviousState: productDto);
  }
}
