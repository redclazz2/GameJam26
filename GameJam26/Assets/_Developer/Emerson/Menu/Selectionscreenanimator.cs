using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SelectionScreenAnimator : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private CanvasGroup characterGridPanel;
    [SerializeField] private CanvasGroup detailsPanel;
    [SerializeField] private RectTransform titleText;
    
    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float slideDistance = 100f;
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private void Start()
    {
        StartCoroutine(AnimateIntro());
    }
    
    private IEnumerator AnimateIntro()
    {
        // Configuración inicial
        if (characterGridPanel != null)
        {
            characterGridPanel.alpha = 0;
            RectTransform gridRect = characterGridPanel.GetComponent<RectTransform>();
            Vector2 originalPos = gridRect.anchoredPosition;
            gridRect.anchoredPosition = new Vector2(originalPos.x - slideDistance, originalPos.y);
        }
        
        if (detailsPanel != null)
        {
            detailsPanel.alpha = 0;
            RectTransform detailsRect = detailsPanel.GetComponent<RectTransform>();
            Vector2 originalPos = detailsRect.anchoredPosition;
            detailsRect.anchoredPosition = new Vector2(originalPos.x + slideDistance, originalPos.y);
        }
        
        if (titleText != null)
        {
            titleText.localScale = Vector3.zero;
        }
        
        // Animar título
        if (titleText != null)
        {
            yield return StartCoroutine(AnimateScale(titleText, Vector3.one, 0.3f));
        }
        
        yield return new WaitForSeconds(0.1f);
        
        // Animar paneles
        if (characterGridPanel != null)
        {
            StartCoroutine(FadeInPanel(characterGridPanel, slideDistance, true));
        }
        
        if (detailsPanel != null)
        {
            StartCoroutine(FadeInPanel(detailsPanel, slideDistance, false));
        }
    }
    
    private IEnumerator FadeInPanel(CanvasGroup panel, float slideAmount, bool fromLeft)
    {
        RectTransform rectTransform = panel.GetComponent<RectTransform>();
        Vector2 targetPos = rectTransform.anchoredPosition;
        Vector2 startPos = fromLeft ? 
            new Vector2(targetPos.x - slideAmount, targetPos.y) : 
            new Vector2(targetPos.x + slideAmount, targetPos.y);
        
        float elapsed = 0f;
        
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = animationCurve.Evaluate(elapsed / fadeInDuration);
            
            panel.alpha = t;
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            
            yield return null;
        }
        
        panel.alpha = 1f;
        rectTransform.anchoredPosition = targetPos;
    }
    
    private IEnumerator AnimateScale(RectTransform target, Vector3 targetScale, float duration)
    {
        Vector3 startScale = target.localScale;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = animationCurve.Evaluate(elapsed / duration);
            
            target.localScale = Vector3.Lerp(startScale, targetScale, t);
            
            yield return null;
        }
        
        target.localScale = targetScale;
    }
    
    public void AnimateSelection(RectTransform target)
    {
        StartCoroutine(PulseAnimation(target));
    }
    
    private IEnumerator PulseAnimation(RectTransform target)
    {
        Vector3 originalScale = target.localScale;
        Vector3 targetScale = originalScale * 1.2f;
        float duration = 0.2f;
        
        // Expand
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            target.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }
        
        // Contract
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            target.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }
        
        target.localScale = originalScale;
    }
}