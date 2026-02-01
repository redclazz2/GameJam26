using UnityEngine;
using TMPro;
using System.Collections;

public class CountdownTextFX : MonoBehaviour
{
    public TextMeshProUGUI text;

    [Header("Blink")]
    public float blinkSpeed = 0.15f;

    [Header("Shake")]
    public float shakeDuration = 0.4f;
    public float shakeStrength = 8f;

    private Coroutine blinkRoutine;
    private Vector3 originalPos;

    void Awake()
    {
        originalPos = text.rectTransform.anchoredPosition;
    }

    // =========================
    // BLINK
    // =========================
    public void StartBlink()
    {
        StopBlink();
        blinkRoutine = StartCoroutine(BlinkCoroutine());
    }

    public void StopBlink()
    {
        if (blinkRoutine != null)
            StopCoroutine(blinkRoutine);

        text.enabled = true;
    }

    IEnumerator BlinkCoroutine()
    {
        while (true)
        {
            text.enabled = !text.enabled;
            yield return new WaitForSeconds(blinkSpeed);
        }
    }

    // =========================
    // SHAKE
    // =========================
    public void Shake()
    {
        StartCoroutine(ShakeCoroutine());
    }

    IEnumerator ShakeCoroutine()
    {
        float timer = 0f;

        while (timer < shakeDuration)
        {
            Vector2 randomOffset = Random.insideUnitCircle * shakeStrength;
            text.rectTransform.anchoredPosition = originalPos + (Vector3)randomOffset;

            timer += Time.deltaTime;
            yield return null;
        }

        text.rectTransform.anchoredPosition = originalPos;
    }
}
