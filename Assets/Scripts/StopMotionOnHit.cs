using UnityEngine;

public class StopMotionOnHit : MonoBehaviour
{
    public float duration;
    public float timeScale = 0.5f;

    private bool _isActive;
    private float _timer;

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"{gameObject.name} colision con {collision.gameObject.name}");
        StartStopMotionEffect();
    }

    private void StartStopMotionEffect()
    {
        Time.timeScale = timeScale;
        _timer = 0;
        _isActive = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (_isActive)
        {
            _timer += Time.deltaTime;

            if (_timer > duration)
            {
                _isActive = false;
                Time.timeScale = 1;
            }
        }
    }
}
