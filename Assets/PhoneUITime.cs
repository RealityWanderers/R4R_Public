using UnityEngine;
using TMPro; 

public class PhoneUITime : MonoBehaviour
{
    private PlayerPhone playerPhone;
    public TextMeshProUGUI text_Time; 

    private void Awake()
    {
        playerPhone = PlayerPhone.Instance;
    }

    private void OnEnable()
    {
        UpdateTimeIfPhoneIsActive();
        InvokeRepeating(nameof(UpdateTimeIfPhoneIsActive), 0f, 60f); // every 60 seconds
        //Debug.Log("UpdateTime"); 
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(UpdateTimeIfPhoneIsActive));
        //Debug.Log("StopUpdateTime");
    }

    private void UpdateTimeIfPhoneIsActive()
    {
        if (!playerPhone.isPhoneActive) return;

        string currentTime = System.DateTime.Now.ToString("HH:mm");
        //Debug.Log("UpdatingTime");
        //Debug.Log(currentTime); 
        text_Time.SetText(currentTime);
    }
}
