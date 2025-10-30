using CentralStore.Shared.Dtos;

namespace CentralStore.Shared.Messages
{
  public record CreateProductMessage(ProductDto CurrentState);
  public record RemoveProductMessage(ProductDto PreviousState);
  public record UpdateProductMessage(ProductDto PreviousState, ProductDto CurrentState);

  public record CreationFailedMessage(Guid ProductId);
  public record RemovalFailedMessage(ProductDto PreviousState);
  public record UpdateFailedMessage(ProductDto PreviousState);
}
