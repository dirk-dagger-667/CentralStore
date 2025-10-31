using CentralStore.Domain;
using CentralStore.IntegrationTests.Shared;
using CentralStore.LocalStore.Domain;
using CentralStore.Shared.Messages;
using FluentAssertions;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using LocalStoreProduct = CentralStore.LocalStore.ProductManagement.CreateProduct;
using CentralStoreProduct = CentralStore.ProductManagement.CreateProduct;

namespace CentralStore.IntegrationTests.CentralStoreTests
{
  public class CreateProductTests : IClassFixture<RabbitMqFixture>, IAsyncLifetime
  {
    private CentralStoreApi _centralStoreApi;
    private LocalStoreApi _localStoreApi;
    private readonly HttpClient _centralStoreClient;

    public CreateProductTests(RabbitMqFixture rabbitMq)
    {
      _centralStoreApi = new CentralStoreApi(rabbitMq);
      _localStoreApi = new LocalStoreApi(rabbitMq);
      _centralStoreClient = _centralStoreApi.CreateClient();
    }

    public async Task InitializeAsync() { }

    public async Task DisposeAsync()
    {
      await _centralStoreApi.DisposeAsync();
      _centralStoreClient.Dispose();
    }

    // 1. CentralStore receives create request
    // 2. Adds Product
    // 3. Sends data to LocalStore
    // 4. Adds Product
    [Fact]
    public async Task CreatePruduct_CentralStore_Success()
    {
      var testHarness = _localStoreApi.Services.GetTestHarness();

      Guid.TryParse(TestConstants.StoreId, out Guid storeId);

      var createRequest = new CentralStoreProduct.CreateProductRequest(
        Name: "TestCreate",
        Description: "TestCreate",
        Price: 10m,
        MinPrice: 10m,
        StoreId: storeId);

      // Send create request
      var createHttpResponse = await _centralStoreClient.PostAsJsonAsync("api/products", createRequest);
      createHttpResponse.IsSuccessStatusCode.Should().BeTrue();
      var createResponse = await createHttpResponse.Content.ReadFromJsonAsync<CentralStoreProduct.CreateProductResponse>();
      // Assert the request has passed, save to db and should have sent CreateProductMessage to LocalStore
      createResponse.Should().NotBeNull();
      createResponse.StoreId.Should().Be(storeId);

      // Assert CreateProduct message was consumed
      var consumerHarness = testHarness.GetConsumerHarness<LocalStoreProduct.CreateProductConsumer>();
      (await consumerHarness.Consumed.Any<CreateProductMessage>()).Should().BeTrue();

      // Assert Product was created on CentralStore
      using (var scope = _centralStoreApi.Services.CreateAsyncScope())
      {
        var dbContextCentral = scope.ServiceProvider.GetRequiredService<CentralStoreDbContext>();
        var product = await dbContextCentral.Products.FindAsync(createResponse?.Id);

        product.Should().NotBeNull();
        product.Id.Should().Be(createResponse!.Id);
      }

      // Assert Product was created on LocalStore
      using (var scope = _localStoreApi.Services.CreateAsyncScope())
      {
        var dbContextCentral = scope.ServiceProvider.GetRequiredService<LocalStoreDbContext>();
        var product = await dbContextCentral.Products.FindAsync(createResponse?.Id);

        product.Should().NotBeNull();
        product.Id.Should().Be(createResponse!.Id);
      }
    }
  }
}
