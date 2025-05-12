using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovimientoCubo : MonoBehaviour
{
    public Rigidbody rb;
    public float rapidez = 5f;

    private void Awake()
    {
        GameManager.Instance.onGameStateChanged += GameManager_OnGameStateChanged;
    }

    private void OnDestroy()
    {
        GameManager.Instance.onGameStateChanged -= GameManager_OnGameStateChanged;
    }

    private void GameManager_OnGameStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.Waiting:
                enabled = false;
                break;
            case GameState.Playing:
                enabled = true;
                break;
            case GameState.Paused:
                enabled = false;
                break;
            case GameState.GameOver:
                enabled = false;
                break;

        }
    }

    // Variable depende del CPU
    void Update()
    {
        
    }

    // Fijo 50 veces por segundo, para acciones de fisica del motor
    void FixedUpdate()
    {
        Vector3 velocidad = new Vector3(0, 0, -rapidez);
        rb.linearVelocity = velocidad;
    }
}
