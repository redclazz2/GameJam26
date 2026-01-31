using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class CharacterSlot : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("UI References")]
    [SerializeField] private Image characterIcon;
    [SerializeField] private Text characterNameText;
    [SerializeField] private Image selectionFrame;
    [SerializeField] private Image background;
    
    [Header("Visual Settings")]
    [SerializeField] private Color normalColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    [SerializeField] private Color hoverColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    [SerializeField] private Color selectedColor = new Color(1f, 0.8f, 0f, 1f);
    [SerializeField] private float scaleOnHover = 1.1f;
    
    private CharacterData characterData;
    private Action onClickCallback;
    private bool isSelected = false;
    private Vector3 originalScale;
    
    private void Awake()
    {
        originalScale = transform.localScale;
        
        if (selectionFrame != null)
            selectionFrame.enabled = false;
    }
    
    public void Setup(CharacterData data, Action onClick)
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
        
        // Configurar color de fondo
        if (background != null)
        {
            background.color = normalColor;
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isSelected)
        {
            if (background != null)
                background.color = hoverColor;
            
            // Efecto de escala
            transform.localScale = originalScale * scaleOnHover;
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isSelected)
        {
            if (background != null)
                background.color = normalColor;
            
            transform.localScale = originalScale;
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        onClickCallback?.Invoke();
    }
    
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        
        if (selectionFrame != null)
            selectionFrame.enabled = selected;
        
        if (background != null)
        {
            background.color = selected ? selectedColor : normalColor;
        }
        
        // Resetear escala si no está seleccionado
        if (!selected)
        {
            transform.localScale = originalScale;
        }
        else
        {
            transform.localScale = originalScale * scaleOnHover;
        }
    }
}