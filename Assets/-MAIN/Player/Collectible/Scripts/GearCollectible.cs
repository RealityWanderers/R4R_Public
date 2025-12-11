using UnityEngine;

public class GearCollectible : CollectibleBase
{
    public override void Collect()
    {
        base.Collect();
    }

    public override void OnCollectedCallback()
    {
        base.OnCollectedCallback(); // Call the base method to retain default behavior
        //Place to add special Logic. 
    }
}