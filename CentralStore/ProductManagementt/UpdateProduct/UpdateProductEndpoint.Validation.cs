using FluentValidation;

namespace CentralStore.ProductManagement.UpdateProduct
{
  public class UpdateEndpointValidator : AbstractValidator<UpdateProductRequest>
  {
    public UpdateEndpointValidator()
    {
      var storeIds = new List<Guid>()
      {
        new Guid("1af34956-9cd2-45c3-ac5d-be939c6d6e35"),
        new Guid("10861dee-7536-408b-a97d-fdb477d46b1c")
      };

      RuleFor(p => p.Name).NotEmpty().MaximumLength(100);
      RuleFor(p => p.Description).NotEmpty().MaximumLength(500);
      RuleFor(p => p.Price).GreaterThanOrEqualTo(p => p.MinPrice);
      RuleFor(p => p.MinPrice).GreaterThanOrEqualTo(0);
      RuleFor(p => p.StoreId).Must(storeId => storeIds.Contains(storeId));
    }
  }
}
