using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Cámara estilo Street Fighter para juegos de pelea 2D.
/// - Centrada horizontalmente entre jugadores
/// - Altura fija (como SF, la cámara casi no se mueve en Y)
/// - Zoom dinámico según distancia
/// - Límites del escenario (la cámara no sale del stage)
/// </summary>
[RequireComponent(typeof(Camera))]
public class DynamicCameraZoom : MonoBehaviour
{
    [Header("Targets")]
    public List<Transform> players;

    [Header("Stage Bounds (Street Fighter Style)")]
    [Tooltip("Límite izquierdo del escenario")]
    public float stageMinX = -10f;
    [Tooltip("Límite derecho del escenario")]
    public float stageMaxX = 10f;
    [Tooltip("Altura fija de la cámara (SF usa altura constante)")]
    public float fixedCameraY = 2f;

    [Header("Zoom Settings")]
    [Tooltip("Zoom cuando están cerca (orthographicSize pequeño = más cerca)")]
    public float minZoom = 4.5f;
    [Tooltip("Zoom cuando están lejos (orthographicSize grande = más lejos)")]
    public float maxZoom = 7f;
    [Tooltip("Distancia mínima antes de empezar a hacer zoom out")]
    public float zoomStartDistance = 3f;
    [Tooltip("Distancia máxima para zoom out completo")]
    public float zoomMaxDistance = 12f;

    [Header("Smoothing")]
    [Tooltip("Tiempo de suavizado para movimiento horizontal")]
    public float horizontalSmoothTime = 0.1f;
    [Tooltip("Velocidad de transición del zoom")]
    public float zoomSpeed = 4f;

    [Header("Screen Shake")]
    [Tooltip("Multiplicador del screen shake")]
    public float shakeMultiplier = 1f;

    private Camera cam;
    private Vector3 velocity;
    private float zoomVelocity;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (players == null || players.Count == 0)
            return;

        // Limpiar jugadores nulos (por si alguno fue destruido)
        players.RemoveAll(p => p == null);
        if (players.Count == 0) return;

        MoveCamera();
        ZoomCamera();
    }

    // =============================
    // CAMERA POSITION (STREET FIGHTER STYLE)
    // =============================
    void MoveCamera()
    {
        // Centro horizontal entre jugadores
        float targetX = GetHorizontalCenterX();
        
        // Calcular el ancho visible de la cámara
        float cameraHalfWidth = cam.orthographicSize * cam.aspect;
        
        // Clampear X para que la cámara no salga del escenario
        float minCameraX = stageMinX + cameraHalfWidth;
        float maxCameraX = stageMaxX - cameraHalfWidth;
        
        // Si el escenario es más pequeño que la cámara, centrar
        if (minCameraX > maxCameraX)
        {
            targetX = (stageMinX + stageMaxX) * 0.5f;
        }
        else
        {
            targetX = Mathf.Clamp(targetX, minCameraX, maxCameraX);
        }

        // Street Fighter: Y fija (la cámara casi nunca se mueve verticalmente)
        float targetY = fixedCameraY;

        Vector3 targetPosition = new Vector3(
            targetX,
            targetY,
            transform.position.z
        );

        // Suavizado del movimiento
        Vector3 smoothPosition = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            horizontalSmoothTime,
            Mathf.Infinity,
            Time.unscaledDeltaTime // Usar unscaledDeltaTime para que funcione durante HitStop
        );

        // Añadir screen shake
        Vector3 shakeOffset = Vector3.zero;
        if (ScreenShakeManager.Instance != null)
        {
            shakeOffset = ScreenShakeManager.Instance.GetShakeOffset() * shakeMultiplier;
        }

        transform.position = smoothPosition + shakeOffset;
    }

    // =============================
    // ZOOM (STREET FIGHTER STYLE)
    // =============================
    void ZoomCamera()
    {
        float distance = GetHorizontalDistance();
        
        // Calcular zoom basado en distancia con rango definido
        float t = Mathf.InverseLerp(zoomStartDistance, zoomMaxDistance, distance);
        float targetZoom = Mathf.Lerp(minZoom, maxZoom, t);

        // Suavizado del zoom (usar unscaledDeltaTime para HitStop)
        cam.orthographicSize = Mathf.SmoothDamp(
            cam.orthographicSize,
            targetZoom,
            ref zoomVelocity,
            1f / zoomSpeed,
            Mathf.Infinity,
            Time.unscaledDeltaTime
        );
    }

    // =============================
    // HELPER METHODS
    // =============================
    
    /// <summary>
    /// Obtiene el punto medio horizontal entre todos los jugadores
    /// </summary>
    float GetHorizontalCenterX()
    {
        if (players.Count == 0) return transform.position.x;
        
        float minX = float.MaxValue;
        float maxX = float.MinValue;

        foreach (Transform player in players)
        {
            if (player == null) continue;
            float x = player.position.x;
            minX = Mathf.Min(minX, x);
            maxX = Mathf.Max(maxX, x);
        }

        return (minX + maxX) * 0.5f;
    }

    /// <summary>
    /// Obtiene la distancia horizontal entre los jugadores más alejados
    /// </summary>
    float GetHorizontalDistance()
    {
        if (players.Count < 2) return 0f;

        float minX = float.MaxValue;
        float maxX = float.MinValue;

        foreach (Transform player in players)
        {
            if (player == null) continue;
            float x = player.position.x;
            minX = Mathf.Min(minX, x);
            maxX = Mathf.Max(maxX, x);
        }

        return maxX - minX;
    }

    // =============================
    // DEBUG GIZMOS
    // =============================
    void OnDrawGizmosSelected()
    {
        // Dibujar límites del escenario
        Gizmos.color = Color.red;
        float height = 10f;
        
        // Línea izquierda
        Gizmos.DrawLine(
            new Vector3(stageMinX, fixedCameraY - height/2, 0),
            new Vector3(stageMinX, fixedCameraY + height/2, 0)
        );
        
        // Línea derecha
        Gizmos.DrawLine(
            new Vector3(stageMaxX, fixedCameraY - height/2, 0),
            new Vector3(stageMaxX, fixedCameraY + height/2, 0)
        );

        // Área del escenario
        Gizmos.color = new Color(0, 1, 0, 0.1f);
        Gizmos.DrawCube(
            new Vector3((stageMinX + stageMaxX) / 2f, fixedCameraY, 0),
            new Vector3(stageMaxX - stageMinX, height, 1)
        );
    }
}
