using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;

public class ModularRotateObject : MonoBehaviour
{
    [Header("Settings")]
    public Vector3 totalRotation = new Vector3(0, 0, -360); // Total rotation for X, Y, and Z.
    public float rotationSpeed;
    public Ease rotationEaseType;
    public bool onStart;
    private Vector3 startRotation;
    public LoopType loopType = LoopType.Restart; 

    public void Start()
    {
        startRotation = transform.eulerAngles; 

        if (onStart)
        {
            RotateRepeat();
        }    
    }

    private Tween tweenRotation;
    [Button]
    public void RotateRepeat()
    {
        //NOTE: THIS HAS ISSUES WHEN THE OBJECT IS ALREADY ROTATED BY DEFAULT.
        //FOR EXAMPLE 45,0,90. THEN THE TOTALROTATION NEEDS TO BE 45,360,90 FOR EXAMPLE TO ONLY SPIN ON THE Y AXIS.
        //BECAUSE 0,360,0 WOULD INSTEAD ROTATE IT FROM THE 45,0,90 TO 0,360,0 RATHER THAN JUST ADDING THE 360 AND NOT TOUCHING THE X AND Z.
        //COULD FIX THIS LATER IF NEEDED.
        transform.eulerAngles = startRotation; 
        tweenRotation?.Kill();
        tweenRotation = transform.DORotate(totalRotation, 1f / rotationSpeed, RotateMode.FastBeyond360)
            .SetEase(rotationEaseType)
            .SetLoops(-1, loopType); 
    }
}
