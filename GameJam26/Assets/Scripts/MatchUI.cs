using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI para mostrar información de la partida (tiempo, puntos, barras de vida)
/// Requiere TextMeshPro instalado en el proyecto
/// </summary>
public class MatchUI : MonoBehaviour
{
    [Header("Referencias de Tiempo")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Referencias de Puntos")]
    [SerializeField] private TextMeshProUGUI player1PointsText;
    [SerializeField] private TextMeshProUGUI player2PointsText;
    
    // Alternativa: usar imágenes para los puntos (como en Street Fighter)
    [SerializeField] private Image[] player1PointIndicators;
    [SerializeField] private Image[] player2PointIndicators;
    [SerializeField] private Color pointActiveColor = Color.yellow;
    [SerializeField] private Color pointInactiveColor = Color.gray;

    [Header("Referencias de Barras de Vida")]
    [SerializeField] private Image player1HealthBar;
    [SerializeField] private Image player2HealthBar;
    [SerializeField] private Player player1;
    [SerializeField] private Player player2;

    [Header("Paneles de Fin de Ronda/Match")]
    [SerializeField] private GameObject roundEndPanel;
    [SerializeField] private TextMeshProUGUI roundEndText;
    [SerializeField] private GameObject matchEndPanel;
    [SerializeField] private TextMeshProUGUI matchEndText;

    [Header("Countdown")]
    [SerializeField] private TextMeshProUGUI countdownText;

    private MatchManager matchManager;

    private void Start()
    {
        matchManager = MatchManager.Instance;
        
        if (matchManager == null)
        {
            Debug.LogWarning("MatchManager no encontrado. MatchUI no funcionará correctamente.");
            return;
        }

        // Suscribirse a eventos del MatchManager
        matchManager.OnTimeChanged.AddListener(UpdateTimer);
        matchManager.OnPointsChanged.AddListener(UpdatePoints);
        matchManager.OnRoundEnd.AddListener(ShowRoundEnd);
        matchManager.OnMatchEnd.AddListener(ShowMatchEnd);
        matchManager.OnRoundStart.AddListener(HideEndPanels);
        matchManager.OnCountdownChanged.AddListener(UpdateCountdown);

        // Suscribirse a eventos de salud de los jugadores
        if (player1 != null)
        {
            player1.OnHealthChanged.AddListener((_) => UpdateHealthBars());
        }
        if (player2 != null)
        {
            player2.OnHealthChanged.AddListener((_) => UpdateHealthBars());
        }

        // Inicializar UI
        UpdatePoints(0, 0);
        UpdateHealthBars();
        HideEndPanels();
    }

    private void UpdateTimer(float time)
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";

            // Cambiar color si queda poco tiempo (últimos 10 segundos)
            if (time <= 10f)
            {
                timerText.color = Color.red;
            }
            else if (time <= 30f)
            {
                timerText.color = Color.yellow;
            }
            else
            {
                timerText.color = Color.white;
            }
        }
    }

    private void UpdateCountdown(string countdownValue)
    {
        if (countdownText == null) return;

        if (string.IsNullOrEmpty(countdownValue))
        {
            countdownText.gameObject.SetActive(false);
            
            // Mostrar el timer cuando termina el countdown
            if (timerText != null)
            {
                timerText.gameObject.SetActive(true);
            }
        }
        else
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = countdownValue;

            // Color especial para "GO!"
            countdownText.color = countdownValue == "GO!" ? Color.green : Color.black;
        }
    }

    private void UpdatePoints(int p1Points, int p2Points)
    {
        // Actualizar texto de puntos
        if (player1PointsText != null)
        {
            player1PointsText.text = p1Points.ToString();
        }
        if (player2PointsText != null)
        {
            player2PointsText.text = p2Points.ToString();
        }

        // Actualizar indicadores visuales de puntos (estilo Street Fighter)
        UpdatePointIndicators(player1PointIndicators, p1Points);
        UpdatePointIndicators(player2PointIndicators, p2Points);
    }

    private void UpdatePointIndicators(Image[] indicators, int points)
    {
        if (indicators == null) return;

        for (int i = 0; i < indicators.Length; i++)
        {
            if (indicators[i] != null)
            {
                indicators[i].color = i < points ? pointActiveColor : pointInactiveColor;
            }
        }
    }

    private void UpdateHealthBars()
    {
        if (player1HealthBar != null && player1 != null)
        {
            player1HealthBar.fillAmount = player1.CurrentHealth / player1.MaxHealth;
        }
        if (player2HealthBar != null && player2 != null)
        {
            player2HealthBar.fillAmount = player2.CurrentHealth / player2.MaxHealth;
        }
    }

    private void ShowRoundEnd(int winner)
    {
        // Ocultar el timer al finalizar la ronda
        if (timerText != null)
        {
            timerText.gameObject.SetActive(false);
        }

        if (roundEndPanel != null)
        {
            roundEndPanel.SetActive(true);
            
            if (roundEndText != null)
            {
                if (winner == 0)
                {
                    roundEndText.text = "¡EMPATE!";
                }
                else
                {
                    roundEndText.text = $"¡JUGADOR {winner} GANA LA RONDA!";
                }
            }
        }
    }

    private void ShowMatchEnd(int winner)
    {
        // Ocultar el timer al finalizar el match
        if (timerText != null)
        {
            timerText.gameObject.SetActive(false);
        }

        if (matchEndPanel != null)
        {
            matchEndPanel.SetActive(true);
            
            if (matchEndText != null)
            {
                matchEndText.text = $"¡JUGADOR {winner}\nGANA EL MATCH!";
            }
        }
    }

    private void HideEndPanels()
    {
        if (roundEndPanel != null)
        {
            roundEndPanel.SetActive(false);
        }
        if (matchEndPanel != null)
        {
            matchEndPanel.SetActive(false);
        }

        // Ocultar timer durante el countdown (se mostrará cuando termine)
        if (timerText != null)
        {
            timerText.gameObject.SetActive(false);
            timerText.color = Color.white;
        }
    }

    /// <summary>
    /// Método público para reiniciar el match desde un botón
    /// </summary>
    public void OnRestartButtonClicked()
    {
        matchManager?.RestartMatch();
    }

    private void OnDestroy()
    {
        // Desuscribirse de eventos
        if (matchManager != null)
        {
            matchManager.OnTimeChanged.RemoveListener(UpdateTimer);
            matchManager.OnPointsChanged.RemoveListener(UpdatePoints);
            matchManager.OnRoundEnd.RemoveListener(ShowRoundEnd);
            matchManager.OnMatchEnd.RemoveListener(ShowMatchEnd);
            matchManager.OnRoundStart.RemoveListener(HideEndPanels);
            matchManager.OnCountdownChanged.RemoveListener(UpdateCountdown);
        }
    }
}
