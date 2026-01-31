using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class CharacterSelectionManager : MonoBehaviour
{
    [Header("Character Data")]
    [SerializeField] private List<CharacterData> availableCharacters = new List<CharacterData>();
    
    [Header("UI References")]
    [SerializeField] private GameObject characterSlotPrefab;
    [SerializeField] private Transform characterGridContainer;
    [SerializeField] private Image characterPortraitDisplay;
    [SerializeField] private Text characterNameText;
    [SerializeField] private Text characterDescriptionText;
    [SerializeField] private Button selectButton;
    [SerializeField] private Button backButton;
    
    [Header("Stats Display")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private Slider attackBar;
    [SerializeField] private Slider defenseBar;
    [SerializeField] private Slider speedBar;
    [SerializeField] private Text healthText;
    [SerializeField] private Text attackText;
    [SerializeField] private Text defenseText;
    [SerializeField] private Text speedText;
    
    [Header("Preview")]
    [SerializeField] private Transform characterPreviewPosition;
    [SerializeField] private float rotationSpeed = 50f;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip selectSound;
    [SerializeField] private AudioClip confirmSound;
    
    [Header("Settings")]
    [SerializeField] private string gameSceneName = "GameScene";
    
    private int currentSelectedIndex = -1;
    private GameObject currentPreviewCharacter;
    private List<CharacterSlot> characterSlots = new List<CharacterSlot>();
    
    public static CharacterData SelectedCharacter { get; private set; }
    
    private void Start()
    {
        InitializeCharacterGrid();
        
        if (selectButton != null)
            selectButton.onClick.AddListener(ConfirmSelection);
        
        if (backButton != null)
            backButton.onClick.AddListener(GoBack);
        
        if (selectButton != null)
            selectButton.interactable = false;
    }
    
    private void Update()
    {
        // Rotar el personaje preview
        if (currentPreviewCharacter != null)
        {
            currentPreviewCharacter.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }
    
    private void InitializeCharacterGrid()
    {
        // Limpiar slots existentes
        foreach (Transform child in characterGridContainer)
        {
            Destroy(child.gameObject);
        }
        characterSlots.Clear();
        
        // Crear slots para cada personaje
        for (int i = 0; i < availableCharacters.Count; i++)
        {
            GameObject slotObj = Instantiate(characterSlotPrefab, characterGridContainer);
            CharacterSlot slot = slotObj.GetComponent<CharacterSlot>();
            
            if (slot != null)
            {
                int index = i; // Capturar índice para el closure
                slot.Setup(availableCharacters[i], () => OnCharacterSelected(index));
                characterSlots.Add(slot);
            }
        }
    }
    
    public void OnCharacterSelected(int index)
    {
        if (index < 0 || index >= availableCharacters.Count)
            return;
        
        currentSelectedIndex = index;
        CharacterData character = availableCharacters[index];
        
        // Actualizar UI
        UpdateCharacterDisplay(character);
        
        // Actualizar selección visual en los slots
        for (int i = 0; i < characterSlots.Count; i++)
        {
            characterSlots[i].SetSelected(i == index);
        }
        
        // Reproducir sonido
        PlaySound(hoverSound);
        
        // Habilitar botón de selección
        if (selectButton != null)
            selectButton.interactable = true;
        
        // Actualizar preview 3D
        UpdateCharacterPreview(character);
    }
    
    private void UpdateCharacterDisplay(CharacterData character)
    {
        // Portrait
        if (characterPortraitDisplay != null)
        {
            if (character.characterPortrait != null)
            {
                characterPortraitDisplay.sprite = character.characterPortrait;
                characterPortraitDisplay.enabled = true;
            }
            else
            {
                characterPortraitDisplay.enabled = false;
            }
        }
        
        // Nombre
        if (characterNameText != null)
            characterNameText.text = character.characterName;
        
        // Descripción
        if (characterDescriptionText != null)
            characterDescriptionText.text = character.characterDescription;
        
        // Stats
        UpdateStatBar(healthBar, healthText, character.health, "Health");
        UpdateStatBar(attackBar, attackText, character.attack, "Attack");
        UpdateStatBar(defenseBar, defenseText, character.defense, "Defense");
        UpdateStatBar(speedBar, speedText, character.speed, "Speed");
    }
    
    private void UpdateStatBar(Slider bar, Text text, int value, string statName)
    {
        if (bar != null)
        {
            bar.value = value / 100f; // Normalizar a 0-1
        }
        
        if (text != null)
        {
            text.text = $"{statName}: {value}";
        }
    }
    
    private void UpdateCharacterPreview(CharacterData character)
    {
        // Destruir preview anterior
        if (currentPreviewCharacter != null)
        {
            Destroy(currentPreviewCharacter);
        }
        
        // Crear nuevo preview si hay prefab
        if (character.characterPrefab != null && characterPreviewPosition != null)
        {
            currentPreviewCharacter = Instantiate(
                character.characterPrefab, 
                characterPreviewPosition.position, 
                characterPreviewPosition.rotation
            );
            
            // Opcional: Desactivar componentes innecesarios para el preview
            Rigidbody rb = currentPreviewCharacter.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;
            
            // Aplicar animator si existe
            if (character.characterAnimator != null)
            {
                Animator animator = currentPreviewCharacter.GetComponent<Animator>();
                if (animator != null)
                {
                    animator.runtimeAnimatorController = character.characterAnimator;
                }
            }
        }
    }
    
    public void ConfirmSelection()
    {
        if (currentSelectedIndex < 0 || currentSelectedIndex >= availableCharacters.Count)
            return;
        
        SelectedCharacter = availableCharacters[currentSelectedIndex];
        PlaySound(confirmSound);
        
        // Cargar escena del juego
        SceneManager.LoadScene(gameSceneName);
    }
    
    public void GoBack()
    {
        // Volver al menú principal o escena anterior
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    private void OnDestroy()
    {
        if (currentPreviewCharacter != null)
        {
            Destroy(currentPreviewCharacter);
        }
    }
}