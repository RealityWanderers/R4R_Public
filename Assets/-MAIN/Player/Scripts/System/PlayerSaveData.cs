using Sirenix.OdinInspector;
using UnityEngine;
using RootMotion.FinalIK;

[DefaultExecutionOrder(-20)]
public class PlayerSaveData : MonoBehaviour
{
    private const string HeightKey = "PlayerHeight";
    public const string TurnTypeKey = "TurnType";
    public enum TurnType { SnapTurn, SmoothTurn}
    [ReadOnly] public TurnType turnType = TurnType.SnapTurn;
    private const string SnapTurnAngleKey = "SnapTurnAngle";
    private const string SmoothTurnSpeedKey = "SmoothTurnSpeed";
    private const string CalibrationScaleKey = "CalibrationScale";

    public static PlayerSaveData Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Prevent duplicates
        }
    }

    public void SaveCalibrationData(CalibrationData data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("CalibrationData", json);
        PlayerPrefs.Save();

        //Debug.Log("Calibration Data Saved: " + json);
    }

    public CalibrationData LoadCalibrationData()
    {
        if (PlayerPrefs.HasKey("CalibrationData"))
        {
            string json = PlayerPrefs.GetString("CalibrationData");

            // Check if JSON is empty or invalid before parsing
            if (string.IsNullOrWhiteSpace(json))
            {
                //Debug.LogWarning("CalibrationData JSON is empty or invalid!");
                return null;
            }

            CalibrationData data = null;

            try
            {
                data = JsonUtility.FromJson<CalibrationData>(json);

                if (data == null)
                {
                    //Debug.LogWarning("Failed to deserialize CalibrationData! The JSON structure might be incorrect.");
                    return null;
                }

                //Debug.Log("Calibration Data Loaded: " + JsonUtility.ToJson(data, true));
                return data;
            }
            catch (System.Exception ex)
            {
                //Debug.Log($"Error while loading CalibrationData: {ex.Message}");
            }
        }

        //Debug.LogWarning("No CalibrationData found in PlayerPrefs.");
        return null;
    }


    public void ClearPlayerCalibrationData()
    {
        PlayerPrefs.SetString("CalibrationData", "NULL"); // Special flag for "null" data
        PlayerPrefs.Save();
        //Debug.Log("CalibrationData Cleared Successfully");
    }

    //NOT IMPLEMENTED ATM. 
    public void SaveHeightData(float playerHeight)
    {
        playerHeight = Mathf.Round(playerHeight * 100f) / 100f; //Rounds playerheight to 2 decimals. 
        PlayerPrefs.SetFloat(HeightKey, playerHeight);
        PlayerPrefs.Save();
        //Debug.Log("Calibrated Height: " + playerHeight);
    }
    public float LoadHeightData()
    {
        return PlayerPrefs.GetFloat(HeightKey, 0); // If the key doesn't exist, returns 0 by default
    }

    public void SaveCalibrationScale(float calibrationScale)
    {
        calibrationScale = Mathf.Round(calibrationScale * 100f) / 100f; //Rounds playerheight to 2 decimals. 
        PlayerPrefs.SetFloat(CalibrationScaleKey, calibrationScale);
        PlayerPrefs.Save();
    }
    public float LoadCalibrationScale()
    {
        return PlayerPrefs.GetFloat(CalibrationScaleKey, 0); // If the key doesn't exist, returns 0 by default
    }

    public void EditHeight(float editAmount)
    {
        float playerHeight = PlayerPrefs.GetFloat(HeightKey);
        playerHeight += editAmount;
        if (playerHeight > 2.5f)
        {
            playerHeight = 2.5f;
            //Debug.Log("Playerheight capped to: " + playerHeight);
        }
        if (playerHeight < 0.5f)
        {
            playerHeight = 0.5f;
            //Debug.Log("Playerheight capped to: " + playerHeight);
        }
        PlayerPrefs.SetFloat(HeightKey, playerHeight);
        PlayerPrefs.Save();
        //Debug.Log("Calibrated Height: " + playerHeight);
    }

    [Button]
    public void TurningTypeSnapTurn()
    {
        PlayerPrefs.SetInt(TurnTypeKey, (int)TurnType.SnapTurn);
        PlayerPrefs.Save();
        //Debug.Log("Turn Type: " + 0);
    }

    [Button]
    public void TurningTypeSmoothTurn()
    {
        PlayerPrefs.SetInt(TurnTypeKey, (int)TurnType.SmoothTurn);
        PlayerPrefs.Save();
        //Debug.Log("Turn Type: " + 1);
    }

    public void SetDefaultSnapTurnAngle(int defaultAmount)
    {
        PlayerPrefs.SetInt(SnapTurnAngleKey, defaultAmount);
        PlayerPrefs.Save();
    }

    public void EditSnapTurnAngle(int editAmount)
    {
        int snapTurnAngle = PlayerPrefs.GetInt(SnapTurnAngleKey);
        snapTurnAngle += editAmount;
        if (snapTurnAngle > 90)
        {
            snapTurnAngle = 90;
            //Debug.Log("SmoothTurn capped to: " + snapTurnAngle);
        }
        if (snapTurnAngle < 15)
        {
            snapTurnAngle = 15;
            //Debug.Log("SmoothTurn capped to: " + snapTurnAngle);
        }
        PlayerPrefs.SetInt(SnapTurnAngleKey, snapTurnAngle);
        PlayerPrefs.Save();
        //Debug.Log("Set SnapTurnAngle: " + snapTurnAngle);
    }

    public void SetDefaultSmoothTurnSpeed(float defaultAmount)
    {
        defaultAmount = Mathf.Round(defaultAmount * 10f) / 10f; //Rounds to 1 decimal. ; 
        PlayerPrefs.SetFloat(SmoothTurnSpeedKey, defaultAmount);
        PlayerPrefs.Save();
    }

    public void EditSmoothTurnSpeed(float editAmount)
    {
        float smoothTurnSpeed = PlayerPrefs.GetFloat(SmoothTurnSpeedKey);
        smoothTurnSpeed += editAmount;
        smoothTurnSpeed = Mathf.Round(smoothTurnSpeed * 10f) / 10f; //Rounds to 1 decimal. 
        if (smoothTurnSpeed > 1.5f)
        {
            smoothTurnSpeed = 1.5f;
            //Debug.Log("SmoothTurn capped to: " + smoothTurnSpeed);
        }
        if (smoothTurnSpeed < 0.5f)
        {
            smoothTurnSpeed = 0.5f;
            //Debug.Log("SmoothTurn capped to: " + smoothTurnSpeed);
        }
        PlayerPrefs.SetFloat(SmoothTurnSpeedKey, smoothTurnSpeed);
        PlayerPrefs.Save();
        //Debug.Log("Set SmoothTurnSpeed: " + smoothTurnSpeed);
    }
}
