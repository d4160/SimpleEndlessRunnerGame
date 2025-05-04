using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovimientoCubo : MonoBehaviour
{
    public Rigidbody rb;
    public float rapidez = 5f; 

    void Start()
    {

    }

    // Variable depende del CPU
    void Update()
    {
        
    }

    // Fijo 50 veces por segundo, para acciones de fisica del motor
    void FixedUpdate()
    {
        Vector3 velocidad = new Vector3(0, 0, -rapidez);
        rb.linearVelocity = velocidad;
    }
}
