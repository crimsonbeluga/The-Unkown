// File: PlayerController.cs
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerPawn))]
public class PlayerController : MonoBehaviour, PlayerInputActions.IPlayerActions
{
    private PlayerInputActions inputActions;
    private PlayerPawn pawn;

    private Vector2 moveInput;
    private bool jumpRequested;
    private bool crouchHeld;
    private bool runHeld;
    private bool crawlHeld;

    private bool allowInput = true;

    [SerializeField] private PushPullHandler pushPullHandler;

    void Awake()
    {
        inputActions = new PlayerInputActions();
        inputActions.Player.SetCallbacks(this);
        pawn = GetComponent<PlayerPawn>();
    }

    void OnEnable()
    {
        inputActions.Player.SetCallbacks(this);
        inputActions.Player.Enable();
        Debug.Log("Interact bound to: " + inputActions.Player.Interact.bindings[0].path);
    }

    void OnDisable() => inputActions.Player.Disable();

    void Update()
    {
        if (!allowInput) return;

        Vector2 processedInput = moveInput;

        if (runHeld || crawlHeld || processedInput.x != 0)
            processedInput.y = 0;

        pawn.ReceiveInput(processedInput, jumpRequested, crouchHeld, runHeld, crawlHeld);

        jumpRequested = false;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (!allowInput) return;
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!allowInput || !context.performed) return;
        jumpRequested = true;
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        if (!allowInput) return;
        runHeld = context.ReadValueAsButton();
    }

    public void OnRoll(InputAction.CallbackContext context)
    {
        if (!allowInput || !context.performed) return;
        pawn.TriggerRoll();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        Debug.Log("OnInteract callback hit!");
        if (!allowInput || !context.performed) return;

        Debug.Log("E pressed");
        pushPullHandler?.TryInteract();
    }

    public void OnCrawl(InputAction.CallbackContext context)
    {
        if (!allowInput) return;
        crawlHeld = context.ReadValueAsButton();
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        if (!allowInput) return;
        crouchHeld = context.ReadValueAsButton();
    }

    public void FreezeMovement(bool freeze)
    {
        if (pawn != null)
            pawn.FreezeMovement(freeze);
    }
    public void DisableInput()
    {
        allowInput = false;
        moveInput = Vector2.zero;
        jumpRequested = false;
        crouchHeld = false;
        runHeld = false;
        crawlHeld = false;
    }

    public void EnableInput() => allowInput = true;
    public bool IsInputEnabled => allowInput;
}
