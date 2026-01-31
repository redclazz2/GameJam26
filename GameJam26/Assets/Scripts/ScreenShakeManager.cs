using UnityEngine;

public class ScreenShakeManager : MonoBehaviour
{
    public static ScreenShakeManager Instance;

    [Header("Shake Settings")]
    public float shakeDuration = 0.08f;
    public float shakeStrength = 0.15f;
    public float damping = 20f;

    private float shakeTimeRemaining;
    private float currentStrength;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

public void TriggerShake(float strengthMultiplier = 1f)
{
    Debug.Log("SHAKE TRIGGERED");
    shakeTimeRemaining = shakeDuration;
    currentStrength = shakeStrength * strengthMultiplier;
}


    public Vector3 GetShakeOffset()
    {
        if (shakeTimeRemaining <= 0f)
            return Vector3.zero;

        shakeTimeRemaining -= Time.unscaledDeltaTime;

        float damper = Mathf.Clamp01(shakeTimeRemaining * damping);

        return new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            0f
        ) * currentStrength * damper;
    }
}
