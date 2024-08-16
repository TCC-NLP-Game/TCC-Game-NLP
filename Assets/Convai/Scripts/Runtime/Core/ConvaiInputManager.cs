using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
///     The Input Manager class for Convai, allowing you to control inputs in your project through this class.
///     It supports both the New Input System and Old Input System.
/// </summary>
[DefaultExecutionOrder(-105)]
public class ConvaiInputManager : MonoBehaviour
{
#if ENABLE_LEGACY_INPUT_MANAGER
    [Serializable]
    public class FourDirectionalMovementKeys
    {
        public KeyCode Forward = KeyCode.W;
        public KeyCode Backward = KeyCode.S;
        public KeyCode Right = KeyCode.D;
        public KeyCode Left = KeyCode.A;
    }
#endif

#if ENABLE_INPUT_SYSTEM

    /// <summary>
    ///     Input Action for player movement.
    /// </summary>
    [Header("Player Related")] public InputAction PlayerMovementKeyAction;

    /// <summary>
    ///     Input Action for player jumping.
    /// </summary>
    public InputAction PlayerJumpKeyAction;

    /// <summary>
    ///     Input Action for player running.
    /// </summary>
    public InputAction PlayerRunKeyAction;

    /// <summary>
    ///     Input Action for locking the cursor.
    /// </summary>
    [Header("General")] public InputAction CursorLockKeyAction;

    /// <summary>
    ///     Input Action for sending text.
    /// </summary>
    public InputAction TextSendKeyAction;

    /// <summary>
    ///     Input Action for talk functionality.
    /// </summary>
    public InputAction TalkKeyAction;

    /// <summary>
    ///     Action to open the Settings Panel.
    /// </summary>
    public InputAction SettingsKeyAction;
#elif ENABLE_LEGACY_INPUT_MANAGER
    /// <summary>
    /// Key used to manage cursor lock
    /// </summary>
    public KeyCode CursorLockKey = KeyCode.Escape;

    /// <summary>
    /// Key used to manage text send
    /// </summary>
    public KeyCode TextSendKey = KeyCode.Return;

    /// <summary>
    /// Key used to manage text send
    /// </summary>
    public KeyCode TextSendAltKey = KeyCode.KeypadEnter;

    /// <summary>
    /// Key used to manage record user audio
    /// </summary>
    public KeyCode TalkKey = KeyCode.T;

    /// <summary>
    /// Key used to manage setting panel toggle
    /// </summary>
    public KeyCode OpenSettingPanelKey = KeyCode.F10;

    /// <summary>
    /// Key used to manage running
    /// </summary>
    public KeyCode RunKey = KeyCode.LeftShift;

    /// <summary>
    /// Keys used to manage movement
    /// </summary>
    public FourDirectionalMovementKeys MovementKeys;
#endif


    /// <summary>
    ///     Singleton instance providing easy access to the ConvaiInputManager from other scripts.
    /// </summary>
    public static ConvaiInputManager Instance { get; private set; }

