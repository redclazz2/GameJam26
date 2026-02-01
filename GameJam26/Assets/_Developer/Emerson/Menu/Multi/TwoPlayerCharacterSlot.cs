using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Slot de personaje mejorado para 2 jugadores
/// Muestra indicadores separados para cada jugador
/// </summary>
public class TwoPlayerCharacterSlot : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image characterIcon;
    [SerializeField] private Text characterNameText;
    [SerializeField] private Image background;
    
    [Header("Player Indicators")]
    [SerializeField] private GameObject player1Indicator; // Indicador para jugador 1 (ej: borde azul)
    [SerializeField] private GameObject player2Indicator; // Indicador para jugador 2 (ej: borde rojo)
    
    [Header("Visual Settings")]
    [SerializeField] private Color normalColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    [SerializeField] private Color player1Color = new Color(0.2f, 0.5f, 1f, 1f); // Azul
    [SerializeField] private Color player2Color = new Color(1f, 0.3f, 0.2f, 1f); // Rojo
    [SerializeField] private Color bothPlayersColor = new Color(0.8f, 0.8f, 0f, 1f); // Amarillo
    
    private CharacterData characterData;
    private System.Action onClickCallback;
    private bool p1Selected = false;
    private bool p2Selected = false;
    
    private void Awake()
    {
        if (player1Indicator != null)
            player1Indicator.SetActive(false);
        
        if (player2Indicator != null)
            player2Indicator.SetActive(false);
    }
    
    public void Setup(CharacterData data, System.Action onClick)
    {
        characterData = data;
        onClickCallback = onClick;
        
        // Configurar icono
        if (characterIcon != null && data.characterIcon != null)
        {
            characterIcon.sprite = data.characterIcon;
        }
        
        // Configurar nombre
        if (characterNameText != null)
        {
            characterNameText.text = data.characterName;
        }
        
        // Color inicial
        if (background != null)
        {
            background.color = normalColor;
        }
    }
    
    public void SetPlayerSelection(bool player1, bool player2)
    {
        p1Selected = player1;
        p2Selected = player2;
        
        // Activar/desactivar indicadores
        if (player1Indicator != null)
            player1Indicator.SetActive(player1);
        
        if (player2Indicator != null)
            player2Indicator.SetActive(player2);
        
        // Cambiar color de fondo
        if (background != null)
        {
            if (player1 && player2)
            {
                // Ambos jugadores seleccionaron este personaje
                background.color = bothPlayersColor;
            }
            else if (player1)
            {
                background.color = player1Color;
            }
            else if (player2)
            {
                background.color = player2Color;
            }
            else
            {
                background.color = normalColor;
            }
        }
    }
}