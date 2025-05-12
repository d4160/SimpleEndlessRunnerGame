using UnityEngine;

public enum GameState
{
    Waiting,
    Playing,
    Paused,
    GameOver
}

[DefaultExecutionOrder(-5)]
public class GameManager : MonoBehaviour
{
    public GameState currentState;

    // Como crear tu propio OnTrigger
    public delegate void GameStateDelegate(GameState state); 
    public event GameStateDelegate onGameStateChanged;

    // Singleton
    public static GameManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        { 
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        currentState = GameState.Waiting;
        InvokeEvent();
    }

    public void Play()
    {
        currentState = GameState.Playing;
        InvokeEvent();
    }

    public void Pause()
    {
        currentState = GameState.Paused;
        InvokeEvent();
    }

    private void InvokeEvent()
    {
        onGameStateChanged?.Invoke(currentState);
    }
}
