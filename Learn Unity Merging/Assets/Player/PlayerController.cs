using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private InputActionReference movement;
    [SerializeField] private InputActionReference jump;

    [Header("Basic movement")]
    [SerializeField] [Min(0)] private float moveSpeed = 1;
    [SerializeField] [Range(0, 1)] private float groundControl = 0.5f;
    [SerializeField] [Range(0, 1)] private float airControl = 0.5f;

    [Header("Jumping")]
    [SerializeField] private float jumpPower = 5f;
    [SerializeField] [Range(0, 90)] private float maxGroundAngle = 20; //How steep is too steep to climb/jump?
    [SerializeField] private float coyoteTime = 0.2f; //Give a small window for player to jump even after being off the ground
    private float lastGroundTime;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        Vector3 finalVelocity = rb.velocity;

        //Calculate frame of reference
        Vector3 right = Camera.main.transform.right;
        right.y = 0; right.Normalize(); //Flatten
        Vector3 forward = new Vector3(-right.z, 0, right.x); //Rotate 90 deg CCW on Y axis

        bool isGrounded = (lastGroundTime + coyoteTime > Time.time);

        //Handle WASD input
        Vector2 rawMove = movement.action.ReadValue<Vector2>();
        Vector3 targetMove3D = right*rawMove.x + forward*rawMove.y;

        //Apply WASD input on X/Z axis. Don't touch Y (so we can jump)
        //Lerp has a nice smoothing effect, causing us to always approach the target
        float moveControl = isGrounded ? groundControl : airControl;
        finalVelocity.x = Mathf.Lerp(finalVelocity.x, moveSpeed*targetMove3D.x, moveControl);
        finalVelocity.z = Mathf.Lerp(finalVelocity.z, moveSpeed*targetMove3D.z, moveControl);

        //Handle jumping
        if (isGrounded && jump.action.IsPressed())
        {
            lastGroundTime = -coyoteTime; //Mark that we're no longer on the ground
            finalVelocity += Vector3.up * jumpPower; //Apply jump velocity
        }

        rb.velocity = finalVelocity;
    }

    private void OnCollisionStay(Collision collision)
    {
        //If it's not too steep, mark that we're touching the ground
        float angle = Vector3.Angle(collision.contacts[0].normal, Vector3.up);
        if (angle < maxGroundAngle)
        {
            lastGroundTime = Time.fixedTime;
        }
    }
}
