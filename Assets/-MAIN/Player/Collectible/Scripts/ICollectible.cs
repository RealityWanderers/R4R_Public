public interface ICollectible
{
    event System.Action<ICollectible> OnCollected;
    void Collect();
    CollectibleData GetData();
}