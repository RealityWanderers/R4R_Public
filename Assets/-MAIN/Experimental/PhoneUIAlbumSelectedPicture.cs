using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PhoneUIAlbumData))]
[RequireComponent(typeof(PhoneUIAlbumAnimation))]
public class PhoneUIAlbumSelectedPicture : MonoBehaviour
{
    public bool selectedPictureActive { get; private set; }

    [Header("References")]
    public GameObject selectedPicture;
    public RawImage selectedPictureRawImage;
    public GameObject selectedPictureBG;
    public RawImage selectedPictureBGRawImage;
    public GameObject selectedPictureMenuIcons;

    [Header("References")]
    private PhoneUIAlbumData albumData;
    private PhoneUIAlbumAnimation albumAnimation;

    private void OnEnable()
    {
        albumData = GetComponent<PhoneUIAlbumData>();
        albumAnimation = GetComponent<PhoneUIAlbumAnimation>();

        //if (albumData != null)
        //    albumData.OnPicturesUpdated += RefreshPicture;

        ClosePicture(); 
    }

    private void OnDisable()
    {
        //if (albumData != null)
        //    albumData.OnPicturesUpdated -= RefreshPicture;
    }

    [Button]
    public void OpenPicture()
    {
        if (albumData.imagePaths.Count == 0) { return; }
        selectedPictureRawImage.texture = albumData.GetCurrentPictureTexture();
        albumAnimation.AnimateShowSelectedPicture();
    }

    [Button]
    public void ClosePicture()
    {
        albumAnimation.AnimateHideSelectedPicture();
    }

    public void RefreshPicture() //Called when the picture data is updated, for example on deletion.
    {
        selectedPictureRawImage.texture = albumData.GetCurrentPictureTexture();
    }

    public void ToggleSelectedPictureActiveState(bool state)
    {
        selectedPictureActive = state;
    }
}
