using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovimientoObjeto : MonoBehaviour
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

    void FixedUpdate()
    {
        Vector3 velocidad = new Vector3(0, 0, -rapidez);
        rb.linearVelocity = velocidad;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.name == "Destructor")
        {
            Destroy(gameObject);
        }
    }

}
