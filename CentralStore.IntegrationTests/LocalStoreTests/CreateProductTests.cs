using CentralStore.Domain;
using CentralStore.IntegrationTests.Shared;
using CentralStore.LocalStore.Domain;
using CentralStore.Shared.Messages;
using FluentAssertions;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using CentralStoreProduct = CentralStore.ProductManagement.CreateProduct;
using LocalStoreProduct = CentralStore.LocalStore.ProductManagement.CreateProduct;

namespace CentralStore.IntegrationTests.LocalStoreTests
{
  public class CreateProductTests : IClassFixture<RabbitMqFixture>
  {
    private CentralStoreApi _centralStoreApi;
    private LocalStoreApi _localStoreApi;
    private readonly HttpClient _localStoreClient;

    public CreateProductTests(RabbitMqFixture rabbitMq)
    {
      Environment.SetEnvironmentVariable("STORE_ID", TestConstants.StoreId);
      _centralStoreApi = new CentralStoreApi(rabbitMq);
      _localStoreApi = new LocalStoreApi(rabbitMq);

      _localStoreClient = _localStoreApi.CreateClient();
    }

    // 1. LocalStore receives create request
    // 2. Saves data
    // 3. Sends data to CentralStore
    // 4. Adds Product
    [Fact]
    public async Task CreatePruduct_LocalStore_Success()
    {
      var testHarness = _centralStoreApi.Services.GetTestHarness();

      var createRequest = new LocalStoreProduct.CreateProductRequest(
        Name: "TestCreate",
        Description: "TestCreate",
        Price: 10m,
        MinPrice: 10m);

      // Send create request
      var createHttpResponse = await _localStoreClient.PostAsJsonAsync("api/products", createRequest);
      createHttpResponse.IsSuccessStatusCode.Should().BeTrue();
      var createResponse = await createHttpResponse.Content.ReadFromJsonAsync<LocalStoreProduct.CreateProductResponse>();
      // Assert the request has passed, save to db and should have sent CreateProductMessage to LocalStore
      createResponse.Should().NotBeNull();
      createResponse.Name.Should().Be(createRequest.Name);

      // Assert CreateProduct message was consumed
      var consumerHarness = testHarness.GetConsumerHarness<CentralStoreProduct.CreateProductConsumer>();
      (await consumerHarness.Consumed.Any<CreateProductMessage>()).Should().BeTrue();

      // Assert Product was created on LocalStore
      using (var scope = _localStoreApi.Services.CreateAsyncScope())
      {
        var dbContextCentral = scope.ServiceProvider.GetRequiredService<LocalStoreDbContext>();
        var product = await dbContextCentral.Products.FindAsync(createResponse?.Id);

        product.Should().NotBeNull();
        product.Id.Should().Be(createResponse!.Id);
      }

      // Assert Product was created on CentralStore
      using (var scope = _centralStoreApi.Services.CreateAsyncScope())
      {
        var dbContextCentral = scope.ServiceProvider.GetRequiredService<CentralStoreDbContext>();
        var product = await dbContextCentral.Products.FindAsync(createResponse?.Id);

        product.Should().NotBeNull();
        product.Id.Should().Be(createResponse!.Id);
        Guid.TryParse(TestConstants.StoreId, out Guid storeId);
        product.StoreId.Should().Be(storeId);
      }
    }
  }
}
