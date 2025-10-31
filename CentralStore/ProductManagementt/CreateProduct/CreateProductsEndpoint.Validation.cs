using FluentValidation;

namespace CentralStore.ProductManagement.CreateProduct
{
  public class CreateEdnpointValidator: AbstractValidator<CreateProductRequest>
  {
    public CreateEdnpointValidator()
    {
      RuleFor(p => p.Name).NotEmpty().MaximumLength(100);
      RuleFor(p => p.Description).NotEmpty().MaximumLength(500);
      RuleFor(p => p.Price).GreaterThanOrEqualTo(p => p.MinPrice);
      RuleFor(p => p.MinPrice).GreaterThanOrEqualTo(0);
    }
  }
}
