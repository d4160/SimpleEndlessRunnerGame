using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerarCubos : MonoBehaviour
{
    public GameObject cuboPrefab;
    public Transform posicionGeneradora;
    public Transform[] otrasPosiciones;

    private bool _isActive = false;

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

    private void OnTriggerEnter(Collider other)
    {
        if (_isActive == false) return;

        if (other.name == "Jugador")
        {
            if (PlayerPrefs.GetInt("level") == 1)
            {
                Instantiate(cuboPrefab, posicionGeneradora.position, Quaternion.identity);
            }
            else if (PlayerPrefs.GetInt("level") == 2)
            {
                int randomIndex = Random.Range(0, otrasPosiciones.Length);
                Instantiate(cuboPrefab, posicionGeneradora.position, Quaternion.identity);
                Instantiate(cuboPrefab, otrasPosiciones[randomIndex].position, Quaternion.identity);
            }
        }
    }
}

