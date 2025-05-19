using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneradorPremios : MonoBehaviour
{
    public GameObject premioPrefab;
    public Transform[] posiciones;

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
                CancelInvoke("GenerarPremios");
                break;
            case GameState.Playing:
                InvokeRepeating("GenerarPremios", 0f, 3f);
                break;
            case GameState.Paused:
                CancelInvoke("GenerarPremios");
                break;
            case GameState.GameOver:
                CancelInvoke("GenerarPremios");
                break;

        }
    }

    public void GenerarPremios()
    {
        int randomIndex = Random.Range(0, posiciones.Length);
        Instantiate(premioPrefab, posiciones[randomIndex].position + Vector3.up * 0.5f, premioPrefab.transform.rotation);
    }
}
