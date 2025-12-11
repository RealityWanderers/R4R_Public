using UnityEngine;

public class DisableOnAndroidBuild : MonoBehaviour
{
    void Awake()
    {
        if (Application.isMobilePlatform)
        {
            gameObject.SetActive(false);
        }
    }
}
