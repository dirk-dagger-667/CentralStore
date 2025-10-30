using CentralStore.Shared.Dtos;
using CentralStore.Shared.Messages;
using Microsoft.AspNetCore.Mvc;

namespace CentralStore.ProductManagement.RemoveProduct
{
  public record RemoveProductRequest([FromRoute] Guid Id,
    Guid ConcurrencyToken,
    Guid StoreId);

  public static class RemoveProductMappings
  {
    public static RemoveProductMessage ToMessage(this ProductDto productDto)
      => new RemoveProductMessage(PreviousState: productDto);
  }
}
