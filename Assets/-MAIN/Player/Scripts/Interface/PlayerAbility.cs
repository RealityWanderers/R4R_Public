using UnityEngine;

public abstract class PlayerAbility : MonoBehaviour
{
    public virtual void DisableAbility()
    {
        this.enabled = false;
    }

    public virtual void EnableAbility()
    {
        this.enabled = true;
    }

    public virtual void ResetAbility()
    {
        this.StopAllCoroutines();
    }
}
