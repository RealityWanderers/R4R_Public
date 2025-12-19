using Sirenix.OdinInspector;
using UnityEngine;


[RequireComponent(typeof(PhoneUIHomeMenuData))]
[RequireComponent(typeof(PhoneUIHomeMenuAnimation))]
public class PhoneUIHomeMenuNavigation : MonoBehaviour
{
    [Header("Scroll")]
    public float scrollCooldown = 0.15f;
    private float scrollTimer = 0f;

    [Header("References")]
    private PhoneUIHomeMenuData homeMenuData;
    private PhoneUIHomeMenuAnimation homeMenuAnimation;
    private PlayerInputManager input;
    private PlayerPhone playerPhone;

    private void Awake()
    {
        homeMenuData = GetComponent<PhoneUIHomeMenuData>();
        homeMenuAnimation = GetComponent<PhoneUIHomeMenuAnimation>();

        input = PlayerInputManager.Instance;
        playerPhone = PlayerPhone.Instance;
    }

    private void Update()
    {
        if (playerPhone.currentPanel != PlayerPhone.PhonePanelType.Home) { return; }

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
            YButtonAction();
        }

        //"X" button
        if (input.playerInput.Left.Primary.WasPerformedThisFrame())
        {
            XButtonAction();
        }
    }

    public void SetScrollCooldown(float multi = 1)
    {
        scrollTimer = scrollCooldown * multi;
    }

    [Button]
    private void NavigateLeft()
    {
        homeMenuData.NavigateLeft();
    }

    [Button]
    private void NavigateRight()
    {
        homeMenuData.NavigateRight();
    }

    [Button]
    private void NavigateUp()
    {
        homeMenuData.NavigateUp();
    }

    [Button]
    private void NavigateDown()
    {
        homeMenuData.NavigateDown();
    }

    [Button]
    private void YButtonAction()
    {
        homeMenuData.OnMenuSelect(); 
    }

    [Button]
    private void XButtonAction()
    {

    }
}
