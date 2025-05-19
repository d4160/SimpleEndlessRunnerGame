using System.Collections;
using UnityEngine;
using TMPro; 

public class PlayerController : MonoBehaviour
{
    public Rigidbody rb;


    public TextMeshProUGUI txtPuntos;
    public float score;

    bool finJuego = false;
    public GameObject panelFinJuego;

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

    void Start()
    {
        txtPuntos.text = "0";
    }

    void Update()
    {
        if (_isActive == false) return;

        score += Time.deltaTime;
        txtPuntos.text = Mathf.RoundToInt(score).ToString();
         
        if(score >= PlayerPrefs.GetInt("score") && !finJuego)
        {
            panelFinJuego.SetActive(true);
            TextMeshProUGUI txtMensaje = panelFinJuego.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            txtMensaje.text += PlayerPrefs.GetInt("level");
            finJuego = true;
        }
    }

    void FixedUpdate()
    {
        if (_isActive == false) return;

        float movimientoHorizontal = Input.GetAxis("Horizontal") * 5;
        rb.linearVelocity = new Vector3 (movimientoHorizontal, rb.linearVelocity.y, rb.linearVelocity.z);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.collider.name.Contains("Obstaculo"))
        {
            DisminuirScore(PlayerPrefs.GetInt("penalizacion"));
            GameObject.Find("Main Camera").GetComponent<Camera>().backgroundColor = Color.red;
            Destroy(other.gameObject);
            StartCoroutine(CambiarColorCamara());
        }
        else if (other.collider.name.Contains("Premio"))
        {
            AumentarScore(5);
            Destroy(other.gameObject);
        }
    }

    public IEnumerator CambiarColorCamara()
    {
        yield return new WaitForSeconds(0.5f);
        Color colorRGB = new Color(0.7888221f, 0.8867924f, 0.7813812f);
        GameObject.Find("Main Camera").GetComponent<Camera>().backgroundColor = colorRGB;
    }

    public void AumentarScore(float aumento)
    {
        score += aumento;
    }

    public void DisminuirScore(float penalizacion)
    {
        score -= penalizacion;
    }
}
