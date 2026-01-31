using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Camera))]
public class DynamicCameraZoom : MonoBehaviour
{
    [Header("Targets")]
    public List<Transform> players;

    [Header("Stage")]
    public float stageBaseY = 0f;   // Altura del piso del escenario
    public float minCameraY = 2f;   // Nunca más abajo que esto

    [Header("Zoom Settings")]
    public float minZoom = 5f;
    public float maxZoom = 9f;
    public float zoomLimiter = 8f;

    [Header("Vertical Offset (Fighting Game)")]
    public float maxYOffset = 1.2f;
    public float closeDeadZone = 2.8f;        // Offset = 0 cuando están cerca
    public float offsetStartDistance = 3.5f;  // Empieza a subir
    public float offsetFullDistance = 8f;     // Offset máximo
    public AnimationCurve offsetCurve;        // Ease-in (MUY importante)

    [Header("Smooth")]
    public float smoothTime = 0.12f;

    private Camera cam;
    private Vector3 velocity;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (players == null || players.Count == 0)
            return;

        MoveCamera();
        ZoomCamera();
    }

    // =============================
    // CAMERA POSITION (X + Y LOCK)
    // =============================
    void MoveCamera()
    {
        float distance = GetHorizontalDistance();

        float yOffset = CalculateYOffset(distance);

        float targetX = GetHorizontalCenterX();
        float targetY = stageBaseY + yOffset;
        targetY = Mathf.Max(targetY, minCameraY);

        // 👉 ESTA LÍNEA FALTABA
        Vector3 targetPosition = new Vector3(
            targetX,
            targetY,
            transform.position.z
        );

Vector3 smoothPosition = Vector3.SmoothDamp(
    transform.position,
    targetPosition,
    ref velocity,
    smoothTime
);

Vector3 shakeOffset = ScreenShakeManager.Instance != null
    ? ScreenShakeManager.Instance.GetShakeOffset()
    : Vector3.zero;

transform.position = smoothPosition + shakeOffset;

    }


    // =============================
    // ZOOM (SOLO DISTANCIA X)
    // =============================
    void ZoomCamera()
    {
        float distance = GetHorizontalDistance();
        float t = Mathf.Clamp01(distance / zoomLimiter);

        float targetZoom = Mathf.Lerp(minZoom, maxZoom, t);

        cam.orthographicSize = Mathf.Lerp(
            cam.orthographicSize,
            targetZoom,
            Time.deltaTime * 5f
        );
    }

    // =============================
    // OFFSET LOGIC (PRO)
    // =============================
    float CalculateYOffset(float distance)
    {
        // Dead zone: NO offset en combate cercano
        if (distance <= closeDeadZone)
            return 0f;

        float t = Mathf.InverseLerp(
            offsetStartDistance,
            offsetFullDistance,
            distance
        );

        // Curva para que NO reaccione fuerte al inicio
        if (offsetCurve != null && offsetCurve.length > 0)
            t = offsetCurve.Evaluate(t);

        return Mathf.Lerp(0f, maxYOffset, t);
    }

    // =============================
    // HORIZONTAL CALCULATIONS ONLY
    // =============================
    float GetHorizontalCenterX()
    {
        float minX = players[0].position.x;
        float maxX = players[0].position.x;

        foreach (Transform player in players)
        {
            float x = player.position.x;
            minX = Mathf.Min(minX, x);
            maxX = Mathf.Max(maxX, x);
        }

        return (minX + maxX) * 0.5f;
    }

    float GetHorizontalDistance()
    {
        if (players.Count < 2)
            return 0f;

        float minX = players[0].position.x;
        float maxX = players[0].position.x;

        foreach (Transform player in players)
        {
            float x = player.position.x;
            minX = Mathf.Min(minX, x);
            maxX = Mathf.Max(maxX, x);
        }

        return maxX - minX;
    }
}
