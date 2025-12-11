using UnityEngine;

public abstract class PlayerUI : MonoBehaviour
{
    public virtual void DisableUI()
    {
        this.enabled = false;
    }

    public virtual void EnableUI()
    {
        this.enabled = true;
    }

    public virtual void ResetUI()
    {
        this.StopAllCoroutines();

    }
}
