namespace CentralStore.LocalStore.Shared
{
  public class QueueMetadata
  {
    public const string SectionName = nameof(QueueMetadata);

    public required string StoreIdHeaderKey { get; set; }
    public required string StoreIdConfigKey { get; set; }
    public required string LocalStoreQueueName { get; set; }
    public required string CentralStoreQueueName { get; set; }
  }
}