    /// <summary>
    ///     Awake is called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        // Ensure only one instance of ConvaiInputManager exists
        if (Instance != null)
        {
            Debug.LogError("There's more than one ConvaiInputManager! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    /// <summary>
    ///     Enable input actions when the object is enabled.
    /// </summary>
    private void OnEnable()
    {
#if ENABLE_INPUT_SYSTEM
        PlayerMovementKeyAction.Enable();
        PlayerJumpKeyAction.Enable();
        PlayerRunKeyAction.Enable();
        CursorLockKeyAction.Enable();
        TextSendKeyAction.Enable();
        TalkKeyAction.Enable();
        SettingsKeyAction.Enable();
#endif
    }

    /// <summary>
    ///     Checks if the left mouse button was pressed.
    /// </summary>
    public bool WasMouseLeftButtonPressed()
    {
        // Check if the left mouse button was pressed this frame
#if ENABLE_INPUT_SYSTEM && (!UNITY_ANDROID || !UNITY_IOS)
        return Mouse.current.leftButton.wasPressedThisFrame;
#else
        return Input.GetMouseButtonDown(0);
#endif
    }

    /// <summary>
    ///     Gets the current mouse position.
    /// </summary>
    public Vector2 GetMousePosition()
    {
        // Get the current mouse position
#if ENABLE_INPUT_SYSTEM
        return Mouse.current.position.ReadValue();
#else
        return Input.mousePosition;
#endif
    }

    /// <summary>
    ///     Gets the vertical movement of the mouse.
    /// </summary>
    public float GetMouseYAxis()
    {
        // Get the vertical movement of the mouse
#if ENABLE_INPUT_SYSTEM && (!UNITY_ANDROID || !UNITY_IOS)
        return Mouse.current.delta.y.ReadValue();
#else
        return Input.GetAxis("Mouse Y");
#endif
    }

    /// <summary>
    ///     Gets the horizontal movement of the mouse.
    /// </summary>
    public float GetMouseXAxis()
    {
        // Get the horizontal movement of the mouse
#if ENABLE_INPUT_SYSTEM && (!UNITY_ANDROID || !UNITY_IOS)
        return Mouse.current.delta.x.ReadValue();
#else
        return Input.GetAxis("Mouse X");
#endif
    }

    // General input methods
    /// <summary>
    ///     Checks if the cursor lock key was pressed.
    /// </summary>
    public bool WasCursorLockKeyPressed()
    {
        // Check if the cursor lock key was pressed this frame
#if ENABLE_INPUT_SYSTEM
        return CursorLockKeyAction.WasPressedThisFrame();
#else
        return Input.GetKeyDown(CursorLockKey);
#endif
    }

    /// <summary>
    ///     Checks if the text send key was pressed.
    /// </summary>
    public bool WasTextSendKeyPressed()
    {
        // Check if the text send key was pressed this frame
#if ENABLE_INPUT_SYSTEM
        return TextSendKeyAction.WasPressedThisFrame();
#else
        return Input.GetKeyDown(TextSendKey) || Input.GetKeyDown(TextSendAltKey);
#endif
    }

    /// <summary>
    ///     Checks if the talk key was pressed.
    /// </summary>
    public bool WasTalkKeyPressed()
    {
        // Check if the talk key was pressed this frame
#if ENABLE_INPUT_SYSTEM
        return TalkKeyAction.WasPressedThisFrame();
#else
        return Input.GetKeyDown(TalkKey);
#endif
    }

    /// <summary>
    ///     Checks if the talk key is being held down.
    /// </summary>
    public bool IsTalkKeyHeld()
    {
        // Check if the talk key is being held down
#if ENABLE_INPUT_SYSTEM
        return TalkKeyAction.IsPressed();
#else
        return Input.GetKey(TalkKey);
#endif
    }

#if ENABLE_INPUT_SYSTEM
    /// <summary>
    ///     Retrieves the InputAction associated with the talk key.
    /// </summary>
    /// <returns>The InputAction for handling talk-related input.</returns>
    public InputAction GetTalkKeyAction() => TalkKeyAction;
#endif
    /// <summary>
    ///     Checks if the talk key was released.
    /// </summary>
    public bool WasTalkKeyReleased()
    {
        // Check if the talk key was released this frame
#if ENABLE_INPUT_SYSTEM
        return TalkKeyAction.WasReleasedThisFrame();
#else
        return Input.GetKeyUp(TalkKey);
#endif
    }

    /// <summary>
    ///     Checks if the Settings key was pressed.
    /// </summary>
    public bool WasSettingsKeyPressed()
    {
        // Check if the Settings key was pressed this frame
#if ENABLE_INPUT_SYSTEM
        return SettingsKeyAction.WasPressedThisFrame();
#else
        return Input.GetKeyDown(OpenSettingPanelKey);
#endif
    }

    // Player related input methods

    /// <summary>
    ///     Checks if the jump key was pressed.
    /// </summary>
    public bool WasJumpKeyPressed()
    {
        // Check if the jump key was pressed this frame
#if ENABLE_INPUT_SYSTEM
        return PlayerJumpKeyAction.WasPressedThisFrame();
#else
        return Input.GetButton("Jump");
#endif
    }

    /// <summary>
    ///     Checks if the run key is being held down.
    /// </summary>
    public bool IsRunKeyHeld()
    {
        // Check if the run key is being held down
#if ENABLE_INPUT_SYSTEM
        return PlayerRunKeyAction.IsPressed();
#else
        return Input.GetKey(RunKey);
#endif
    }

    /// <summary>
    ///     Gets the player's movement input vector.
    /// </summary>
    public Vector2 GetPlayerMoveVector()
    {
        // Get the player's movement input vector
#if ENABLE_INPUT_SYSTEM
        return PlayerMovementKeyAction.ReadValue<Vector2>();
#else
        Vector2 inputMoveDir = new Vector2(0, 0);
        // Manual input for player movement
        if (Input.GetKey(MovementKeys.Forward))
        {
            inputMoveDir.y += 1f;
        }

        if (Input.GetKey(MovementKeys.Backward))
        {
            inputMoveDir.y -= 1f;
        }

        if (Input.GetKey(MovementKeys.Left))
        {
            inputMoveDir.x -= 1f;
        }

        if (Input.GetKey(MovementKeys.Right))
        {
            inputMoveDir.x += 1f;
        }

        return inputMoveDir;
#endif
    }
}