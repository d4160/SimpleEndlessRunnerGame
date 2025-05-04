using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Rigidbody rb;

    void Start()
    {
        
    }

    void FixedUpdate()
    {
        float movimientoHorizontal = Input.GetAxis("Horizontal") * 5;
        rb.linearVelocity = new Vector3 (movimientoHorizontal, rb.linearVelocity.y, rb.linearVelocity.z);
    }
}
