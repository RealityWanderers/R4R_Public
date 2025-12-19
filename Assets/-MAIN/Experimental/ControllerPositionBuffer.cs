using UnityEngine;

public class ControllerPositionBuffer
{
    private const int BufferSize = 60;
    private Vector3[] positionBuffer = new Vector3[BufferSize];
    private int currentIndex = 0;

    private PlayerComponentManager cM;
    private Transform playerTransform;

    public void AddPosition(Vector3 controllerWorldPosition)
    {
        if (playerTransform == null)
        {
            cM = PlayerComponentManager.Instance;
            playerTransform = cM.transform_XRRig; 
        }

        // Store position relative to player rig
        Vector3 localPos = playerTransform.InverseTransformPoint(controllerWorldPosition);
        positionBuffer[currentIndex] = localPos;
        currentIndex = (currentIndex + 1) % BufferSize;
    }

    // Get position in local space (relative to player rig)
    public Vector3 GetPositionFramesAgoLocal(int framesAgo)
    {
        framesAgo = Mathf.Clamp(framesAgo, 0, BufferSize - 1);
        int index = (currentIndex - framesAgo + BufferSize) % BufferSize;
        return positionBuffer[index];
    }

    // Get position converted *back* to world space
    public Vector3 GetPositionFramesAgoWorld(int framesAgo)
    {
        Vector3 localPos = GetPositionFramesAgoLocal(framesAgo);
        return playerTransform.TransformPoint(localPos);
    }

    // Get velocity in world space between two frames ago counts
    public Vector3 GetVelocityWorld(int startFramesAgo, int endFramesAgo)
    {
        Vector3 startWorld = GetPositionFramesAgoWorld(startFramesAgo);
        Vector3 endWorld = GetPositionFramesAgoWorld(endFramesAgo);
        float deltaTime = (startFramesAgo - endFramesAgo) * Time.deltaTime;
        return (endWorld - startWorld) / deltaTime;
    }

    // Optional: velocity in local space
    public Vector3 GetVelocityLocal(int startFramesAgo, int endFramesAgo)
    {
        Vector3 startLocal = GetPositionFramesAgoLocal(startFramesAgo);
        Vector3 endLocal = GetPositionFramesAgoLocal(endFramesAgo);
        float deltaTime = (startFramesAgo - endFramesAgo) * Time.deltaTime;
        return (endLocal - startLocal) / deltaTime;
    }

    public void DebugDraw(Color color, Transform origin = null)
    {
        for (int i = 0; i < BufferSize - 1; i++)
        {
            Vector3 a = GetPositionFramesAgoLocal(i);
            Vector3 b = GetPositionFramesAgoLocal(i + 1);

            if (origin != null)
            {
                a = origin.TransformPoint(a);
                b = origin.TransformPoint(b);
            }

            Debug.DrawLine(a, b, color);
        }
    }
}
