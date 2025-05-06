using UnityEngine;

public class PlayerHitEffect : MonoBehaviour
{
    public GameObject effect;

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"{gameObject.name} colision con {collision.gameObject.name}");
        GameObject effectObj = Instantiate(effect, collision.contacts[0].point, Quaternion.identity);
        
        Destroy(effectObj, 1f);
    }
}
