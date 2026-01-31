using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Events;

/// <summary>
/// Gestor central de dispositivos de entrada para multiplayer local.
/// Detecta automáticamente qué dispositivo usa cada jugador según el orden de input.
/// El primer dispositivo que hace input es Player 1, el segundo es Player 2.
/// </summary>
public class InputDeviceManager : MonoBehaviour
{
    public static InputDeviceManager Instance { get; private set; }

    [Header("Configuración")]
    [SerializeField] private int maxPlayers = 2;
    [SerializeField] private bool allowKeyboardAsTwoPlayers = true; // WASD = P1, Flechas = P2

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    // Dispositivos asignados a cada jugador (índice 0 = Player 1, índice 1 = Player 2)
    private InputDevice[] assignedDevices;
    private string[] keyboardSchemes; // Para teclado: "KeyboardP1" o "KeyboardP2"

    // Eventos
    public UnityEvent<int, InputDevice> OnDeviceAssigned; // (playerIndex, device)
    public UnityEvent<int> OnDeviceUnassigned; // (playerIndex)

    // Referencia a los handlers registrados
    private Dictionary<int, PlayerInputHandler> registeredHandlers = new Dictionary<int, PlayerInputHandler>();

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Inicializar arrays
        assignedDevices = new InputDevice[maxPlayers];
        keyboardSchemes = new string[maxPlayers];

