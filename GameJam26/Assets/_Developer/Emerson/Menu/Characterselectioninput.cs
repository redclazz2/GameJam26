using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Navegación por teclado/gamepad para el selector de personajes
/// Estilo fighting games como Mortal Kombat
/// </summary>
public class CharacterSelectionInput : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterSelectionManager selectionManager;
    
    [Header("Navigation Settings")]
    [SerializeField] private int gridColumns = 4;
    [SerializeField] private float inputDelay = 0.15f;
    
    [Header("Key Bindings - Player 1")]
    [SerializeField] private KeyCode p1Up = KeyCode.W;
    [SerializeField] private KeyCode p1Down = KeyCode.S;
    [SerializeField] private KeyCode p1Left = KeyCode.A;
    [SerializeField] private KeyCode p1Right = KeyCode.D;
    [SerializeField] private KeyCode p1Select = KeyCode.Space;
    [SerializeField] private KeyCode p1Back = KeyCode.Escape;
    
    [Header("Gamepad Support")]
    [SerializeField] private bool enableGamepad = true;
    [SerializeField] private float joystickDeadzone = 0.3f;
    
    private int currentIndex = 0;
    private int totalCharacters = 0;
    private float lastInputTime = 0f;
    private bool hasInitialized = false;
    
    private void Start()
    {
        if (selectionManager != null)
        {
            // Obtener el número total de personajes del manager
            // Esto requeriría hacer el campo público o crear un getter
            hasInitialized = true;
        }
    }
    
    private void Update()
    {
        if (!hasInitialized || selectionManager == null)
            return;
        
        // Verificar cooldown de input
        if (Time.time - lastInputTime < inputDelay)
            return;
        
        // Navegación con teclado
        HandleKeyboardInput();
        
        // Navegación con gamepad
        if (enableGamepad)
        {
            HandleGamepadInput();
        }
    }
    
    private void HandleKeyboardInput()
    {
        int newIndex = currentIndex;
        
        // Navegación direccional
        if (Input.GetKeyDown(p1Up))
        {
            newIndex = GetIndexAbove(currentIndex);
            lastInputTime = Time.time;
        }
        else if (Input.GetKeyDown(p1Down))
        {
            newIndex = GetIndexBelow(currentIndex);
            lastInputTime = Time.time;
        }
        else if (Input.GetKeyDown(p1Left))
        {
            newIndex = GetIndexLeft(currentIndex);
            lastInputTime = Time.time;
        }
        else if (Input.GetKeyDown(p1Right))
        {
            newIndex = GetIndexRight(currentIndex);
            lastInputTime = Time.time;
        }
        
        // Si cambió el índice, actualizar selección
        if (newIndex != currentIndex)
        {
            currentIndex = newIndex;
            selectionManager.OnCharacterSelected(currentIndex);
        }
        
        // Seleccionar personaje
        if (Input.GetKeyDown(p1Select))
        {
            selectionManager.ConfirmSelection();
        }
        
        // Volver atrás
        if (Input.GetKeyDown(p1Back))
        {
            selectionManager.GoBack();
        }
    }
    
    private void HandleGamepadInput()
    {
        int newIndex = currentIndex;
        
        // Eje vertical (stick izquierdo o D-pad)
        float vertical = Input.GetAxis("Vertical");
        if (vertical > joystickDeadzone)
        {
            newIndex = GetIndexAbove(currentIndex);
            lastInputTime = Time.time;
        }
        else if (vertical < -joystickDeadzone)
        {
            newIndex = GetIndexBelow(currentIndex);
            lastInputTime = Time.time;
        }
        
        // Eje horizontal
        float horizontal = Input.GetAxis("Horizontal");
        if (horizontal < -joystickDeadzone)
        {
            newIndex = GetIndexLeft(currentIndex);
            lastInputTime = Time.time;
        }
        else if (horizontal > joystickDeadzone)
        {
            newIndex = GetIndexRight(currentIndex);
            lastInputTime = Time.time;
        }
        
        // Actualizar si cambió
        if (newIndex != currentIndex && Time.time - lastInputTime >= inputDelay)
        {
            currentIndex = newIndex;
            selectionManager.OnCharacterSelected(currentIndex);
        }
        
        // Botones de gamepad
        if (Input.GetButtonDown("Submit")) // Típicamente botón A
        {
            selectionManager.ConfirmSelection();
        }
        
        if (Input.GetButtonDown("Cancel")) // Típicamente botón B
        {
            selectionManager.GoBack();
        }
    }
    
    private int GetIndexAbove(int current)
    {
        int newIndex = current - gridColumns;
        if (newIndex < 0)
        {
            // Wrap around - ir al final de esa columna
            int column = current % gridColumns;
            int rows = Mathf.CeilToInt((float)totalCharacters / gridColumns);
            newIndex = (rows - 1) * gridColumns + column;
            
            // Ajustar si excede el total
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
            // Wrap around - ir al principio de esa columna
            newIndex = current % gridColumns;
        }
        return Mathf.Clamp(newIndex, 0, totalCharacters - 1);
    }
    
    private int GetIndexLeft(int current)
    {
        int newIndex = current - 1;
        int currentRow = current / gridColumns;
        int newRow = newIndex / gridColumns;
        
        // Si cambiamos de fila, hacer wrap
        if (newIndex < 0 || newRow != currentRow)
        {
            // Ir al final de esta fila
            newIndex = Mathf.Min((currentRow + 1) * gridColumns - 1, totalCharacters - 1);
        }
        
        return Mathf.Clamp(newIndex, 0, totalCharacters - 1);
    }
    
    private int GetIndexRight(int current)
    {
        int newIndex = current + 1;
        int currentRow = current / gridColumns;
        int newRow = newIndex / gridColumns;
        
        // Si cambiamos de fila o excedemos total, hacer wrap
        if (newIndex >= totalCharacters || newRow != currentRow)
        {
            // Ir al principio de esta fila
            newIndex = currentRow * gridColumns;
        }
        
        return Mathf.Clamp(newIndex, 0, totalCharacters - 1);
    }
    
    public void SetTotalCharacters(int total)
    {
        totalCharacters = total;
    }
    
    public void SetGridColumns(int columns)
    {
        gridColumns = columns;
    }
    
    public void ResetToFirstCharacter()
    {
        currentIndex = 0;
        if (selectionManager != null)
        {
            selectionManager.OnCharacterSelected(currentIndex);
        }
    }
}