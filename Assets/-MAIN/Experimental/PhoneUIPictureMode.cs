using Sirenix.OdinInspector;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using System.Threading.Tasks;
using System.Collections;
using System;
using System.Linq;
using DG.Tweening;
using UnityEngine.UI;

[RequireComponent(typeof(PhoneUIPictureModeAnimation))]
public class PhoneUIPictureMode : MonoBehaviour, IPhonePanel
{
    [Header("Zoom")]
    [ReadOnly] [SerializeField] public float currentZoom;
    [SerializeField] private float zoomSpeed = 1f;
    [SerializeField] private float zoomStartFOV = 60f;
    [SerializeField] private float zoomEndFOV = 90f;

    [Header("CameraFlash")]
    public AudioSource source_CameraFlash;
    public AudioClip clip_CameraFlash;

    [Header("References")]
    [SerializeField] private RawImage pictureCameraRawImage;
    [SerializeField] private Camera pictureCamera;
    [SerializeField] private Transform transform_CameraPivot;
    [SerializeField] private RenderTexture renderTexture;
    private PlayerPhone playerPhone;
    private PlayerInputManager input;
    private PhoneUIPictureModeAnimation pictureModeAnimation;

    public void OnPanelShown()
    {
        Initialize();
        EnableCamera();
        ResetCameraZoom();
    }

    public void OnPanelHidden()
    {
        DisableCamera();
    }

    private void Initialize()
    {
        playerPhone = PlayerPhone.Instance;
        input = PlayerInputManager.Instance;
        pictureModeAnimation = GetComponent<PhoneUIPictureModeAnimation>();
    }

    void Update()
    {
        if (playerPhone.currentPanel != PlayerPhone.PhonePanelType.PictureMode) {return; }
        if (pictureModeAnimation.picturePreviewActive) { return; }

        float zoomInput = input.stickAxis_Y_L;
        if (Mathf.Abs(zoomInput) > 0.5f)
        {
            if (!zoomActive)
            {
                pictureModeAnimation.ShowZoomUI();
            }
            Zoom(zoomInput);
        }
        else if (zoomActive)
        {
            zoomActive = false;
            pictureModeAnimation.HideZoomUI();
            StopZoomBlur();
        }
        blurMat.SetFloat("_BlurSize", blurStrength);

        if (input.stickAxis_X_L <= -0.7f)
        {
            SwapToAlbum();
        }

        bool cameraSwapInput = input.playerInput.Left.Primary.WasPerformedThisFrame();
        if (cameraSwapInput)
        {
            SwapCamera();
        }

        bool takePictureInput = input.playerInput.Left.Secondary.WasPerformedThisFrame();
        if (takePictureInput)
        {
            TakePicture();
        }
    }

    [Button]
    public void SwapToAlbum()
    {
        playerPhone.ShowPanel(PlayerPhone.PhonePanelType.Album);
        pictureModeAnimation.OpenAlbumSequence();
    }

    public void EnableCamera()
    {
        pictureCamera.enabled = true;
        blurMat = pictureCameraRawImage.material;
        pictureModeAnimation.FlashCameraBorderUI();
    }

    public void DisableCamera()
    {
        pictureCamera.enabled = false;
    }

    private bool zoomActive;
    private Tween zoomBlurTween;
    private Material blurMat;
    private float blurStrength = 0f;
    public void Zoom(float input)
    {
        zoomActive = true;
        float direction = Mathf.Sign(input); // +1 or -1
        currentZoom += direction * zoomSpeed * Time.deltaTime;
        currentZoom = Mathf.Clamp01(currentZoom);

        float targetFOV = Mathf.Lerp(zoomStartFOV, zoomEndFOV, currentZoom);
        pictureCamera.fieldOfView = targetFOV;

        float scrollProgress = currentZoom;
        float xPos = Mathf.Lerp(pictureModeAnimation.scrollMinMax.x, pictureModeAnimation.scrollMinMax.y, scrollProgress);
        Vector3 scrollIndicatorPos = pictureModeAnimation.rect_ZoomScrollProgressIndicator.anchoredPosition;
        pictureModeAnimation.rect_ZoomScrollProgressIndicator.anchoredPosition = new Vector3(xPos, scrollIndicatorPos.y, scrollIndicatorPos.z);

        if (currentZoom > 0 && currentZoom < 1)
        {
            StartZoomBlur();
        }
        else
        {
            StopZoomBlur();
        }
    }

