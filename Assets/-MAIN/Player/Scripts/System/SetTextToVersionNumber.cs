using UnityEngine;
using TMPro;

public class SetTextToVersionNumber : MonoBehaviour
{
    private TextMeshProUGUI versionText;

    void Start()
    {
        if (TryGetComponent<TextMeshProUGUI>(out versionText))
        {
            versionText.SetText("V: " + Application.version);
        }
        else
        {
            Debug.Log("No version textmeshpro found");
        }
    }
}
