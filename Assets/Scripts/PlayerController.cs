using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Rigidbody rb;

    private bool _isActive;

    private void OnEnable()
    {
        GameManager.Instance.onGameStateChanged += GameManager_OnGameStateChanged;
    }

    private void OnDisable()
    {
        GameManager.Instance.onGameStateChanged -= GameManager_OnGameStateChanged;
    }

    private void GameManager_OnGameStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.Waiting:
                _isActive = false;
                break;
            case GameState.Playing:
                _isActive = true;
                break;
            case GameState.Paused:
                _isActive = false;
                break;
            case GameState.GameOver:
                _isActive = false;
                break;

        }
    }

    void FixedUpdate()
    {
        if (_isActive == false) return;
        float movimientoHorizontal = Input.GetAxis("Horizontal") * 5;
        rb.linearVelocity = new Vector3 (movimientoHorizontal, rb.linearVelocity.y, rb.linearVelocity.z);
    }
}