    public void StartZoomBlur()
    {
        zoomBlurTween?.Kill();
        zoomBlurTween = DOTween.To(() => blurStrength, x => blurStrength = x, 6f, 0.065f);
    }

    public void StopZoomBlur()
    {
        zoomBlurTween?.Kill();
        zoomBlurTween = DOTween.To(() => blurStrength, x => blurStrength = x, 0f, 0.12f);
    }

    public void ResetCameraZoom()
    {
        currentZoom = 0;
        pictureCamera.fieldOfView = zoomStartFOV;
        pictureModeAnimation.HideZoomUI();
    }

    public void PlaySFX_CameraFlash()
    {
        source_CameraFlash.clip = clip_CameraFlash;
        source_CameraFlash.Play();
    }

    private bool isFrontCamera = true;
    void SwapCamera()
    {
        isFrontCamera = !isFrontCamera;

        transform_CameraPivot.localRotation = isFrontCamera
            ? Quaternion.identity
            : Quaternion.Euler(0, 180, 0);
    }

    [Button]
    void TakePicture()
    {
        SavePhotoAsync(renderTexture);
        pictureModeAnimation.CameraFlash();
        //Debug.Log("TakePic");
    }

    private int photoCounter = 0;

    void SavePhotoAsync(RenderTexture renderTex)
    {
        // Create sRGB temp RT with same dimensions
        RenderTexture tempRT = new RenderTexture(renderTex.width, renderTex.height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        tempRT.Create();

        // Blit linear renderTex into gamma-corrected sRGB tempRT
        Graphics.Blit(renderTex, tempRT);

        // Request async readback from gamma-corrected tempRT
        AsyncGPUReadback.Request(tempRT, 0, TextureFormat.RGBA32, request =>
        {
            if (request.hasError)
            {
                Debug.LogError("Failed to read texture from GPU!");
                tempRT.Release();
                return;
            }

            var rawData = request.GetData<byte>();

            MainThreadDispatcher.Enqueue(() =>
            {
                Debug.Log("Starting FinalizePhotoAsync Coroutine");
                this.StartCoroutine(FinalizePhotoAsync(rawData.ToArray(), tempRT.width, tempRT.height, photoCounter++));
            });

            tempRT.Release();
        });
    }

    IEnumerator FinalizePhotoAsync(byte[] data, int width, int height, int index)
    {
        yield return null; // let one frame pass, keep things smooth

        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.LoadRawTextureData(data);
        tex.Apply();

        yield return null; // give GPU some breathing room

        // Get raw texture data on main thread (safe!)
        byte[] rawTextureData = tex.GetRawTextureData().ToArray();
        var graphicsFormat = tex.graphicsFormat;

        // Now encode in thread
        byte[] png = null;
        bool done = false;

        System.Threading.ThreadPool.QueueUserWorkItem(_ =>
        {
            try
            {
                png = ImageConversion.EncodeArrayToPNG(rawTextureData, graphicsFormat, (uint)width, (uint)height);
            }
            catch (Exception e)
            {
                Debug.LogError("Exception in PNG encoding thread: " + e);
            }
            finally
            {
                done = true;
            }
        });

        // Wait until encoding is done
        int waitFrames = 0;
        while (!done)
        {
            if (waitFrames % 30 == 0)
                Debug.Log("Waiting for encoding to finish... " + waitFrames + " frames");
            waitFrames++;

            if (waitFrames > 600)
            {
                Debug.LogWarning("Encoding timeout, breaking wait loop.");
                break;
            }

            yield return null;
        }

        if (png != null)
        {
            string folderName = "MyPhotos";
            string folderPath = Path.Combine(Application.persistentDataPath, folderName);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Format: RamenPhoto_2025-05-24_14-30-12.png
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string filename = $"RamenPhoto_{timestamp}.png";
            string filePath = Path.Combine(folderPath, filename);

            // If by some crazy chance the file exists (like you saved multiple pics within the same second),
            // append a small unique suffix to avoid overwrite:
            int suffix = 1;
            while (File.Exists(filePath))
            {
                filename = $"RamenPhoto_{timestamp}_{suffix}.png";
                filePath = Path.Combine(folderPath, filename);
                suffix++;
            }

            File.WriteAllBytes(filePath, png);
            pictureModeAnimation.ShowPicture(tex);
            //Debug.Log($"Saved photo with timestamp filename to: {filePath}");
        }
        else
        {
            //Debug.LogError("PNG data is null, failed to save photo.");
        }
    }
}
