using System;
using UnityEngine;
using UnityEngine.Events;

public class MatchManager : MonoBehaviour
{
    public static MatchManager Instance { get; private set; }

    [Header("Jugadores")]
    [SerializeField] private Player player1;
    [SerializeField] private Player player2;

    [Header("Spawn Points")]
    [SerializeField] private Transform player1SpawnPoint;
    [SerializeField] private Transform player2SpawnPoint;

    [Header("Configuración de Partida")]
    [SerializeField] private float roundTime = 100f; // 2 minutos
    [SerializeField] private int pointsToWin = 3;

    [Header("Estado Actual")]
    [SerializeField] private int player1Points = 0;
    [SerializeField] private int player2Points = 0;
    [SerializeField] private float currentTime;
    [SerializeField] private bool isRoundActive = false;
    [SerializeField] private bool isMatchOver = false;

    // Eventos para UI y otros sistemas
    public UnityEvent<float> OnTimeChanged;
    public UnityEvent<int, int> OnPointsChanged; // player1Points, player2Points
    public UnityEvent<int> OnRoundEnd; // ganador de la ronda (1 o 2, 0 = empate)
    public UnityEvent<int> OnMatchEnd; // ganador del match (1 o 2)
    public UnityEvent OnRoundStart;

    // Propiedades públicas
    public float CurrentTime => currentTime;
    public float RoundTime => roundTime;
    public int Player1Points => player1Points;
    public int Player2Points => player2Points;
    public bool IsRoundActive => isRoundActive;
    public bool IsMatchOver => isMatchOver;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Inicializar eventos si son null
        OnTimeChanged ??= new UnityEvent<float>();
        OnPointsChanged ??= new UnityEvent<int, int>();
        OnRoundEnd ??= new UnityEvent<int>();
        OnMatchEnd ??= new UnityEvent<int>();
        OnRoundStart ??= new UnityEvent();

        // Suscribirse a los eventos de muerte de los jugadores
        if (player1 != null)
        {
            player1.OnDeath.AddListener(() => OnPlayerDeath(1));
        }
        if (player2 != null)
        {
            player2.OnDeath.AddListener(() => OnPlayerDeath(2));
        }

        // Iniciar la primera ronda
        StartRound();
    }

    private void Update()
    {
        if (!isRoundActive || isMatchOver) return;

        // Actualizar el tiempo
        currentTime -= Time.deltaTime;
        OnTimeChanged?.Invoke(currentTime);

        // Verificar si se acabó el tiempo
        if (currentTime <= 0f)
        {
            currentTime = 0f;
            EndRoundByTimeout();
        }
    }

    /// <summary>
    /// Inicia una nueva ronda
    /// </summary>
    public void StartRound()
    {
        if (isMatchOver) return;

        currentTime = roundTime;
        isRoundActive = true;

        // Teletransportar jugadores a sus spawn points
        if (player1 != null && player1SpawnPoint != null)
        {
            player1.transform.position = player1SpawnPoint.position;
        }
        if (player2 != null && player2SpawnPoint != null)
        {
            player2.transform.position = player2SpawnPoint.position;
        }

        // Resetear la salud de ambos jugadores
        player1?.ResetHealth();
        player2?.ResetHealth();

        // Habilitar el movimiento de los jugadores
        player1?.SetMovementEnabled(true);
        player2?.SetMovementEnabled(true);

        OnRoundStart?.Invoke();
        OnTimeChanged?.Invoke(currentTime);

        Debug.Log("¡Ronda iniciada!");
    }

    /// <summary>
    /// Llamado cuando un jugador muere (salud llega a 0)
    /// </summary>
    private void OnPlayerDeath(int playerNumber)
    {
        if (!isRoundActive) return;

        // El ganador es el otro jugador
        int winner = playerNumber == 1 ? 2 : 1;
        EndRound(winner);
    }

    /// <summary>
    /// Llamado cuando se acaba el tiempo
    /// </summary>
    private void EndRoundByTimeout()
    {
        if (!isRoundActive) return;

        // Comparar salud de ambos jugadores
        float health1 = player1 != null ? player1.CurrentHealth : 0;
        float health2 = player2 != null ? player2.CurrentHealth : 0;

        int winner;
        if (health1 > health2)
        {
            winner = 1;
        }
        else if (health2 > health1)
        {
            winner = 2;
        }
        else
        {
            // Empate - ninguno gana punto
            winner = 0;
        }

        EndRound(winner);
    }

    /// <summary>
    /// Termina la ronda actual y asigna punto al ganador
    /// </summary>
    private void EndRound(int winner)
    {
        isRoundActive = false;

        // Deshabilitar movimiento de los jugadores
        player1?.SetMovementEnabled(false);
        player2?.SetMovementEnabled(false);

        // Asignar punto al ganador
        if (winner == 1)
        {
            player1Points++;
            Debug.Log($"¡Jugador 1 gana la ronda! Puntos: {player1Points}");
        }
        else if (winner == 2)
        {
            player2Points++;
            Debug.Log($"¡Jugador 2 gana la ronda! Puntos: {player2Points}");
        }
        else
        {
            Debug.Log("¡Empate! Nadie gana punto.");
        }

        OnPointsChanged?.Invoke(player1Points, player2Points);
        OnRoundEnd?.Invoke(winner);

        // Verificar si alguien ganó el match
        CheckMatchWinner();
    }

    /// <summary>
    /// Verifica si algún jugador ha ganado el match completo
    /// </summary>
    private void CheckMatchWinner()
    {
        if (player1Points >= pointsToWin)
        {
            EndMatch(1);
        }
        else if (player2Points >= pointsToWin)
        {
            EndMatch(2);
        }
        else
        {
            // Continuar con la siguiente ronda después de un delay
            Invoke(nameof(StartRound), 3f);
        }
    }

    /// <summary>
    /// Termina el match completo
    /// </summary>
    private void EndMatch(int winner)
    {
        isMatchOver = true;
        OnMatchEnd?.Invoke(winner);
        Debug.Log($"¡JUGADOR {winner} GANA EL MATCH! ({player1Points} - {player2Points})");
    }

    /// <summary>
    /// Reinicia el match completo
    /// </summary>
    public void RestartMatch()
    {
        player1Points = 0;
        player2Points = 0;
        isMatchOver = false;
        
        OnPointsChanged?.Invoke(player1Points, player2Points);
        StartRound();
    }

    /// <summary>
    /// Pausa o reanuda la partida
    /// </summary>
    public void SetPaused(bool paused)
    {
        Time.timeScale = paused ? 0f : 1f;
    }

    /// <summary>
    /// Obtiene el tiempo formateado como MM:SS
    /// </summary>
    public string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);
        return $"{minutes:00}:{seconds:00}";
    }

    private void OnDestroy()
    {
        // Desuscribirse de eventos
        if (player1 != null)
        {
            player1.OnDeath.RemoveAllListeners();
        }
        if (player2 != null)
        {
            player2.OnDeath.RemoveAllListeners();
        }
    }
}
