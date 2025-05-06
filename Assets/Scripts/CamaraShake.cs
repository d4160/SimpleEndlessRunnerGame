using TMPro.EditorUtilities;
using UnityEngine;

public class CamaraShake : MonoBehaviour
{
    public Camera cam;

    public float duration;
    public float shakeRadius;

    private static float _timer;
    private static bool _isActive;

    public static void StartEffect()
    {
        _isActive = true;
        _timer = 0;
    }

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
    }

    public void StopEffect()
    { 

    }
}
