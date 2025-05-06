using UnityEngine;

public class CameraShakeOnHit : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"{gameObject.name} colision con {collision.gameObject.name}");
        CamaraShake.StartEffect();
    }
}
