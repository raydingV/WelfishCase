using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    private Rigidbody2D rb;
    
    [Header("Sounds")] 
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip JumpSFX;
    [SerializeField] private AudioClip LandSFX;
    
    [Header("Movement")]
    private Vector2 moveInput;
    private Vector2 direction = Vector2.right;
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float jumpImpulse = 5f;
    [SerializeField] private float _gravityScale = 5f;
    [SerializeField] private float _gravityJumpScale = 3f;
    private const float DEADZONE = 0.2f;
    private int jumpCount = 0;
    private float jumpTimer = 0.3f;

    [Header("MovementVisual")] [SerializeField]
    private Quaternion walkRotation = new Quaternion(0, 0, -0.7f, 0);
    private Quaternion normalRotation;
    private Quaternion targetRotation;

    [Header("JumpVisual")] 
    [SerializeField] private GameObject spriteRenderer;
    [SerializeField] private Vector2 jumpScale = new Vector2(0.9f, 1.1f);
    [SerializeField] private Vector2 landScale = new Vector2(0.9f, 1.1f);
    [SerializeField] private Vector2 landPos = new Vector2(0, 0);
    private Vector2 normalScale;
    private Vector2 targetScale;
    private Vector2 normalPosition;
    private Vector2 targetPosition;
    
    [Header("Raycast")]
    public Transform groundCheck;
    [SerializeField] private Transform wallCheck;
    public float CheckDistance = 0.1f;
    public LayerMask groundLayer;
    
    [SerializeField] private bool onGround = false;
    [SerializeField] private bool hitWall = false;
    private bool landed = false;

    private float MoveSpeed
    {
        get
        {
            return walkSpeed;
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        normalScale = spriteRenderer.transform.localScale;
        targetScale = normalScale;
        normalPosition = spriteRenderer.transform.localPosition;
        targetPosition = normalPosition;
        normalRotation = spriteRenderer.transform.localRotation;
        targetRotation = normalRotation;
    }
    
    void Update()
    {
        //It's for Dynamic Gravity, gravity increases and becomes stronger as the player falls
        if (rb.velocity.y <= 0 && rb.velocity.y >= -0.2f)
        {
            rb.gravityScale = _gravityScale;
        }
        if (rb.velocity.y < 0)
        {
            rb.gravityScale += Time.deltaTime * 10f;
        }
        
        if (rb.velocity.x != 0)
        {
            targetRotation = walkRotation;
        }
        else
        {
            targetRotation = normalRotation;
        }

        //For end of Squash
        if (Vector2.Distance(spriteRenderer.transform.localScale, targetScale) <= 0.05f)
        {
            targetScale = normalScale;
        }
        
        //End of Land Squash
        if (targetScale == normalScale && onGround)
        {
            targetPosition = normalPosition;
        }

        RayCheck();
        
        //Landing settings
        if (onGround && !landed)
        {
            landed = true;
            _audioSource.PlayOneShot(LandSFX);
            targetScale = landScale;
            landPos = new Vector2(0f, -(1 - landScale.y));
            
            targetPosition = landPos;

            jumpCount = 0;
            jumpTimer = 0.3f;
        }
        
        //First Jump or Fall Sequence
        if (onGround == false)
        {
            landed = false;
            jumpTimer -= Time.deltaTime; //Coyote Time
        }
    }

    private void FixedUpdate()
    {
        if (!hitWall)
        {
            Movement(MoveSpeed);
        }
        
        Squash();
    }

    void Movement(float speed)
    {
        rb.velocity = new Vector2(moveInput.x * speed, rb.velocity.y);
    }
    
    public void onMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();

        moveInput = new Vector2(Mathf.Abs(input.x) < DEADZONE ? 0f : input.x, 0f);

        if (Mathf.Abs(moveInput.x) > 0)
        {
            transform.localScale = new Vector2(Mathf.Sign(moveInput.x), transform.localScale.y); //Player direction
        }
        
        direction = moveInput.x > 0 ? Vector2.right : Vector2.left; // RayCast direction for wallCheck

    }
    
    public void Jump(InputAction.CallbackContext context)
    {
        if (context.started && jumpCount < 1 && jumpTimer >= 0)
        {
            rb.velocity = new Vector2(rb.velocityX, jumpImpulse);
            targetScale = jumpScale;
            rb.gravityScale = _gravityJumpScale;
            jumpCount = 1;
            _audioSource.PlayOneShot(JumpSFX);
        }
    }
    
    private void Squash()
    {
        //A smooth transition is achieved with lerp to create animation.
        if (spriteRenderer != null)
        {
            spriteRenderer.transform.localScale = Vector2.Lerp(spriteRenderer.transform.localScale, targetScale, Time.deltaTime * 10f);

            if (onGround)
            {
                spriteRenderer.transform.localPosition = Vector2.Lerp(spriteRenderer.transform.localPosition, targetPosition, Time.deltaTime * 10f);
            }
            
            spriteRenderer.transform.localRotation = Quaternion.Lerp(spriteRenderer.transform.localRotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    private void RayCheck()
    {
        onGround = Physics2D.Raycast(groundCheck.position, Vector2.down, 0.1f, groundLayer);
        hitWall = Physics2D.BoxCast(wallCheck.position,new Vector2(0.1f, 1f),0f, direction, 0.5f, groundLayer);
    }
}
