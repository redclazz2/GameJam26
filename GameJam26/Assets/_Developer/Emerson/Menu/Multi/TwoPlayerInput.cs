using UnityEngine;

/// <summary>
/// Sistema de input para 2 jugadores
/// P1: WASD + Space para confirmar, Backspace para cancelar
/// P2: Flechas + Enter para confirmar, Delete para cancelar
/// </summary>
public class TwoPlayerInput : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TwoPlayerSelectionManager selectionManager;
    
    [Header("Navigation Settings")]
    [SerializeField] private int gridColumns = 4;
    [SerializeField] private float inputDelay = 0.15f;
    
    [Header("Player 1 Controls (WASD)")]
    [SerializeField] private KeyCode p1Up = KeyCode.W;
    [SerializeField] private KeyCode p1Down = KeyCode.S;
    [SerializeField] private KeyCode p1Left = KeyCode.A;
    [SerializeField] private KeyCode p1Right = KeyCode.D;
    [SerializeField] private KeyCode p1Confirm = KeyCode.Space;
    [SerializeField] private KeyCode p1Cancel = KeyCode.Backspace;
    
    [Header("Player 2 Controls (Arrows)")]
    [SerializeField] private KeyCode p2Up = KeyCode.UpArrow;
    [SerializeField] private KeyCode p2Down = KeyCode.DownArrow;
    [SerializeField] private KeyCode p2Left = KeyCode.LeftArrow;
    [SerializeField] private KeyCode p2Right = KeyCode.RightArrow;
    [SerializeField] private KeyCode p2Confirm = KeyCode.Return;
    [SerializeField] private KeyCode p2Cancel = KeyCode.Delete;
    
    [Header("Gamepad Support")]
    [SerializeField] private bool enableGamepad = true;
    [SerializeField] private float joystickDeadzone = 0.5f;
    
    private int p1CurrentIndex = 0;
    private int p2CurrentIndex = 1; // Empieza en posición diferente
    private int totalCharacters = 0;
    
    private float p1LastInputTime = 0f;
    private float p2LastInputTime = 0f;
    
    private bool p1VerticalPressed = false;
    private bool p2VerticalPressed = false;
    private bool p1HorizontalPressed = false;
    private bool p2HorizontalPressed = false;
    
    private void Start()
    {
        if (selectionManager != null)
        {
            totalCharacters = selectionManager.GetTotalCharacters();
            
            // Seleccionar inicialmente
            selectionManager.OnPlayer1Select(p1CurrentIndex);
            selectionManager.OnPlayer2Select(p2CurrentIndex);
        }
    }
    
    private void Update()
    {
        if (selectionManager == null || totalCharacters == 0)
            return;
        
        // Input del jugador 1
        HandlePlayer1Input();
        
        // Input del jugador 2
        HandlePlayer2Input();
        
        // Gamepad (opcional)
        if (enableGamepad)
        {
            HandleGamepadInput();
        }
    }
    
    private void HandlePlayer1Input()
    {
        var mode = selectionManager.GetSelectionMode();
        var turn = selectionManager.GetCurrentTurn();
        
        // En modo por turnos, solo permitir input si es el turno de P1
        if (mode == TwoPlayerSelectionManager.SelectionMode.TurnBased && 
            turn != TwoPlayerSelectionManager.PlayerTurn.Player1)
            return;
        
        // Navegación
        if (Time.time - p1LastInputTime >= inputDelay)
        {
            int newIndex = p1CurrentIndex;
            bool moved = false;
            
            // Vertical
            if (Input.GetKey(p1Up) && !p1VerticalPressed)
            {
                newIndex = GetIndexAbove(p1CurrentIndex);
                p1VerticalPressed = true;
                moved = true;
            }
            else if (Input.GetKey(p1Down) && !p1VerticalPressed)
            {
                newIndex = GetIndexBelow(p1CurrentIndex);
                p1VerticalPressed = true;
                moved = true;
            }
            else if (!Input.GetKey(p1Up) && !Input.GetKey(p1Down))
            {
                p1VerticalPressed = false;
            }
            
            // Horizontal
            if (Input.GetKey(p1Left) && !p1HorizontalPressed)
            {
                newIndex = GetIndexLeft(p1CurrentIndex);
                p1HorizontalPressed = true;
                moved = true;
            }
            else if (Input.GetKey(p1Right) && !p1HorizontalPressed)
            {
                newIndex = GetIndexRight(p1CurrentIndex);
                p1HorizontalPressed = true;
                moved = true;
            }
            else if (!Input.GetKey(p1Left) && !Input.GetKey(p1Right))
            {
                p1HorizontalPressed = false;
            }
            
            if (moved && newIndex != p1CurrentIndex)
            {
                p1CurrentIndex = newIndex;
                selectionManager.OnPlayer1Select(p1CurrentIndex);
                p1LastInputTime = Time.time;
            }
        }
        
        // Confirmar
        if (Input.GetKeyDown(p1Confirm))
        {
            selectionManager.OnPlayer1Confirm();
        }
        
        // Cancelar
        if (Input.GetKeyDown(p1Cancel))
        {
            selectionManager.OnPlayer1Cancel();
        }
    }
    
    private void HandlePlayer2Input()
    {
        var mode = selectionManager.GetSelectionMode();
        var turn = selectionManager.GetCurrentTurn();
        
        // En modo por turnos, solo permitir input si es el turno de P2
        if (mode == TwoPlayerSelectionManager.SelectionMode.TurnBased && 
            turn != TwoPlayerSelectionManager.PlayerTurn.Player2)
            return;
        
        // Navegación
        if (Time.time - p2LastInputTime >= inputDelay)
        {
            int newIndex = p2CurrentIndex;
            bool moved = false;
            
            // Vertical
            if (Input.GetKey(p2Up) && !p2VerticalPressed)
            {
                newIndex = GetIndexAbove(p2CurrentIndex);
                p2VerticalPressed = true;
                moved = true;
            }
            else if (Input.GetKey(p2Down) && !p2VerticalPressed)
            {
                newIndex = GetIndexBelow(p2CurrentIndex);
                p2VerticalPressed = true;
                moved = true;
            }
            else if (!Input.GetKey(p2Up) && !Input.GetKey(p2Down))
            {
                p2VerticalPressed = false;
            }
            
            // Horizontal
            if (Input.GetKey(p2Left) && !p2HorizontalPressed)
            {
                newIndex = GetIndexLeft(p2CurrentIndex);
                p2HorizontalPressed = true;
                moved = true;
            }
            else if (Input.GetKey(p2Right) && !p2HorizontalPressed)
            {
                newIndex = GetIndexRight(p2CurrentIndex);
                p2HorizontalPressed = true;
                moved = true;
            }
            else if (!Input.GetKey(p2Left) && !Input.GetKey(p2Right))
            {
                p2HorizontalPressed = false;
            }
            
            if (moved && newIndex != p2CurrentIndex)
            {
                p2CurrentIndex = newIndex;
                selectionManager.OnPlayer2Select(p2CurrentIndex);
                p2LastInputTime = Time.time;
            }
        }
        
        // Confirmar
        if (Input.GetKeyDown(p2Confirm))
        {
            selectionManager.OnPlayer2Confirm();
        }
        
        // Cancelar
        if (Input.GetKeyDown(p2Cancel))
        {
            selectionManager.OnPlayer2Cancel();
        }
    }
    
    private void HandleGamepadInput()
    {
        // Joystick 1 (Player 1)
        float j1Vertical = Input.GetAxis("Vertical");
        float j1Horizontal = Input.GetAxis("Horizontal");
        
        if (Time.time - p1LastInputTime >= inputDelay)
        {
            int newIndex = p1CurrentIndex;
            bool moved = false;
            
            if (j1Vertical > joystickDeadzone)
            {
                newIndex = GetIndexAbove(p1CurrentIndex);
                moved = true;
            }
            else if (j1Vertical < -joystickDeadzone)
            {
                newIndex = GetIndexBelow(p1CurrentIndex);
                moved = true;
            }
            
            if (j1Horizontal < -joystickDeadzone)
            {
                newIndex = GetIndexLeft(p1CurrentIndex);
                moved = true;
            }
            else if (j1Horizontal > joystickDeadzone)
            {
                newIndex = GetIndexRight(p1CurrentIndex);
                moved = true;
            }
            
            if (moved && newIndex != p1CurrentIndex)
            {
                p1CurrentIndex = newIndex;
                selectionManager.OnPlayer1Select(p1CurrentIndex);
                p1LastInputTime = Time.time;
            }
        }
        
        // Botones de gamepad
        if (Input.GetButtonDown("Submit"))
        {
            selectionManager.OnPlayer1Confirm();
        }
        if (Input.GetButtonDown("Cancel"))
        {
            selectionManager.OnPlayer1Cancel();
        }
    }
    
    // Funciones de navegación
    private int GetIndexAbove(int current)
    {
        int newIndex = current - gridColumns;
        if (newIndex < 0)
        {
            // Wrap around al final
            int column = current % gridColumns;
            int rows = Mathf.CeilToInt((float)totalCharacters / gridColumns);
            newIndex = (rows - 1) * gridColumns + column;
            
            if (newIndex >= totalCharacters)
                newIndex -= gridColumns;
        }
        return Mathf.Clamp(newIndex, 0, totalCharacters - 1);
    }
    
    private int GetIndexBelow(int current)
    {
        int newIndex = current + gridColumns;
        if (newIndex >= totalCharacters)
        {
            // Wrap around al principio
            newIndex = current % gridColumns;
        }
        return Mathf.Clamp(newIndex, 0, totalCharacters - 1);
    }
    
    private int GetIndexLeft(int current)
    {
        int newIndex = current - 1;
        int currentRow = current / gridColumns;
        int newRow = newIndex / gridColumns;
        
        if (newIndex < 0 || newRow != currentRow)
        {
            // Wrap al final de la fila
            newIndex = Mathf.Min((currentRow + 1) * gridColumns - 1, totalCharacters - 1);
        }
        
        return Mathf.Clamp(newIndex, 0, totalCharacters - 1);
    }
    
    private int GetIndexRight(int current)
    {
        int newIndex = current + 1;
        int currentRow = current / gridColumns;
        int newRow = newIndex / gridColumns;
        
        if (newIndex >= totalCharacters || newRow != currentRow)
        {
            // Wrap al principio de la fila
            newIndex = currentRow * gridColumns;
        }
        
        return Mathf.Clamp(newIndex, 0, totalCharacters - 1);
    }
    
    public void SetGridColumns(int columns)
    {
        gridColumns = columns;
    }
}
