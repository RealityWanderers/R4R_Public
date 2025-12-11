using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerIKScale : MonoBehaviour
{
    [Header("Settings")]
    public float scaleFactor = 1.87f;
    public float heightCorrection = 0.075f;
    public bool heightBased; 

    [Header("Refs")]
    public Transform playerIKRoot;
    public PlayerIKPosition playerIKPosition; 
    private const string HeightKey = "PlayerHeight";

    void Start()
    {
        float savedHeight = PlayerPrefs.GetFloat(HeightKey, 1.75f); // Default to 1.75m if no data
        CalibratePlayerScale(savedHeight); 
    }

    public void CalibratePlayerScale(float playerHeight)
    {
        if (heightBased) //height based 
        {
            playerHeight = Mathf.Clamp(playerHeight, 0.5f, 2.5f); //Kinda redundant as it should always scale properly and there is no gameplay tied to IK scale.
            float playerIKScale = playerHeight / scaleFactor;
            playerIKRoot.localScale = new Vector3(playerIKScale, playerIKScale, playerIKScale);
            playerIKPosition.offset.y = -playerHeight + heightCorrection; //Small correction 
        }
        else //arm based
        {

        }

        //Debug.Log("Height = " + playerHeight + "IK Scale = " + playerIKRoot.localScale); 
    }
}
