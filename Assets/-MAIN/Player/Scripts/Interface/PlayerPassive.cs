using UnityEngine;

public abstract class PlayerPassive : MonoBehaviour
{
    public virtual void DisablePassive()
    {
        this.enabled = false;
    }

    public virtual void EnablePassive()
    {
        this.enabled = true;
    }

    public virtual void ResetPassive()
    {
        this.StopAllCoroutines();
    }
}
