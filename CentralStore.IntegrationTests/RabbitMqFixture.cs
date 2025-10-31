using Testcontainers.RabbitMq;

namespace CentralStore.IntegrationTests
{
  public class RabbitMqFixture : IAsyncLifetime
  {
    private RabbitMqContainer _rabbitMqContainer = null!;

    public string Host => _rabbitMqContainer.Hostname;
    public int Port => _rabbitMqContainer.GetMappedPublicPort(5672);

    public string RabbitMqConnectionString => _rabbitMqContainer.GetConnectionString();

    public async Task InitializeAsync()
    {
      _rabbitMqContainer = new RabbitMqBuilder()
          .WithImage("rabbitmq:3-management")
          .WithPortBinding(5672, 5672) // AMQP
          .WithPortBinding(15672, 15672) // Management UI (optional)
          .WithUsername("guest")
          .WithPassword("guest")// Default creds
          .Build();
      await _rabbitMqContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
      await _rabbitMqContainer.DisposeAsync();
    }
  }
}
