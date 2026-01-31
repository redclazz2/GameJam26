using UnityEngine;
using System.Collections;

/// <summary>
/// Sistema de HitStop (freeze frames) para dar impacto a los golpes estilo Street Fighter
/// Singleton accesible desde cualquier script
/// </summary>
public class HitStop : MonoBehaviour
{
    public static HitStop Instance { get; private set; }

    [Header("Configuración de HitStop")]
    [SerializeField] private float defaultHitStopDuration = 0.05f;
    [SerializeField] private float defaultTimeScale = 0.1f;

    private bool isHitStopActive = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Activa el efecto de HitStop con valores por defecto
    /// </summary>
    public void TriggerHitStop()
    {
        TriggerHitStop(defaultHitStopDuration, defaultTimeScale);
    }

    /// <summary>
    /// Activa el efecto de HitStop con duración personalizada
    /// </summary>
    public void TriggerHitStop(float duration)
    {
        TriggerHitStop(duration, defaultTimeScale);
    }

    /// <summary>
    /// Activa el efecto de HitStop con parámetros personalizados
    /// </summary>
    public void TriggerHitStop(float duration, float timeScale)
    {
        if (isHitStopActive) return;
        StartCoroutine(HitStopCoroutine(duration, timeScale));
    }

    private IEnumerator HitStopCoroutine(float duration, float timeScale)
    {
        isHitStopActive = true;
        
        // Guardar el timeScale original
        float originalTimeScale = Time.timeScale;
        
        // Aplicar el freeze
        Time.timeScale = timeScale;
        
        // Esperar en tiempo real (no afectado por timeScale)
        yield return new WaitForSecondsRealtime(duration);
        
        // Restaurar el timeScale
        Time.timeScale = originalTimeScale;
        
        isHitStopActive = false;
    }
}
