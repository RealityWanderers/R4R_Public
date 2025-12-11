using RootMotion.Demos;
using RootMotion.FinalIK;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIKCalibration : MonoBehaviour
{
    [Header("Settings")]
    public bool disablePlayerMovement;
    public bool autoLoadCalibrationData; 

    [Header("References")]
    public GameObject prefabIK;
    private GameObject currentIKObject;
    private VRIK ikScript;
    private ModularSetToTransform modularSetToTransform;
    private PlayerIKTransformReferences transformReferences; 
    public WaveGenerator handWaveGenerator; 

    [Header("Setup")]
    public Transform playerLocation;
    public PlayerUICalibration playerUICalibration;
    public List<GameObject> playerControllerModel;
    [Header("Head")]
    public Vector3 headsetPositionOffset;
    public Vector3 headsetRotationOfset;
    public Transform centerEyeAnchor;
    public Transform mainCamera;
    [Header("Hands")]
    public Vector3 controller_PositionOffset;
    public Vector3 controller_RotationOffset;
    public Transform leftHandAnchor;
    public Transform rightHandAnchor;
    [Header("Scale")]
    public float scaleMultiplier = 1f;

    [Header("Activating Calibration")]
    public float requiredHoldTime;
    [ReadOnly] public float currentHoldTime;
    [ReadOnly] public float calibrationProgress;
    [ReadOnly] public bool canCalibrate;
    [ReadOnly] public bool holdingGrip;

    //Ensures players need to release the grip before starting a new calibration.
    //Otherwise it would instantly start a new calibration when reaching full progress.
    [ReadOnly] public bool gripsReleased;

    [Header("Settings")]
    //public bool loadCalibrationOnStart; //CURRENTLY DISABLED, NEEDS MORE WORK TO WORK WELL, SEEMS TO ACT WEIRD WHEN PLAYER HEIGHT IS SLIGHTLY DIFFERENT.

    [Header("Managers")]
    private PlayerInputManager pI;
    private PlayerComponentManager cM;
    private PlayerAbilityManager pA;
    private PlayerPassivesManager pP; 
    private PlayerSaveData playerSaveData;

    public static PlayerIKCalibration Instance { get; private set; }

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

        cM = PlayerComponentManager.Instance; 
        pI = PlayerInputManager.Instance;
        pA = PlayerAbilityManager.Instance;
        pP = PlayerPassivesManager.Instance; 
        playerSaveData = PlayerSaveData.Instance;
    }

    void Start()
    {
        ResetCalibrationProgress();

        ShowControllerVisual(true); 

        if (disablePlayerMovement)
        {
            Invoke(nameof(DisablePlayerMovement), 0.5f); //Add a small delay otherwise this interrupts the loading of reference causing null refs.
            //Should probably ensure refs are got when all the scripts are enabled rather than on start as start can be skipped possible like here if we have 0 delay.
        }

        if (autoLoadCalibrationData)
        {
            Invoke(nameof(LoadCalibrationData), 1f);
        }

        //if (loadCalibrationOnStart)
        //{
        //    Invoke(nameof(LoadOnStart), 2f); //Prob add a different condition than time here. For example selecting start game.
        //}
    }

    public void DisablePlayerMovement()
    {
        pA.DisableAllAbilities();
        //pP.DisableAllPassives();
    }

    //public void LoadOnStart()
    //{
    //    if (playerSaveData.LoadCalibrationData() != null)
    //    {
    //        Debug.Log("CalibrationData present");
    //        LoadCalibrationData();
    //    }
    //    else
    //    {
    //        Debug.Log("No CalibrationData present");
    //    }
    //}

    void Update()
    {
        if (pI.gripValue_L > 0.7f && pI.gripValue_R > 0.7f)
        {
            holdingGrip = true;
        }
        else
        {
            holdingGrip = false;
            gripsReleased = true;
        }

        if (canCalibrate)
        {
            if (holdingGrip && gripsReleased)
            {
                currentHoldTime += Time.deltaTime;
                calibrationProgress = Mathf.Clamp(currentHoldTime / requiredHoldTime, 0, 1);
                playerUICalibration.UpdateCalibrationProgress(calibrationProgress);

                float exponentialProgress = Mathf.Pow(calibrationProgress, 2.5f);
                pI.playerHaptic_L.SendHapticImpulse(exponentialProgress, 0.1f);
                pI.playerHaptic_R.SendHapticImpulse(exponentialProgress, 0.1f);

                if (currentHoldTime > requiredHoldTime)
                {
                    CreateNewCalibrationData();
                    gripsReleased = false;
                }
            }
            else if (!holdingGrip)
            {
                ResetCalibrationProgress();
            }
        }
    }

    public void DestroyCurrentPlayerIK()
    {
        if (currentIKObject != null) { Destroy(currentIKObject); }
    }

    public void SpawnNewPlayerIK()
    {
        DestroyCurrentPlayerIK(); 
        //As we need a completely fresh instance we don't use object pooling here. 
        currentIKObject = Instantiate(prefabIK);
        if (currentIKObject.TryGetComponent<VRIK>(out ikScript))
        {
            //Has IK and has been set. 
        }
        else
        {
            //Debug.LogError("No Ik Found");
            return;
        }

        if (currentIKObject.TryGetComponent<ModularSetToTransform>(out modularSetToTransform))
        {
            //Has modular transform and has been set. 
            modularSetToTransform.target = cM.transform_MainCamera; 
        }
        else
        {
            //Debug.LogError("No modular transform setter found");
            return;
        }

        if (currentIKObject.TryGetComponent<PlayerIKTransformReferences>(out transformReferences))
        {
            //Has PlayerIKTransformReferences and has set WaveGenerator. 
            handWaveGenerator.SetStartAndEnd(transformReferences.waveGeneratorStart, transformReferences.waveGeneratorEnd);
        }
        else
        {
            //Debug.LogError("No PlayerIKTransformReferences found");
            return;
        }

       
    }

    [Button]
    public void CreateNewCalibrationData()
    {
        SpawnNewPlayerIK(); 
        SetToPlayer_1(); //Split into two parts as we need to disable, set position and in part two enable and set height after data is loaded / calibrated. 
        playerSaveData.ClearPlayerCalibrationData();
        VRIKCalibrator.CalibrationData dataTemp = VRIKCalibrator.Calibrate(false, ikScript, centerEyeAnchor, leftHandAnchor, rightHandAnchor, headsetPositionOffset, headsetRotationOfset, controller_PositionOffset, controller_RotationOffset, scaleMultiplier);
        //Save any data that is not a direct reference to an object in the scene such as (center eye, left and right hand)
        CalibrationData calibrationData = new CalibrationData()
        {
            headsetPositionOffset = headsetPositionOffset,
            headsetRotationOffset = headsetRotationOfset,
            controllerPositionOffset = controller_PositionOffset,  // Example: using left hand
            controllerRotationOffset = controller_RotationOffset,  // Example: using left hand
            scaleMultiplier = scaleMultiplier // Set this based on your needs
        };

        // Save the calibration data
        playerSaveData.SaveCalibrationData(calibrationData);

        //data = VRIKCalibrator.Calibrate(ik, centerEyeAnchor, leftHandAnchor, rightHandAnchor, headsetPositionOffset, headsetRotationOfset, controller_PositionOffset, controller_RotationOffset, scaleMultiplier);
        playerSaveData.SaveHeightData(mainCamera.localPosition.y);

        SetToPlayer_2();
        ShowControllerVisual(false);

        ResetCalibrationProgress();
        playerUICalibration.OnCalibrationComplete();
    }

    [Button]
    public void LoadCalibrationData()
    {
        if (playerSaveData.LoadCalibrationData() == null && playerSaveData.LoadCalibrationScale() != 0)
        {
            //Debug.Log("No CalibrationData to Load!");
            return;
        }

        SpawnNewPlayerIK();
        SetToPlayer_1();
        // Load the calibration data from PlayerPrefs (or from a file)
        CalibrationData data = playerSaveData.LoadCalibrationData();
        //Debug.Log(data.scaleMultiplier);
        // Call Calibrate with the loaded data
        if (data != null)
        {
            VRIKCalibrator.Calibrate(
                true,
                ikScript,
                centerEyeAnchor,
                leftHandAnchor,
                rightHandAnchor,
                data.headsetPositionOffset,
                data.headsetRotationOffset,
                data.controllerPositionOffset,
                data.controllerRotationOffset,
                scaleMultiplier * playerSaveData.LoadCalibrationScale()

                //data.scaleMultiplier
            );
            //Debug.Log(playerSaveData.LoadCalibrationScale()); 
        }
        else
        {
            //Debug.LogWarning("No calibration data found!");
        }

        SetToPlayer_2();
        ShowControllerVisual(false);
        //Debug.Log("Calibration Data loaded");
    }

    public void SetToPlayer_1()
    {
        modularSetToTransform.enabled = false;
        currentIKObject.transform.position = playerLocation.position;
    }

    public void SetToPlayer_2()
    {
        modularSetToTransform.enabled = true;

        float height = playerSaveData.LoadHeightData();
        if (height == 0)
        {
            //Debug.LogError("Height is 0, is the headset tracked? Setting to default height");
            modularSetToTransform.offset.y = -1.65f;
        }
        else
        {
            modularSetToTransform.offset.y = -height;
            //Debug.Log("Height = " + height);
        }
    }

    [Button]
    public void ClearCalibrationData()
    {
        DestroyCurrentPlayerIK(); 
        ShowControllerVisual(true);
        playerSaveData.ClearPlayerCalibrationData();
    }

    public void ShowControllerVisual(bool isActive)
    {
        foreach (var controllerModel in playerControllerModel)
        {
            controllerModel.SetActive(isActive);
        }
    }

    [Button]
    private void ResetCalibrationProgress()
    {
        currentHoldTime = 0;
        calibrationProgress = 0;
        playerUICalibration.UpdateCalibrationProgress(0);
    }
}
