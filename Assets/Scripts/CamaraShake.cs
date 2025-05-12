using TMPro.EditorUtilities;
using UnityEngine;

public class CamaraShake : MonoBehaviour
{
    public Camera cam;

    public float duration;
    public float shakeRadius;
    public float lerpSpeed = 2.5f;

    private static float _timer;
    private static bool _isActive;
    private Vector3 _startPosition;

    public static void StartEffect()
    {
        _isActive = true;
        _timer = 0;
    }

    private void Start()
    {
        _startPosition = cam.transform.position;
    }

    // Start(), Awake(), Update(), OnTrigger

    void Update()
    {
        if (_isActive)
        {
            cam.transform.position = cam.transform.position + Random.insideUnitSphere * shakeRadius;

            _timer += Time.deltaTime;

            if (_timer > duration)
            {
                _isActive = false;
                // Desactivr
                StopEffect();
            }
        }
        else
        {
            // A (posicion final del shake) -> B (posicion inicial)
            cam.transform.position = Vector3.Lerp(cam.transform.position,
                _startPosition, Time.deltaTime * lerpSpeed);
        }
    }

    public void StopEffect()
    { 
        //cam.transform.position = _startPosition;
    }
}
