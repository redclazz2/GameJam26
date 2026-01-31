using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Componente que maneja el input del jugador usando el nuevo Input System.
/// Trabaja con InputDeviceManager para asignación automática de dispositivos.
/// El primer dispositivo que hace input se asigna a Player 1, el segundo a Player 2.
/// </summary>
[RequireComponent(typeof(Player))]
public class PlayerInputHandler : MonoBehaviour
{
    [Header("Input Configuration")]
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private string actionMapName = "Fighter";

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    // Referencias a las acciones (instanciadas por jugador)
    private InputActionMap actionMap;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction attackAction;
    private InputAction blockAction;

    // Referencia al dispositivo asignado
    private InputDevice assignedDevice;
    private string assignedScheme;
    private Player playerComponent;
    private bool hasDevice = false;

    // Valores de input actuales
    private Vector2 moveInput;
    private bool jumpPressed;
    private bool jumpHeld;
    private bool attackPressed;
    private bool blockHeld;

    // Propiedades públicas para que otros scripts lean el input
    public Vector2 MoveInput => hasDevice ? moveInput : Vector2.zero;
    public float HorizontalInput => hasDevice ? moveInput.x : 0f;
    public float VerticalInput => hasDevice ? moveInput.y : 0f;
    public bool JumpPressed => hasDevice && jumpPressed;
    public bool JumpHeld => hasDevice && jumpHeld;
    public bool AttackPressed => hasDevice && attackPressed;
    public bool BlockHeld => hasDevice && blockHeld;
    public bool HasDevice => hasDevice;
    public string AssignedScheme => assignedScheme;

    // Para detectar doble tap
    private float lastMoveRightTime;
    private float lastMoveLeftTime;
    private bool wasMovingRight;
    private bool wasMovingLeft;
    
    [SerializeField] private float doubleTapTime = 0.25f;
    public bool DashRightTriggered { get; private set; }
    public bool DashLeftTriggered { get; private set; }

    private void Awake()
    {
        playerComponent = GetComponent<Player>();
        InitializeInputActions();
    }

    private void OnEnable()
    {
        // Registrarse con el InputDeviceManager
        if (InputDeviceManager.Instance != null)
        {
            InputDeviceManager.Instance.RegisterHandler(playerComponent.PlayerNumber, this);
        }
    }

    private void OnDisable()
    {
        DisableInput();
        
        // Desregistrarse del InputDeviceManager
        if (InputDeviceManager.Instance != null)
        {
            InputDeviceManager.Instance.UnregisterHandler(playerComponent.PlayerNumber);
        }
    }

    /// <summary>
    /// Inicializa las acciones sin habilitar aún
    /// </summary>
    private void InitializeInputActions()
    {
        if (inputActions == null)
        {
            Debug.LogError($"PlayerInputHandler (P{playerComponent?.PlayerNumber}): No se ha asignado el InputActionAsset!");
            return;
        }

        // Crear una COPIA del InputActionAsset para este jugador
        inputActions = Instantiate(inputActions);

        // Obtener el Action Map
        actionMap = inputActions.FindActionMap(actionMapName);
        if (actionMap == null)
        {
            Debug.LogError($"PlayerInputHandler (P{playerComponent?.PlayerNumber}): No se encontró el ActionMap '{actionMapName}'");
            return;
        }

        // Obtener las acciones individuales
        moveAction = actionMap.FindAction("Move");
        jumpAction = actionMap.FindAction("Jump");
        attackAction = actionMap.FindAction("Attack");
        blockAction = actionMap.FindAction("Block");

        // Suscribirse a eventos de las acciones
        if (jumpAction != null)
        {
            jumpAction.started += OnJumpStarted;
            jumpAction.canceled += OnJumpCanceled;
        }

        if (attackAction != null)
        {
            attackAction.started += OnAttackStarted;
        }
    }

