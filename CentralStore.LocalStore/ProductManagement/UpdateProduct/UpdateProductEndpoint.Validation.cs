using FluentValidation;

namespace CentralStore.LocalStore.ProductManagement.UpdateProduct
{
  public class UpdateEndpointValidator : AbstractValidator<UpdateProductRequest>
  {
    public UpdateEndpointValidator()
    {
      RuleFor(p => p.Name).NotEmpty().MaximumLength(100);
      RuleFor(p => p.Description).NotEmpty().MaximumLength(500);
      RuleFor(p => p.Price).GreaterThanOrEqualTo(p => p.MinPrice);
      RuleFor(p => p.MinPrice).GreaterThanOrEqualTo(0);
    }
  }
}
