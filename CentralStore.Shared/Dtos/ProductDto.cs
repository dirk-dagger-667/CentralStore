namespace CentralStore.Shared.Dtos
{
  public record ProductDto(Guid Id,
  string Name,
  string Description,
  decimal Price,
  decimal MinPrice,
  DateTime CreatedAt,
  DateTime UpdatedAt,
  Guid ConcurrencyToken);
}
