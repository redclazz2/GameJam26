using UnityEngine;
using System.Collections.Generic;

public class DynamicCameraZoom : MonoBehaviour
{
    [Header("Targets")]
    public List<Transform> players;

    [Header("Zoom Settings")]
    public float minZoom = 5f;
    public float maxZoom = 15f;
    public float zoomLimiter = 10f;

    [Header("Smooth Settings")]
    public float smoothTime = 0.2f;

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

    void MoveCamera()
    {
        Vector3 centerPoint = GetCenterPoint();
        Vector3 newPosition = new Vector3(
            centerPoint.x,
            centerPoint.y,
            transform.position.z
        );

        transform.position = Vector3.SmoothDamp(
            transform.position,
            newPosition,
            ref velocity,
            smoothTime
        );
    }

    void ZoomCamera()
    {
        float greatestDistance = GetGreatestDistance();
        float targetZoom = Mathf.Lerp(
            maxZoom,
            minZoom,
            greatestDistance / zoomLimiter
        );

        cam.orthographicSize = Mathf.Lerp(
            cam.orthographicSize,
            targetZoom,
            Time.deltaTime
        );
    }

    float GetGreatestDistance()
    {
        Bounds bounds = new Bounds(players[0].position, Vector3.zero);

        foreach (Transform player in players)
        {
            bounds.Encapsulate(player.position);
        }

        return Mathf.Max(bounds.size.x, bounds.size.y);
    }

    Vector3 GetCenterPoint()
    {
        if (players.Count == 1)
            return players[0].position;

        Bounds bounds = new Bounds(players[0].position, Vector3.zero);

        foreach (Transform player in players)
        {
            bounds.Encapsulate(player.position);
        }

        return bounds.center;
    }
}
