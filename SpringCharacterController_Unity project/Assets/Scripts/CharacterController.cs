using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    [Header("References")]
    public Renderer playerRenderer;

    [Header("Gravity Force")]
    public float gravityForce = 9.8f;
    public float floorDistanceWhereGravityStartsToSlowDown = 1.0f;
    public AnimationCurve gravityCurve;

    [Header("Ride height spring")]
    public float floorDistanceWhereSpringForceIsMaximal = 0.02f;
    public float maxSpringForce = 100;
    public AnimationCurve springCurve;

    [Header("Character controller settings")]
    public float jumpForce = 10f;
    public float moveSpeed = 5f;
    public float targetRideHeight = 0.1f;
    [Space]
    public float groundCheckDistance = 2.0f;

    private Rigidbody playerRb;
    private CapsuleCollider playerCollider;

    // Start is called before the first frame update
    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        playerCollider = GetComponent<CapsuleCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            playerRb.AddForce(jumpForce * Vector3.up, ForceMode.Impulse);
        }
    }

    private void FixedUpdate()
    {
        if (GetGroundInfo(out float distanceToTheGround))
        {
            // Gravity
            float gravityFactor = 1.0f; // gravity factor goes from 0 to 1 when character is above rideHeight
            if (distanceToTheGround <= targetRideHeight)
            {
                gravityFactor = 0;
            }
            else if (distanceToTheGround <= floorDistanceWhereGravityStartsToSlowDown)
            {
                gravityFactor = (distanceToTheGround - targetRideHeight) / (floorDistanceWhereGravityStartsToSlowDown - targetRideHeight);
            }
            gravityFactor = Mathf.Clamp(gravityFactor, 0, 1);
            gravityFactor = gravityCurve.Evaluate(gravityFactor);
            ApplyGravityWithFactor(gravityFactor);

            // "Spring" to keep character above the ground
            float springFactor = 0.0f; // spring factor is 1 when character touches the ground, and is zero when character is exactly at rideHeight
            if (distanceToTheGround <= floorDistanceWhereSpringForceIsMaximal)
            {
                springFactor = 1.0f;
            }
            else if (distanceToTheGround < targetRideHeight)
            {
                springFactor = (targetRideHeight - distanceToTheGround) / (targetRideHeight - floorDistanceWhereSpringForceIsMaximal);
            }
            springFactor = Mathf.Clamp(springFactor, 0, 1);
            springFactor = springCurve.Evaluate(springFactor);
            ApplySpringForceWithFactor(springFactor);
        }
        else
        {
            // No ground nowhere, standard gravity
            ApplyGravityWithFactor(1.0f);

        }

        // Horizontal movement
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        playerRb.AddForce(moveSpeed * horizontalInput * Vector3.right);
        playerRb.AddForce(moveSpeed * verticalInput * Vector3.forward);
        playerRenderer.transform.localRotation = Quaternion.Euler(20*verticalInput, 0, -20*horizontalInput);
    }

    private void LateUpdate()
    {
        if (GetGroundInfo(out float distanceToGround))
        {
            float distanceToGroundFactor = distanceToGround / 3.0f;
            float yScale = 1 + (distanceToGroundFactor - targetRideHeight);
            yScale = Mathf.Clamp(yScale, 0.6f, 1.4f);
            float horizontalScale = 1 - (yScale - 1);
            playerRenderer.transform.localScale = new Vector3(horizontalScale, yScale, horizontalScale);
        }
        else
        {
            playerRenderer.transform.localScale = Vector3.one;
        }
    }

    private bool GetGroundInfo(out float distance)
    {
        bool isGround = false;
        distance = 0;
        float distanceToTheGround = -1; // no ground
        float sphereRadius = playerCollider.radius;
        if (Physics.SphereCast(this.transform.position, sphereRadius, -groundCheckDistance * Vector3.up, out RaycastHit hit))
        {
            // found the ground!
            distance = hit.distance - (playerCollider.height / 2.0f);
            DebugMessageWithTimestamp("Raycast hit with distance to the ground = " + distanceToTheGround);
            isGround = true;
        }
        return isGround;
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
