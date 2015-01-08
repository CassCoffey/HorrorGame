using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class Player : MonoBehaviour {

    // The player's menu objects.
    public GameObject Menu;
    public GameObject Vitals;
    public GameObject Chat;
    public GameObject player;

    public string Name;

    [SerializeField]private float runSpeed = 8f;                                       // The speed at which we want the character to move
    [SerializeField]private float strafeSpeed = 4f;                                    // The speed at which we want the character to be able to strafe
    [SerializeField]private float jumpPower = 5f;                                      // The power behind the characters jump. increase for higher jumps
#if !MOBILE_INPUT
    [SerializeField]private bool walkByDefault = true;									// controls how the walk/run modifier key behaves.
    [SerializeField]private float walkSpeed = 3f;                                      // The speed at which we want the character to move
#endif
    // Synchronization variables.
    private float lastSynchronizationTime = 0f;
    private float syncDelay = 0f;
    private float syncTime = 0f;
    private Vector3 syncStartPosition = Vector3.zero;
    private Vector3 syncEndPosition = Vector3.zero;
    private Quaternion syncStartRotation = Quaternion.identity;
    private Quaternion syncEndRotation = Quaternion.identity;
    private Vector3 mousePosPrev;
    // Variables for player movement.
    public bool grounded { get; private set; }
    private IComparer rayHitComparer;
    private const float jumpRayLength = 0.7f;
    private const float playerReach = 1f;
    private const float playerReachThickness = 0.25f;
    private Vector2 input;
    private CapsuleCollider capsule;
    private bool sprinting;

    // Variables for item use.
    public GameObject currentItem = null;
    public GameObject sheathedItem = null;
    public GameObject weaponLoc;
    public GameObject sheathedLoc;
    public float throwForce = 1750;
    
    private bool chatting = false;

    [SerializeField]private AdvancedSettings advanced = new AdvancedSettings();
    [System.Serializable]
    public class AdvancedSettings                                                       // The advanced settings
    {
        public float gravityMultiplier = 1f;                                            // Changes the way gravity effect the player ( realistic gravity can look bad for jumping in game )
        public PhysicMaterial zeroFrictionMaterial;                                     // Material used for zero friction simulation
        public PhysicMaterial highFrictionMaterial;                                     // Material used for high friction ( can stop character sliding down slopes )
        public float groundStickyEffect = 5f;											// power of 'stick to ground' effect - prevents bumping down slopes.
    }

    /// <summary>
    /// When this script wakes up, initialize some variables and lock the mouse cursor.
    /// </summary>
    void Awake()
    {
        if (networkView.isMine)
        {
            // Set up a reference to the capsule collider.
            capsule = collider as CapsuleCollider;
            grounded = true;
            Screen.lockCursor = true;
            rayHitComparer = new RayHitComparer();
            Name = GameObject.Find("SpawnManager").GetComponent<SpawnManager>().randomName;
            networkView.RPC("UpdateNameText", RPCMode.AllBuffered, Name);
            transform.FindChild("NameCanvas").gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// If this is my network view, look for input.
    /// </summary>
    void Update()
    {
        if (networkView.isMine)
        {
            MenuInput();
            if (!Menu.activeSelf && !chatting)
            {
                KeyInput();
            }
        }
        else
        {
            UpdateNamePosition();
        }
    }

    /// <summary>
    /// If the network view is mine, handle movement.
    /// If not, then sync movement.
    /// </summary>
    void FixedUpdate()
    {
        if (networkView.isMine)
        {
            InputMovement();
        }
        else
        {
            SyncedMovement();
        }
    }

    /// <summary>
    /// Sync position and lerp the player object smoothly between network updates.
    /// </summary>
    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        Vector3 syncPosition = Vector3.zero;
        Vector3 syncVelocity = Vector3.zero;
        Quaternion syncRotation = Quaternion.identity;
        Vector3 syncAngularVelocity = Vector3.zero;
        if (stream.isWriting)
        {
            syncPosition = rigidbody.position;
            stream.Serialize(ref syncPosition);

            syncVelocity = rigidbody.velocity;
            stream.Serialize(ref syncVelocity);

            syncRotation = rigidbody.rotation;
            stream.Serialize(ref syncRotation);

            syncAngularVelocity = rigidbody.angularVelocity;
            stream.Serialize(ref syncAngularVelocity);
        }
        else
        {
            stream.Serialize(ref syncPosition);
            stream.Serialize(ref syncVelocity);
            stream.Serialize(ref syncRotation);
            stream.Serialize(ref syncAngularVelocity);

            syncTime = 0f;
            syncDelay = Time.time - lastSynchronizationTime;
            lastSynchronizationTime = Time.time;

            syncEndPosition = syncPosition + syncVelocity * syncDelay;
            syncStartPosition = rigidbody.position;
            syncEndRotation = syncRotation * Quaternion.Euler(syncAngularVelocity * syncDelay * Mathf.Rad2Deg);
            syncStartRotation = rigidbody.rotation;
        }
    }

    private void UpdateNamePosition()
    {
        GameObject player = GameObject.Find("SpawnManager").GetComponent<SpawnManager>().myPlayer;
        GameObject NamePlate = transform.FindChild("NameCanvas").gameObject;
        NamePlate.transform.LookAt(player.transform.FindChild("CameraLocation"));
        Color textColor = NamePlate.GetComponentInChildren<Text>().color;
        float distance =  3 / Vector3.Distance(player.transform.position, transform.position);
        if (distance > 1f)
        {
            distance = 1f;
        }
        if (distance < 0.15f)
        {
            distance = 0f;
        }
        NamePlate.GetComponentInChildren<Text>().color = new Color(textColor.r, textColor.g, textColor.b, distance);
    }

    /// <summary>
    /// Handle input and movement for the player.
    /// </summary>
    void InputMovement()
    {
        float speed = runSpeed;

        // Read input
#if CROSS_PLATFORM_INPUT
        float h = CrossPlatformInput.GetAxis("Horizontal");
        float v = CrossPlatformInput.GetAxis("Vertical");
        bool jump = CrossPlatformInput.GetButton("Jump") && transform.GetComponent<Vitals>().CanJump() && grounded;  // Makes sure the player has enough stamina.
#else
		float h = Input.GetAxis("Horizontal");
		float v = Input.GetAxis("Vertical");
        bool jump = Input.GetButton("Jump") && transform.GetComponent<Vitals>().CanJump() && grounded;
#endif
        // Don't take movement if chatting.
        if (chatting)
        {
            h = 0;
            v = 0;
            jump = false;
        }
#if !MOBILE_INPUT
        // Use stamina if jumping.
        if (jump)
        {
            transform.GetComponent<Vitals>().UseStamina(transform.GetComponent<Vitals>().jumpStamina);
        }

        // Use stamina if sprinting.
        if (sprinting)
        {
            transform.GetComponent<Vitals>().UseStamina(Time.deltaTime);
        }

        // On standalone builds, walk/run speed is modified by a key press.
        // We select appropriate speed based on whether we're walking by default, and whether the walk/run toggle button is pressed:
        bool walkOrRun = Input.GetKey(KeyCode.LeftShift) && transform.GetComponent<Vitals>().CanRun();
        sprinting = walkOrRun;
        speed = walkByDefault ? (walkOrRun ? runSpeed : walkSpeed) : (walkOrRun ? walkSpeed : runSpeed);
        // On mobile, it's controlled in analogue fashion by the v input value, and therefore needs no special handling.


#endif

        input = new Vector2(h, v);

        // normalize input if it exceeds 1 in combined length:
        if (input.sqrMagnitude > 1) input.Normalize();

        // Get a vector which is desired move as a world-relative direction, including speeds
        Vector3 desiredMove = transform.forward * input.y * speed + transform.right * input.x * strafeSpeed;

        // preserving current y velocity (for falling, gravity)
        float yv = rigidbody.velocity.y;

        // add jump power
        if (grounded && jump)
        {
            yv += jumpPower;
            grounded = false;
        }

        // Set the rigidbody's velocity according to the ground angle and desired move
        rigidbody.velocity = desiredMove + Vector3.up * yv;

        // Use low/high friction depending on whether we're moving or not
        if (desiredMove.magnitude > 0 || !grounded)
        {
            collider.material = advanced.zeroFrictionMaterial;
        }
        else
        {
            collider.material = advanced.highFrictionMaterial;
        }


        // Ground Check:

        // Create a ray that points down from the centre of the character.
        Ray ray = new Ray(transform.position, -transform.up);

        // Raycast slightly further than the capsule (as determined by jumpRayLength)
        RaycastHit[] hits = Physics.RaycastAll(ray, capsule.height * jumpRayLength);
        System.Array.Sort(hits, rayHitComparer);


        if (grounded || rigidbody.velocity.y < jumpPower * .5f)
        {
            // Default value if nothing is detected:
            grounded = false;
            // Check every collider hit by the ray
            for (int i = 0; i < hits.Length; i++)
            {
                // Check it's not a trigger
                if (!hits[i].collider.isTrigger)
                {
                    // The character is grounded, and we store the ground angle (calculated from the normal)
                    grounded = true;

                    // stick to surface - helps character stick to ground - specially when running down slopes
                    //if (rigidbody.velocity.y <= 0) {
                    rigidbody.position = Vector3.MoveTowards(rigidbody.position, hits[i].point + Vector3.up * capsule.height * .5f, Time.deltaTime * advanced.groundStickyEffect);
                    //}
                    rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0, rigidbody.velocity.z);
                    break;
                }
            }
        }

        Debug.DrawRay(ray.origin, ray.direction * capsule.height * jumpRayLength, grounded ? Color.green : Color.red);


        // add extra gravity
        rigidbody.AddForce(Physics.gravity * (advanced.gravityMultiplier - 1));
    }

    /// <summary>
    /// Smoothly moves networked bodies.
    /// </summary>
    private void SyncedMovement()
    {
        syncTime += Time.deltaTime;
        rigidbody.position = Vector3.Lerp(syncStartPosition, syncEndPosition, syncTime / syncDelay);
        rigidbody.rotation = Quaternion.Lerp(syncStartRotation, syncEndRotation, syncTime / syncDelay);
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
        Vitals.SetActive(!Menu.activeSelf);
        Screen.showCursor = Menu.activeSelf;
        Screen.lockCursor = !Menu.activeSelf;
        foreach (MouseLook mouseLook in GetComponentsInChildren<MouseLook>())
        {
            mouseLook.enabled = !Menu.activeSelf;
        }
        chatting = false;
        GetComponent<ChatScript>().SetInactive();
    }

    /// <summary>
    /// Toggles the chat.
    /// </summary>
    public void ToggleChat()
    {
        chatting = !chatting;
        GetComponent<ChatScript>().ToggleActive();
        foreach (MouseLook mouseLook in GetComponentsInChildren<MouseLook>())
        {
            mouseLook.enabled = !chatting;
        }
    }

    /// <summary>
    /// Looks for key inputs.
    /// </summary>
    private void KeyInput()
    {
        if (Input.GetButtonDown("Interact") && (currentItem == null || sheathedItem == null))
        {
            PickupItem();
        }
        if (Input.GetButtonDown("Swap Weapons"))
        {
            SwapItems();
        }
        if (Input.GetButtonDown("Drop Weapon"))
        {
            DropItem();
        }
        if (Input.GetButton("Attack"))
        {
            ItemUse();
        }
        if (Input.GetButtonDown("Throw Weapon") || Input.GetButtonUp("Throw Weapon"))
        {
            AltItemUse();
        }
    }

    /// <summary>
    /// Pick up a weapon that is in front of you.
    /// </summary>
    private void PickupItem()
    {
        Ray ray = new Ray(transform.FindChild("Player Camera").position, transform.FindChild("Player Camera").forward);

        RaycastHit[] hits = Physics.RaycastAll(ray, capsule.height * playerReach);
        System.Array.Sort(hits, rayHitComparer);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider.gameObject.tag == "Item" && !hits[i].transform.GetComponent<Item>().isEquipped)
            {
                networkView.RPC("SyncPickup", RPCMode.OthersBuffered, hits[i].transform.networkView.viewID);
                hits[i].transform.GetComponent<Item>().PickUp(gameObject);
                if (currentItem == null)
                {
                    SetCurrentItem(hits[i].collider.gameObject);
                }
                else
                {
                    SetSheathedItem(hits[i].collider.gameObject);
                }
                return;
            }
        }
    }

    /// <summary>
    /// Sets the current item.
    /// </summary>
    /// <param name="Item">The item being set.</param>
    public void SetCurrentItem(GameObject Item)
    {
        currentItem = Item;
        if (Item != null)
        {
            Item.transform.SetParent(weaponLoc.transform);
            Item.GetComponent<Item>().LerpTo(Vector3.zero, Quaternion.identity, 0.3f);
        }
    }

    /// <summary>
    /// Sets the sheathed item.
    /// </summary>
    /// <param name="Item">The item being set.</param>
    public void SetSheathedItem(GameObject Item)
    {
        sheathedItem = Item;
        if (Item != null)
        {
            Item.transform.SetParent(sheathedLoc.transform);
            Item.GetComponent<Item>().LerpTo(Vector3.zero, Quaternion.identity, 0.3f);
        }
    }

    /// <summary>
    /// Swaps current and sheathed items.
    /// </summary>
    public void SwapItems()
    {
        if (sheathedItem != null && weaponLoc.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Default") && currentItem.GetComponent<Item>().CanSwitch())
        {
            networkView.RPC("SyncSwap", RPCMode.OthersBuffered);
            GameObject tempItem = currentItem;
            SetCurrentItem(sheathedItem);
            SetSheathedItem(tempItem);
        }
    }

    /// <summary>
    /// Drops your current weapon.
    /// </summary>
    public void DropItem()
    {
        if (currentItem != null && weaponLoc.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Default"))
        {
            networkView.RPC("SyncDrop", RPCMode.OthersBuffered);
            currentItem.GetComponent<Item>().Drop();
            currentItem = null;
            if (sheathedItem != null)
            {
                SetCurrentItem(sheathedItem);
                sheathedItem = null;
            }
        }
    }

    /// <summary>
    /// Begins charging weapon to be thrown.
    /// </summary>
    public void AltItemUse()
    {
        if (currentItem != null && weaponLoc.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Default"))
        {
            currentItem.GetComponent<Item>().AltUse();
            networkView.RPC("Sync", RPCMode.OthersBuffered);
            if (currentItem == null && sheathedItem != null)
            {
                SetCurrentItem(sheathedItem);
                sheathedItem = null;
            }
        }
    }

    /// <summary>
    /// Plays the animation for swinging the weapon.
    /// </summary>
    public void ItemUse()
    {
        if (currentItem != null && weaponLoc.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Default") && currentItem.transform.localPosition == Vector3.zero)
        {
            currentItem.GetComponent<Item>().Use();
            networkView.RPC("Sync", RPCMode.OthersBuffered);
            if (currentItem == null && sheathedItem != null)
            {
                SetCurrentItem(sheathedItem);
                sheathedItem = null;
            }
        }
    }

    /// <summary>
    /// Sets up the player view based on if client or server.
    /// </summary>

	void OnPlayerDisconnected(NetworkPlayer player)
	{
		Network.RemoveRPCs (player);
		Network.DestroyPlayerObjects (player);
	}

    void OnNetworkInstantiate(NetworkMessageInfo info)
    {
        if (networkView.isMine)
        {
            Debug.Log("Instantiating local player view.");
            Camera.SetupCurrent(GetComponentInChildren<Camera>());
            GetComponentInChildren<Camera>().enabled = true;
            GetComponent<MouseLook>().enabled = true;
            GetComponentInChildren<MouseLook>().enabled = true;
            GetComponentInChildren<Camera>().GetComponent<MouseLook>().enabled = true;
        }
        else
        {
            Debug.Log("Instantiating remote player view.");
            GetComponentInChildren<Camera>().enabled = false;
            GetComponent<MouseLook>().enabled = false;
            GetComponentInChildren<MouseLook>().enabled = false;
            GetComponentInChildren<Camera>().GetComponent<MouseLook>().enabled = false;
        }
    }

    [RPC] void UpdateNameText(string name)
    {
        transform.FindChild("NameCanvas").GetComponentInChildren<Text>().text = name;
    }

    //
    /// <summary>
    /// RPCs that sync all weapon movements across clients.
    /// </summary>
    //

    [RPC] void SyncPickup(NetworkViewID viewID)
    {
        GameObject weapon = NetworkView.Find(viewID).gameObject;
        if (currentItem == null)
        {
            SetCurrentItem(weapon.collider.gameObject);
        }
        else
        {
            SetSheathedItem(weapon.collider.gameObject);
        }
    }

    [RPC] void SyncSwap()
    {
        GameObject tempWeapon = currentItem;
        SetCurrentItem(sheathedItem);
        SetSheathedItem(tempWeapon);
    }

    [RPC] void SyncDrop()
    {
        currentItem = null;
        if (sheathedItem != null)
        {
            SetCurrentItem(sheathedItem);
            sheathedItem = null;
        }
    }

    [RPC] void Sync()
    {
        if (currentItem == null && sheathedItem != null)
        {
            SetCurrentItem(sheathedItem);
            sheathedItem = null;
        }
    }


    /// <summary>
    /// Used for comparing distances
    /// </summary>
    class RayHitComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            return ((RaycastHit)x).distance.CompareTo(((RaycastHit)y).distance);
        }
    }
}