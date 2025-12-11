using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(PhoneUIAlbumData))]
[RequireComponent(typeof(PhoneUIAlbumSelectedPicture))]
[RequireComponent(typeof(PhoneUIAlbumAnimation))]
public class PhoneUIAlbumNavigation : MonoBehaviour
{
    [Header("Scroll")]
    public float scrollCooldown = 0.15f;
    private float scrollTimer = 0f;

    [Header("References")]
    private PhoneUIAlbumData albumData;
    private PhoneUIAlbumAnimation albumAnimation;
    private PhoneUIAlbumSelectedPicture albumSelectedPicture;
    private PlayerInputManager input;
    private PlayerPhone playerPhone;

    private void Awake()
    {
        albumData = GetComponent<PhoneUIAlbumData>();
        albumAnimation = GetComponent<PhoneUIAlbumAnimation>();
        albumSelectedPicture = GetComponent<PhoneUIAlbumSelectedPicture>();

        input = PlayerInputManager.Instance;
        playerPhone = PlayerPhone.Instance;
    }

    private void Update()
    {
        if (playerPhone.currentPanel != PlayerPhone.PhonePanelType.Album) { return; }

        scrollTimer -= Time.deltaTime;
        Vector2 stickInput = new Vector2(input.stickAxis_X_L, input.stickAxis_Y_L);
        if (stickInput.magnitude > 0.7f)
        {
            if (scrollTimer <= 0f)
            {
                // Determine dominant axis
                if (Mathf.Abs(stickInput.y) > Mathf.Abs(stickInput.x))
                {
                    if (!albumSelectedPicture.selectedPictureActive) //Specifically block up and down movement while selected picture is active but allow left and right movement.
                    {
                        // Vertical input is dominant
                        if (stickInput.y >= 0.5f)
                        {
                            SetScrollCooldown();
                            NavigateUp();
                        }
                        else if (stickInput.y <= -0.5f)
                        {
                            SetScrollCooldown();
                            NavigateDown();
                        }
                    }
                }
                else
                {
                    // Horizontal input is dominant
                    if (stickInput.x >= 0.5f)
                    {
                        SetScrollCooldown();
                        NavigateLeft();
                    }
                    else if (stickInput.x <= -0.5f)
                    {
                        SetScrollCooldown();
                        NavigateRight();
                    }
                }
            }
        }

        //"Y" button
        if (input.playerInput.Left.Secondary.WasPerformedThisFrame())
        {
            SelectPicture();
        }

        //"X" button
        if (input.playerInput.Left.Primary.WasPerformedThisFrame())
        {
            if (albumSelectedPicture.selectedPictureActive)
            {
                UnSelectPicture();
            }
            else
            {
                SwapToCamera();
            }
        }

        //"A" button
        if (input.playerInput.Right.Primary.WasPerformedThisFrame())
        {
            DeletePicture();
        }

        //"B" button
        if (input.playerInput.Right.Secondary.WasPerformedThisFrame())
        {
            OpenPictureInExplorer();
        }
    }

    public void SetScrollCooldown(float multi = 1)
    {
        scrollTimer = scrollCooldown * multi;
    }

    [Button]
    private void NavigateLeft()
    {
        albumData.NavigateLeft();
        if (albumSelectedPicture.selectedPictureActive)
        {
            albumSelectedPicture.RefreshPicture();
        }
    }

    [Button]
    private void NavigateRight()
    {
        albumData.NavigateRight();
        if (albumSelectedPicture.selectedPictureActive)
        {
            albumSelectedPicture.RefreshPicture();
        }
    }

    [Button]
    private void NavigateUp()
    {
        albumData.NavigateUp();
    }

    [Button]
    private void NavigateDown()
    {
        albumData.NavigateDown();
    }

    [Button]
    private void SelectPicture()
    {
        if (!albumSelectedPicture.selectedPictureActive)
        {
            albumSelectedPicture.OpenPicture();
        }
    }

    [Button]
    private void UnSelectPicture()
    {
        if (albumSelectedPicture.selectedPictureActive)
        {
            albumSelectedPicture.ClosePicture();
        }
    }

    [Button]
    private void DeletePicture()
    {
        if (albumData.imagePaths.Count == 0) { return; }
        if (albumAnimation.sequence_DeletePicture != null && albumAnimation.sequence_DeletePicture.IsActive() && albumAnimation.sequence_DeletePicture.IsPlaying())
        {
            //Sequence is active
        }
        else
        {
            albumAnimation.DeletePictureSequence();
        }
    }

    [Button]
    private void OpenPictureInExplorer()
    {
        if (albumData.imagePaths.Count == 0) { return; }
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        if (albumAnimation.sequence_OpenExplorer != null && albumAnimation.sequence_OpenExplorer.IsActive() && albumAnimation.sequence_OpenExplorer.IsPlaying())
        {
            //Sequence is active
        }
        else
        {
            albumAnimation.OpenExplorerSequence();
        }
#endif
    }

    [Button]
    private void SwapToCamera()
    {
        playerPhone.ShowPanel(PlayerPhone.PhonePanelType.PictureMode);
    }
}
