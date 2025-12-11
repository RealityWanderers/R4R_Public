using UnityEngine;

public class CollectibleBase : MonoBehaviour, ICollectible
{
    public event System.Action<ICollectible> OnCollected;
    [SerializeField] protected CollectibleData data;

    public virtual void Collect()
    {
        OnCollected?.Invoke(this);
        Destroy(gameObject);
    }

    public virtual void OnCollectedCallback()
    {
        //Debug.Log($"Collected a {GetType().Name}!");
    }

    public CollectibleData GetData() => data;
}