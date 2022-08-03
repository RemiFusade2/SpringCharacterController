using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    private Rigidbody playerRb;
    private CapsuleCollider playerCollider;

    public float gravityForce = 9.8f;
    public float jumpForce = 10f;

    [Space]
    public float moveSpeed = 5;

    [Space]
    public float groundCheckDistance = 2.0f;
    public float rideHeight = 0.7f;
    public float maxSpringForce = 100;

    public AnimationCurve curve;

    // Start is called before the first frame update
    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        playerCollider = GetComponent<CapsuleCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            playerRb.AddForce(jumpForce * Vector3.up, ForceMode.Impulse);
        }
    }

    private void FixedUpdate()
    {
        // Distance to the ground
        float distanceToTheGround = GetDistanceFromTheGround();
        if (distanceToTheGround < -0.99f)
        {
            // No ground nowhere, standard gravity
            ApplyGravityWithFactor(1.0f);
        }
        else
        {
            // Gravity
            float gravityFactor = 1.0f; // gravity factor goes from 0 to 1 when character is above rideHeight
            if (distanceToTheGround <= rideHeight)
            {
                gravityFactor = 0;
            }
            else if (distanceToTheGround <= 2 * rideHeight)
            {
                gravityFactor = (distanceToTheGround - rideHeight) / rideHeight;
            }
            gravityFactor = Mathf.Clamp(gravityFactor, 0, 1);
            gravityFactor = curve.Evaluate(gravityFactor);
            ApplyGravityWithFactor(gravityFactor);

            // "Spring" to keep character above the ground
            float springFactor = 0.0f; // spring factor is 1 when character touches the ground, and is zero when character is exactly at rideHeight
            if (distanceToTheGround < rideHeight)
            {
                springFactor = (rideHeight - distanceToTheGround) / rideHeight;
            }
            springFactor = Mathf.Clamp(springFactor, 0, 1);
            springFactor = curve.Evaluate(springFactor);
            ApplySpringForceWithFactor(springFactor);
        }

        // Horizontal movement
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");        
        playerRb.AddForce(moveSpeed * horizontalInput * Vector3.right);
        playerRb.AddForce(moveSpeed * verticalInput * Vector3.forward);
    }
    
    private float GetDistanceFromTheGround()
    {
        float distanceToTheGround = -1; // no ground
        if (Physics.SphereCast(this.transform.position, 0.1f, -groundCheckDistance * Vector3.up, out RaycastHit hit))
        {
            // found the ground!
            distanceToTheGround = hit.distance - (playerCollider.height / 2.0f);
            DebugMessageWithTimestamp("Raycast hit with distance to the ground = " + distanceToTheGround);
        }
        return distanceToTheGround;
    }

    private void ApplyGravityWithFactor(float gravityFactor)
    {
        float gForce = gravityFactor * gravityForce;
        DebugMessageWithTimestamp("gravityForce = " + gravityForce * gravityFactor);
        playerRb.AddForce(-gForce * Vector3.up, ForceMode.Force);
    }

    private void ApplySpringForceWithFactor(float springFactor)
    {
        float springForce = springFactor * maxSpringForce;
        DebugMessageWithTimestamp("springForce = " + springForce);
        playerRb.AddForce(springForce * Vector3.up);
    }

    private void DebugMessageWithTimestamp(string message)
    {
        //Debug.Log(Time.time.ToString("0.00") + ": " + message);
    }
}
