using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Animator))]
[RequireComponent (typeof(Rigidbody2D))]


public class movimento : MonoBehaviour
{
    void Animate()
    {
        animator.SetFloat (xHash, moveInput.x);
        animator.SetFloat (yHash, moveInput.y);

    }
    private static readonly int xHash = Animator.StringToHash("MoveX");
    private static readonly int yHash = Animator.StringToHash("MoveY");
    private Animator animator;
    private Rigidbody2D rb;
    private Vector2 moveInput;

    [SerializeField] private float movementSpeed = 7.5f;

    private void Awake()
    {
        animator = GetComponent<Animator>(); 
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        Animate();
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }

    private void FixedUpdate()
    {
        rb.velocity = moveInput.normalized * Time.fixedDeltaTime * movementSpeed;
    }
}