    /// <summary>
    /// Llamado por InputDeviceManager cuando se asigna un dispositivo a este jugador
    /// </summary>
    public void OnDeviceAssigned(InputDevice device, string scheme)
    {
        assignedDevice = device;
        assignedScheme = scheme;
        hasDevice = true;

        // Configurar el action map para usar solo este dispositivo y scheme
        if (actionMap != null)
        {
            actionMap.bindingMask = InputBinding.MaskByGroup(scheme);
            
            // Para gamepads, restringir a este dispositivo específico
            if (device is Gamepad)
            {
                actionMap.devices = new InputDevice[] { device };
            }
            
            actionMap.Enable();
        }

        if (showDebugLogs)
        {
            Debug.Log($"<color=cyan>[PlayerInputHandler] Jugador {playerComponent?.PlayerNumber} recibió dispositivo: {device.displayName} ({scheme})</color>");
        }
    }

    /// <summary>
    /// Llamado por InputDeviceManager cuando se pierde el dispositivo
    /// </summary>
    public void OnDeviceLost()
    {
        hasDevice = false;
        assignedDevice = null;
        assignedScheme = null;

        DisableInput();
        ResetInputValues();

        if (showDebugLogs)
        {
            Debug.Log($"<color=red>[PlayerInputHandler] Jugador {playerComponent?.PlayerNumber} perdió su dispositivo</color>");
        }
    }

    private void EnableInput()
    {
        actionMap?.Enable();
    }

    private void DisableInput()
    {
        actionMap?.Disable();
    }

    private void ResetInputValues()
    {
        moveInput = Vector2.zero;
        jumpPressed = false;
        jumpHeld = false;
        attackPressed = false;
        blockHeld = false;
        DashRightTriggered = false;
        DashLeftTriggered = false;
    }

    private void Update()
    {
        if (!hasDevice) return;

        // Leer valores continuos
        if (moveAction != null)
        {
            moveInput = moveAction.ReadValue<Vector2>();
        }

        if (blockAction != null)
        {
            blockHeld = blockAction.IsPressed();
        }

        if (jumpAction != null)
        {
            jumpHeld = jumpAction.IsPressed();
        }

        // Detectar doble tap para dash
        DetectDoubleTapDash();
    }

    private void LateUpdate()
    {
        // Resetear los flags de "pressed" después de que todos los scripts los hayan leído
        jumpPressed = false;
        attackPressed = false;
        DashRightTriggered = false;
        DashLeftTriggered = false;
    }

    private void DetectDoubleTapDash()
    {
        bool movingRight = moveInput.x > 0.5f;
        bool movingLeft = moveInput.x < -0.5f;

        // Detectar inicio de movimiento a la derecha
        if (movingRight && !wasMovingRight)
        {
            if (Time.time - lastMoveRightTime < doubleTapTime)
            {
                DashRightTriggered = true;
            }
            lastMoveRightTime = Time.time;
        }

        // Detectar inicio de movimiento a la izquierda
        if (movingLeft && !wasMovingLeft)
        {
            if (Time.time - lastMoveLeftTime < doubleTapTime)
            {
                DashLeftTriggered = true;
            }
            lastMoveLeftTime = Time.time;
        }

        wasMovingRight = movingRight;
        wasMovingLeft = movingLeft;
    }

    // Callbacks de Input
    private void OnJumpStarted(InputAction.CallbackContext context)
    {
        if (hasDevice)
            jumpPressed = true;
    }

    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
        // jumpHeld se actualiza en Update()
    }

    private void OnAttackStarted(InputAction.CallbackContext context)
    {
        if (hasDevice)
            attackPressed = true;
    }

    private void OnDestroy()
    {
        // Desuscribirse de eventos
        if (jumpAction != null)
        {
            jumpAction.started -= OnJumpStarted;
            jumpAction.canceled -= OnJumpCanceled;
        }

        if (attackAction != null)
        {
            attackAction.started -= OnAttackStarted;
        }

        // Destruir la copia del InputActionAsset
        if (inputActions != null)
        {
            Destroy(inputActions);
        }
    }

    /// <summary>
    /// Devuelve información sobre el dispositivo asignado
    /// </summary>
    public string GetDeviceInfo()
    {
        if (!hasDevice || assignedDevice == null)
            return "Esperando input...";
        
        return $"{assignedDevice.displayName} ({assignedScheme})";
    }
}
