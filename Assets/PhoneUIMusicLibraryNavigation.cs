using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(PhoneUIMusicLibraryData))]
[RequireComponent(typeof(PhoneUIMusicLibraryAnimation))]
public class PhoneUIMusicLibraryNavigation : MonoBehaviour, IPhonePanel
{
    [Header("Scroll")]
    public float scrollCooldown = 0.15f;
    private float scrollTimer = 0f;

    [Header("References")]
    private PhoneUIMusicLibraryData musicLibraryData;
    private PhoneUIMusicLibraryAnimation musicLibraryAnimation;
    private PlayerInputManager input;
    private PlayerPhone playerPhone;

    public void OnPanelShown()
    {
        Initialize();
        Debug.Log("PanelShow"); 
    }

    public void Initialize()
    {
        musicLibraryData = GetComponent<PhoneUIMusicLibraryData>();
        musicLibraryAnimation = GetComponent<PhoneUIMusicLibraryAnimation>();

        input = PlayerInputManager.Instance;
        playerPhone = PlayerPhone.Instance;
        Debug.Log("INNIT"); 
    }

    public void OnPanelHidden()
    {

    }

    private void Update()
    {
        if (playerPhone.currentPanel != PlayerPhone.PhonePanelType.MusicLibrary) { return; }

        scrollTimer -= Time.deltaTime;
        Vector2 stickInput = new Vector2(input.stickAxis_X_L, input.stickAxis_Y_L);
        if (stickInput.magnitude > 0.7f)
        {
            if (scrollTimer <= 0f)
            {
                // Determine dominant axis
                if (Mathf.Abs(stickInput.y) > Mathf.Abs(stickInput.x))
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
            SelectMusicAlbum();
        }

        //"X" button
        if (input.playerInput.Left.Primary.WasPerformedThisFrame())
        {
            BackToHome(); 
        }

        ////"A" button
        //if (input.playerInput.Right.Primary.WasPerformedThisFrame())
        //{
        //    DeletePicture();
        //}

        ////"B" button
        //if (input.playerInput.Right.Secondary.WasPerformedThisFrame())
        //{
        //    OpenPictureInExplorer();
        //}
    }

    public void SetScrollCooldown(float multi = 1)
    {
        scrollTimer = scrollCooldown * multi;
    }

    [Button]
    private void NavigateLeft()
    {
        musicLibraryData.NavigateLeft();
    }

    [Button]
    private void NavigateRight()
    {
        musicLibraryData.NavigateRight();
    }

    [Button]
    private void NavigateUp()
    {
        musicLibraryData.NavigateUp();
    }

    [Button]
    private void NavigateDown()
    {
        musicLibraryData.NavigateDown();
    }

    [Button]
    private void SelectMusicAlbum()
    {
        musicLibraryData.StartPlaylist(); 
    }

    private void BackToHome()
    {
        playerPhone.ShowPanel(PlayerPhone.PhonePanelType.Home); 
    }

    //COULD BE RE USED TO OPEN CUSTOM PLAYLIST FOLDER.
//    [Button]
//    private void OpenPictureInExplorer()
//    {
//#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
//        if (musicLibraryAnimation.sequence_OpenExplorer != null && musicLibraryAnimation.sequence_OpenExplorer.IsActive() && musicLibraryAnimation.sequence_OpenExplorer.IsPlaying())
//        {
//            //Sequence is active
//        }
//        else
//        {
//            musicLibraryAnimation.OpenExplorerSequence();
//        }
//#endif
//    }
}