        // Inicializar eventos
        OnDeviceAssigned ??= new UnityEvent<int, InputDevice>();
        OnDeviceUnassigned ??= new UnityEvent<int>();
    }

    private void OnEnable()
    {
        // Escuchar TODOS los eventos de input para detectar nuevos dispositivos
        InputSystem.onEvent += OnInputEvent;
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void OnDisable()
    {
        InputSystem.onEvent -= OnInputEvent;
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    /// <summary>
    /// Procesa cada evento de input para detectar dispositivos nuevos
    /// </summary>
    private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
    {
        // Solo procesar eventos de tipo StateEvent (cambios de estado como botones)
        if (!eventPtr.IsA<StateEvent>() && !eventPtr.IsA<DeltaStateEvent>())
            return;

        // Ignorar si el dispositivo ya está asignado
        if (IsDeviceAssigned(device))
            return;

        // Verificar si hay un slot disponible
        int availableSlot = GetNextAvailableSlot();
        if (availableSlot == -1)
            return;

        // Para teclado, detectar qué teclas se presionaron
        if (device is Keyboard keyboard)
        {
            HandleKeyboardInput(keyboard, availableSlot);
            return;
        }

        // Para gamepad, verificar si hay input significativo
        if (device is Gamepad gamepad)
        {
            if (HasGamepadInput(gamepad))
            {
                AssignDeviceToSlot(device, availableSlot, "Gamepad");
            }
        }
    }

    /// <summary>
    /// Maneja el input de teclado para determinar si es WASD o Flechas
    /// </summary>
    private void HandleKeyboardInput(Keyboard keyboard, int slot)
    {
        // Determinar qué esquema de teclado se está usando
        bool isWASD = keyboard.wKey.isPressed || keyboard.aKey.isPressed || 
                      keyboard.sKey.isPressed || keyboard.dKey.isPressed ||
                      keyboard.eKey.isPressed;
        
        bool isArrows = keyboard.upArrowKey.isPressed || keyboard.downArrowKey.isPressed ||
                        keyboard.leftArrowKey.isPressed || keyboard.rightArrowKey.isPressed ||
                        keyboard.mKey.isPressed;

        if (!isWASD && !isArrows)
            return;

        string scheme = isWASD ? "KeyboardP1" : "KeyboardP2";

        // Verificar si este esquema de teclado ya está asignado
        if (IsKeyboardSchemeAssigned(scheme))
            return;

        // Si no permitimos teclado como dos jugadores, verificar si ya hay teclado asignado
        if (!allowKeyboardAsTwoPlayers && IsDeviceAssigned(keyboard))
            return;

        AssignDeviceToSlot(keyboard, slot, scheme);
    }

    /// <summary>
    /// Verifica si un gamepad tiene input significativo
    /// </summary>
    private bool HasGamepadInput(Gamepad gamepad)
    {
        // Verificar botones
        if (gamepad.buttonSouth.isPressed || gamepad.buttonNorth.isPressed ||
            gamepad.buttonEast.isPressed || gamepad.buttonWest.isPressed ||
            gamepad.startButton.isPressed || gamepad.selectButton.isPressed ||
            gamepad.leftShoulder.isPressed || gamepad.rightShoulder.isPressed)
        {
            return true;
        }

        // Verificar sticks y dpad
        if (gamepad.leftStick.ReadValue().magnitude > 0.5f ||
            gamepad.rightStick.ReadValue().magnitude > 0.5f ||
            gamepad.dpad.ReadValue().magnitude > 0.5f)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Asigna un dispositivo a un slot de jugador
    /// </summary>
    private void AssignDeviceToSlot(InputDevice device, int slot, string scheme)
    {
        assignedDevices[slot] = device;
        keyboardSchemes[slot] = scheme;

        if (showDebugLogs)
        {
            Debug.Log($"<color=green>[InputDeviceManager] Jugador {slot + 1} asignado: {device.displayName} ({scheme})</color>");
        }

        // Notificar al handler si está registrado
        if (registeredHandlers.TryGetValue(slot + 1, out PlayerInputHandler handler))
        {
            handler.OnDeviceAssigned(device, scheme);
        }

        OnDeviceAssigned?.Invoke(slot, device);
    }

    /// <summary>
    /// Detecta cambios de dispositivos (conexión/desconexión)
    /// </summary>
    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (change == InputDeviceChange.Removed || change == InputDeviceChange.Disconnected)
        {
            // Buscar si este dispositivo estaba asignado
            for (int i = 0; i < assignedDevices.Length; i++)
            {
                if (assignedDevices[i] == device)
                {
                    if (showDebugLogs)
                    {
                        Debug.Log($"<color=orange>[InputDeviceManager] Jugador {i + 1} perdió su dispositivo: {device.displayName}</color>");
                    }

                    assignedDevices[i] = null;
                    keyboardSchemes[i] = null;

                    if (registeredHandlers.TryGetValue(i + 1, out PlayerInputHandler handler))
                    {
                        handler.OnDeviceLost();
                    }

                    OnDeviceUnassigned?.Invoke(i);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Registra un PlayerInputHandler para recibir notificaciones
    /// </summary>
    public void RegisterHandler(int playerNumber, PlayerInputHandler handler)
    {
        registeredHandlers[playerNumber] = handler;

        // Si ya hay un dispositivo asignado para este jugador, notificar inmediatamente
        int slot = playerNumber - 1;
        if (slot >= 0 && slot < assignedDevices.Length && assignedDevices[slot] != null)
        {
            handler.OnDeviceAssigned(assignedDevices[slot], keyboardSchemes[slot]);
        }
    }

    /// <summary>
    /// Desregistra un PlayerInputHandler
    /// </summary>
    public void UnregisterHandler(int playerNumber)
    {
        registeredHandlers.Remove(playerNumber);
    }

    /// <summary>
    /// Obtiene el siguiente slot disponible
    /// </summary>
    private int GetNextAvailableSlot()
    {
        for (int i = 0; i < assignedDevices.Length; i++)
        {
            if (assignedDevices[i] == null)
                return i;
        }
        return -1;
    }

    /// <summary>
    /// Verifica si un dispositivo ya está asignado
    /// </summary>
    private bool IsDeviceAssigned(InputDevice device)
    {
        // Para teclado con allowKeyboardAsTwoPlayers, el mismo dispositivo puede estar en dos slots
        if (device is Keyboard && allowKeyboardAsTwoPlayers)
        {
            // Verificar si AMBOS esquemas están asignados
            return IsKeyboardSchemeAssigned("KeyboardP1") && IsKeyboardSchemeAssigned("KeyboardP2");
        }

        foreach (var assigned in assignedDevices)
        {
            if (assigned == device)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Verifica si un esquema de teclado específico ya está asignado
    /// </summary>
    private bool IsKeyboardSchemeAssigned(string scheme)
    {
        foreach (var s in keyboardSchemes)
        {
            if (s == scheme)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Resetea todas las asignaciones (útil para volver al menú)
    /// </summary>
    public void ResetAllAssignments()
    {
        for (int i = 0; i < assignedDevices.Length; i++)
        {
            if (assignedDevices[i] != null)
            {
                assignedDevices[i] = null;
                keyboardSchemes[i] = null;
                OnDeviceUnassigned?.Invoke(i);
            }
        }

        if (showDebugLogs)
        {
            Debug.Log("<color=yellow>[InputDeviceManager] Todas las asignaciones reseteadas</color>");
        }
    }

    /// <summary>
    /// Obtiene el dispositivo asignado a un jugador
    /// </summary>
    public InputDevice GetAssignedDevice(int playerNumber)
    {
        int slot = playerNumber - 1;
        if (slot >= 0 && slot < assignedDevices.Length)
            return assignedDevices[slot];
        return null;
    }

    /// <summary>
    /// Obtiene el esquema de control asignado a un jugador
    /// </summary>
    public string GetAssignedScheme(int playerNumber)
    {
        int slot = playerNumber - 1;
        if (slot >= 0 && slot < keyboardSchemes.Length)
            return keyboardSchemes[slot];
        return null;
    }

    /// <summary>
    /// Verifica si un jugador tiene dispositivo asignado
    /// </summary>
    public bool HasDeviceAssigned(int playerNumber)
    {
        return GetAssignedDevice(playerNumber) != null;
    }

    /// <summary>
    /// Obtiene el número de jugadores con dispositivo asignado
    /// </summary>
    public int GetAssignedPlayerCount()
    {
        int count = 0;
        foreach (var device in assignedDevices)
        {
            if (device != null) count++;
        }
        return count;
    }
}
