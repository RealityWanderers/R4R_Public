using UnityEngine;
using TMPro; 

public class PlayerUISpeed : MonoBehaviour
{
    [Header("Refs")]
    private PlayerComponentManager cM;
    private PlayerPassivesManager pP; 
    private PlayerSoftSpeedCap softSpeedCap;
    private TextMeshProUGUI speedText; 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cM = PlayerComponentManager.Instance;
        pP = PlayerPassivesManager.Instance;
        softSpeedCap = pP.GetPassive<PlayerSoftSpeedCap>(); 
        speedText = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        float speed = softSpeedCap.GetHorizontalSpeed();
        speed = Mathf.Round(speed);
        speedText.SetText(speed.ToString()); 
    }
}
