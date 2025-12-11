using UnityEngine;

public class DisableGameobjectOnStart : MonoBehaviour
{
    void Start()
    {
        gameObject.SetActive(false);
    }
}
