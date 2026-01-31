using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class TwoPlayerSelectionManager : MonoBehaviour
{
    [Header("Character Data")]
    [SerializeField] private List<CharacterData> availableCharacters = new List<CharacterData>();
    
    [Header("Selection Mode")]
    [SerializeField] private SelectionMode selectionMode = SelectionMode.Simultaneous;
    
    [Header("UI References")]
    [SerializeField] private GameObject characterSlotPrefab;
    [SerializeField] private Transform characterGridContainer;
    
    [Header("Player 1 UI")]
    [SerializeField] private Image p1Portrait;
    [SerializeField] private Text p1NameText;
    [SerializeField] private Text p1DescriptionText;
    [SerializeField] private GameObject p1ReadyIndicator;
    [SerializeField] private Text p1StatusText;
    
    [Header("Player 2 UI")]
    [SerializeField] private Image p2Portrait;
    [SerializeField] private Text p2NameText;
    [SerializeField] private Text p2DescriptionText;
    [SerializeField] private GameObject p2ReadyIndicator;
    [SerializeField] private Text p2StatusText;
    
    [Header("Player 1 Stats")]
    [SerializeField] private Slider p1HealthBar;
    [SerializeField] private Slider p1AttackBar;
    [SerializeField] private Slider p1DefenseBar;
    [SerializeField] private Slider p1SpeedBar;
    
    [Header("Player 2 Stats")]
    [SerializeField] private Slider p2HealthBar;
    [SerializeField] private Slider p2AttackBar;
    [SerializeField] private Slider p2DefenseBar;
    [SerializeField] private Slider p2SpeedBar;
    
    [Header("Turn Mode UI (if using turn-based)")]
    [SerializeField] private Text turnIndicatorText;
    [SerializeField] private GameObject p1TurnIndicator;
    [SerializeField] private GameObject p2TurnIndicator;
    
    [Header("Common UI")]
    [SerializeField] private Button startButton;
    [SerializeField] private Text startButtonText;
    [SerializeField] private Button backButton;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip selectSound;
    [SerializeField] private AudioClip confirmSound;
    [SerializeField] private AudioClip readySound;
    
    [Header("Settings")]
    [SerializeField] private string gameSceneName = "GameScene";
    
    // Estado de selección
    private int p1SelectedIndex = -1;
    private int p2SelectedIndex = -1;
    private bool p1Ready = false;
    private bool p2Ready = false;
    private PlayerTurn currentTurn = PlayerTurn.Player1;
    
    private List<CharacterSlot> characterSlots = new List<CharacterSlot>();
    
    // Personajes seleccionados (estáticos para acceder desde otras escenas)
    public static CharacterData Player1Character { get; private set; }
    public static CharacterData Player2Character { get; private set; }
    
    public enum SelectionMode
    {
        Simultaneous,  // Ambos jugadores eligen al mismo tiempo
        TurnBased      // Primero jugador 1, luego jugador 2
    }
    
    public enum PlayerTurn
    {
        Player1,
        Player2,
        Both
    }
    
    private void Start()
    {
        InitializeCharacterGrid();
        
        if (startButton != null)
        {
            startButton.onClick.AddListener(StartGame);
            startButton.interactable = false;
        }
        
        if (backButton != null)
            backButton.onClick.AddListener(GoBack);
        
        // Configurar UI según el modo
        SetupModeUI();
        
        // Inicializar indicadores
        UpdateReadyIndicators();
    }
    
    private void SetupModeUI()
    {
        if (selectionMode == SelectionMode.Simultaneous)
        {
            // Modo simultáneo
            if (p1StatusText != null) p1StatusText.text = "JUGADOR 1 - WASD";
            if (p2StatusText != null) p2StatusText.text = "JUGADOR 2 - FLECHAS";
            if (turnIndicatorText != null) turnIndicatorText.gameObject.SetActive(false);
            if (p1TurnIndicator != null) p1TurnIndicator.SetActive(false);
            if (p2TurnIndicator != null) p2TurnIndicator.SetActive(false);
        }
        else
        {
            // Modo por turnos
            if (turnIndicatorText != null) 
            {
                turnIndicatorText.gameObject.SetActive(true);
                UpdateTurnIndicator();
            }
            if (p1StatusText != null) p1StatusText.text = "JUGADOR 1";
            if (p2StatusText != null) p2StatusText.text = "JUGADOR 2";
        }
    }
    
    private void InitializeCharacterGrid()
    {
        foreach (Transform child in characterGridContainer)
        {
            Destroy(child.gameObject);
        }
        characterSlots.Clear();
        
        for (int i = 0; i < availableCharacters.Count; i++)
        {
            GameObject slotObj = Instantiate(characterSlotPrefab, characterGridContainer);
            CharacterSlot slot = slotObj.GetComponent<CharacterSlot>();
            
            if (slot != null)
            {
                int index = i;
                slot.Setup(availableCharacters[i], () => OnCharacterHovered(index));
                characterSlots.Add(slot);
            }
        }
    }
    
    public void OnCharacterHovered(int index)
    {
        if (index < 0 || index >= availableCharacters.Count)
            return;
        
        PlaySound(hoverSound);
    }
    
    // Llamado desde el sistema de input
    public void OnPlayer1Select(int index)
    {
        if (index < 0 || index >= availableCharacters.Count)
            return;
        
        // En modo por turnos, solo permitir si es turno del jugador 1
        if (selectionMode == SelectionMode.TurnBased && currentTurn != PlayerTurn.Player1)
            return;
        
        // Si ya está listo, no permitir cambio
        if (p1Ready)
            return;
        
        p1SelectedIndex = index;
        UpdatePlayerDisplay(1, availableCharacters[index]);
        UpdateSlotHighlights();
        PlaySound(selectSound);
    }
    
    public void OnPlayer2Select(int index)
    {
        if (index < 0 || index >= availableCharacters.Count)
            return;
        
        // En modo por turnos, solo permitir si es turno del jugador 2
        if (selectionMode == SelectionMode.TurnBased && currentTurn != PlayerTurn.Player2)
            return;
        
        // Si ya está listo, no permitir cambio
        if (p2Ready)
            return;
        
        p2SelectedIndex = index;
        UpdatePlayerDisplay(2, availableCharacters[index]);
        UpdateSlotHighlights();
        PlaySound(selectSound);
    }
    
    public void OnPlayer1Confirm()
    {
        if (p1SelectedIndex < 0 || p1Ready)
            return;
        
        // En modo por turnos, verificar que sea el turno correcto
        if (selectionMode == SelectionMode.TurnBased && currentTurn != PlayerTurn.Player1)
            return;
        
        p1Ready = true;
        PlaySound(readySound);
        UpdateReadyIndicators();
        
        // En modo por turnos, cambiar al siguiente jugador
        if (selectionMode == SelectionMode.TurnBased)
        {
            currentTurn = PlayerTurn.Player2;
            UpdateTurnIndicator();
        }
        
        CheckIfBothReady();
    }
    
    public void OnPlayer2Confirm()
    {
        if (p2SelectedIndex < 0 || p2Ready)
            return;
        
        // En modo por turnos, verificar que sea el turno correcto
        if (selectionMode == SelectionMode.TurnBased && currentTurn != PlayerTurn.Player2)
            return;
        
        p2Ready = true;
        PlaySound(readySound);
        UpdateReadyIndicators();
        
        CheckIfBothReady();
    }
    
    public void OnPlayer1Cancel()
    {
        if (!p1Ready)
            return;
        
        p1Ready = false;
        UpdateReadyIndicators();
        
        // En modo por turnos, volver al turno del jugador 1
        if (selectionMode == SelectionMode.TurnBased && currentTurn == PlayerTurn.Player2 && !p2Ready)
        {
            currentTurn = PlayerTurn.Player1;
            UpdateTurnIndicator();
        }
        
        CheckIfBothReady();
    }
    
    public void OnPlayer2Cancel()
    {
        if (!p2Ready)
            return;
        
        p2Ready = false;
        UpdateReadyIndicators();
        CheckIfBothReady();
    }
    
    private void UpdatePlayerDisplay(int playerNumber, CharacterData character)
    {
        if (playerNumber == 1)
        {
            // Portrait
            if (p1Portrait != null)
            {
                if (character.characterPortrait != null)
                {
                    p1Portrait.sprite = character.characterPortrait;
                    p1Portrait.enabled = true;
                }
                else
                {
                    p1Portrait.enabled = false;
                }
            }
            
            // Nombre y descripción
            if (p1NameText != null) p1NameText.text = character.characterName;
            if (p1DescriptionText != null) p1DescriptionText.text = character.characterDescription;
            
            // Stats
            UpdateStatBar(p1HealthBar, character.health);
            UpdateStatBar(p1AttackBar, character.attack);
            UpdateStatBar(p1DefenseBar, character.defense);
            UpdateStatBar(p1SpeedBar, character.speed);
        }
        else
        {
            // Portrait
            if (p2Portrait != null)
            {
                if (character.characterPortrait != null)
                {
                    p2Portrait.sprite = character.characterPortrait;
                    p2Portrait.enabled = true;
                }
                else
                {
                    p2Portrait.enabled = false;
                }
            }
            
            // Nombre y descripción
            if (p2NameText != null) p2NameText.text = character.characterName;
            if (p2DescriptionText != null) p2DescriptionText.text = character.characterDescription;
            
            // Stats
            UpdateStatBar(p2HealthBar, character.health);
            UpdateStatBar(p2AttackBar, character.attack);
            UpdateStatBar(p2DefenseBar, character.defense);
            UpdateStatBar(p2SpeedBar, character.speed);
        }
    }
    
    private void UpdateStatBar(Slider bar, int value)
    {
        if (bar != null)
        {
            bar.value = value / 100f;
        }
    }
    
    private void UpdateSlotHighlights()
    {
        for (int i = 0; i < characterSlots.Count; i++)
        {
            bool p1Selected = (i == p1SelectedIndex);
            bool p2Selected = (i == p2SelectedIndex);
            
            characterSlots[i].SetPlayerSelection(p1Selected, p2Selected);
        }
    }
    
    private void UpdateReadyIndicators()
    {
        if (p1ReadyIndicator != null)
            p1ReadyIndicator.SetActive(p1Ready);
        
        if (p2ReadyIndicator != null)
            p2ReadyIndicator.SetActive(p2Ready);
        
        // Actualizar texto de estado
        if (p1StatusText != null && selectionMode == SelectionMode.Simultaneous)
        {
            p1StatusText.text = p1Ready ? "LISTO!" : "JUGADOR 1 - WASD";
        }
        
        if (p2StatusText != null && selectionMode == SelectionMode.Simultaneous)
        {
            p2StatusText.text = p2Ready ? "LISTO!" : "JUGADOR 2 - FLECHAS";
        }
    }
    
    private void UpdateTurnIndicator()
    {
        if (selectionMode != SelectionMode.TurnBased)
            return;
        
        if (turnIndicatorText != null)
        {
            if (currentTurn == PlayerTurn.Player1)
                turnIndicatorText.text = "TURNO: JUGADOR 1";
            else
                turnIndicatorText.text = "TURNO: JUGADOR 2";
        }
        
        if (p1TurnIndicator != null)
            p1TurnIndicator.SetActive(currentTurn == PlayerTurn.Player1);
        
        if (p2TurnIndicator != null)
            p2TurnIndicator.SetActive(currentTurn == PlayerTurn.Player2);
    }
    
    private void CheckIfBothReady()
    {
        bool bothReady = p1Ready && p2Ready;
        
        if (startButton != null)
        {
            startButton.interactable = bothReady;
            
            if (startButtonText != null)
            {
                startButtonText.text = bothReady ? "¡COMENZAR BATALLA!" : "Esperando jugadores...";
            }
        }
    }
    
    public void StartGame()
    {
        if (!p1Ready || !p2Ready)
            return;
        
        if (p1SelectedIndex < 0 || p2SelectedIndex < 0)
            return;
        
        // Guardar selecciones
        Player1Character = availableCharacters[p1SelectedIndex];
        Player2Character = availableCharacters[p2SelectedIndex];
        
        PlaySound(confirmSound);
        
        // Cargar escena del juego
        SceneManager.LoadScene(gameSceneName);
    }
    
    public void GoBack()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    // Getters para el sistema de input
    public int GetPlayer1Index() => p1SelectedIndex;
    public int GetPlayer2Index() => p2SelectedIndex;
    public int GetTotalCharacters() => availableCharacters.Count;
    public SelectionMode GetSelectionMode() => selectionMode;
    public PlayerTurn GetCurrentTurn() => currentTurn;
}
