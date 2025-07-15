using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Platform : MonoBehaviour
{
    private Rigidbody2D rb;
    
    public float speed = 5f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        rb.velocity = new Vector2(0, -speed);
    }

    private void Update()
    {
        if (transform.position.y <= -12f)
        {
             Destroy(gameObject);
        }
    }
}
