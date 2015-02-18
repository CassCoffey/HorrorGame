using UnityEngine;
using System.Collections;

public class SpectatorManager : MonoBehaviour {

    public GameObject Menu;
    public GameObject Chat;

    private float flySpeed = 8f;
    private float walkSpeed = 3f;
    private float jumpPower = 5f;
    private bool walkByDefault = true;
    private Vector2 input;

    private bool chatting = false;

	/// <summary>
	/// Turns off all other cameras, and enables the spectator camera.
	/// </summary>
	void Start() 
    {
        Camera.SetupCurrent(GetComponent<Camera>());
        GameObject.FindObjectOfType<Camera>().GetComponent<Camera>().enabled = false;
        GameObject.FindObjectOfType<Camera>().GetComponent<AudioListener>().enabled = false;
        GetComponent<Camera>().enabled = true;
        GetComponent<AudioListener>().enabled = true;
        GetComponent<MouseLook>().enabled = true;
	}

    /// <summary>
    /// When the spectator awaks, re-lock the cursor.
    /// </summary>
    void Awake()
    {
        Screen.lockCursor = true;
    }
	
	/// <summary>
	/// Checks for menu input.
	/// </summary>
	void Update() 
    {
        MenuInput();
    }

    /// <summary>
    /// Checks for movement input.
    /// </summary>
    void FixedUpdate()
    {
        InputMovement();
    }

    /// <summary>
    /// Manages movement.
    /// </summary>
    void InputMovement()
    {
        float speed = flySpeed;

        // Read input
#if CROSS_PLATFORM_INPUT
        float h = CrossPlatformInput.GetAxis("Horizontal");
        float v = CrossPlatformInput.GetAxis("Vertical");
        bool jump = CrossPlatformInput.GetButton("Jump");
#else
		float h = Input.GetAxis("Horizontal");
		float v = Input.GetAxis("Vertical");
        bool jump = Input.GetButton("Jump");
#endif
        // Don't take movement if chatting.
        if (chatting)
        {
            h = 0;
            v = 0;
            jump = false;
        }
#if !MOBILE_INPUT

        // On standalone builds, walk/run speed is modified by a key press.
        // We select appropriate speed based on whether we're walking by default, and whether the walk/run toggle button is pressed:
        bool walkOrRun = Input.GetKey(KeyCode.LeftShift);
        speed = walkByDefault ? (walkOrRun ? flySpeed : walkSpeed) : (walkOrRun ? walkSpeed : flySpeed);
        // On mobile, it's controlled in analogue fashion by the v input value, and therefore needs no special handling.


#endif

        input = new Vector2(h, v);

        // normalize input if it exceeds 1 in combined length:
        if (input.sqrMagnitude > 1) input.Normalize();

        // Get a vector which is desired move as a world-relative direction, including speeds
        Vector3 desiredMove = transform.forward * input.y * speed + transform.right * input.x * speed;

        float yv = 0f;

        // add jump power
        if (jump)
        {
            yv = jumpPower;
        }

        // Set the rigidbody's velocity according to the ground angle and desired move
        rigidbody.velocity = desiredMove + Vector3.up * yv;
    }

    /// <summary>
    /// Looks to see if the player has initiated the menu or chat.
    /// </summary>
    private void MenuInput()
    {
        // Menu Options
        if (Input.GetButtonUp("Menu"))
        {
            ToggleMenu();
        }
        if (Input.GetButtonUp("Chat") && !Menu.activeSelf)
        {
            GetComponent<ChatScript>().SendChatMessage();
            ToggleChat();
        }
    }

    /// <summary>
    /// Toggles the menu on.
    /// </summary>
    public void ToggleMenu()
    {
        Menu.SetActive(!Menu.activeSelf);
        if (Menu.activeSelf == true)
        {
            GetComponent<ChatScript>().SetChat(false);
        }
        Screen.showCursor = Menu.activeSelf;
        Screen.lockCursor = !Menu.activeSelf;
        GetComponent<MouseLook>().enabled = !Menu.activeSelf;
        GetComponent<ChatScript>().SetInactive();
    }

    /// <summary>
    /// Toggles the chat.
    /// </summary>
    public void ToggleChat()
    {
        chatting = !chatting;
        GetComponent<ChatScript>().ToggleActive();
        GetComponent<MouseLook>().enabled = !chatting;
    }
}
