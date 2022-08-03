using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    [Header("References")]
    public Renderer playerRenderer;
    public Transform playerCamera;

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

    private Vector3 rendererTargetScale;
    private Quaternion rendererTargetRotation;

    private bool grounded;

    // Start is called before the first frame update
    void Start()
    {
        grounded = false;
        playerRb = GetComponent<Rigidbody>();
        playerCollider = GetComponent<CapsuleCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        if (grounded && Input.GetButtonDown("Jump"))
        {
            grounded = false;
            playerRb.AddForce(jumpForce * Vector3.up, ForceMode.Impulse);
        }
    }

    private void FixedUpdate()
    {
        grounded = GetGroundInfo(out float distanceToGround);
        if (grounded)
        {
            // Gravity
            float gravityFactor = 1.0f; // gravity factor goes from 0 to 1 when character is above rideHeight
            if (distanceToGround <= targetRideHeight)
            {
                gravityFactor = 0;
            }
            else if (distanceToGround <= floorDistanceWhereGravityStartsToSlowDown)
            {
                gravityFactor = (distanceToGround - targetRideHeight) / (floorDistanceWhereGravityStartsToSlowDown - targetRideHeight);
            }
            gravityFactor = Mathf.Clamp(gravityFactor, 0, 1);
            gravityFactor = gravityCurve.Evaluate(gravityFactor);
            ApplyGravityWithFactor(gravityFactor);

            // "Spring" to keep character above the ground
            float springFactor = 0.0f; // spring factor is 1 when character touches the ground, and is zero when character is exactly at rideHeight
            if (distanceToGround <= floorDistanceWhereSpringForceIsMaximal)
            {
                springFactor = 1.0f;
            }
            else if (distanceToGround < targetRideHeight)
            {
                springFactor = (targetRideHeight - distanceToGround) / (targetRideHeight - floorDistanceWhereSpringForceIsMaximal);
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
        playerRb.AddForce(moveSpeed * horizontalInput * playerCamera.right);
        playerRb.AddForce(moveSpeed * verticalInput * playerCamera.forward);

        // Set new target renderer rotation and scale
        rendererTargetRotation = Quaternion.Euler(20*verticalInput, playerCamera.localRotation.eulerAngles.y, -20*horizontalInput);
        if (grounded)
        {
            float distanceToGroundFactor = distanceToGround / 3.0f;
            float yScale = 1 + (distanceToGroundFactor - targetRideHeight);
            yScale = Mathf.Clamp(yScale, 0.6f, 1.4f);
            float horizontalScale = 1 - (yScale - 1);
            rendererTargetScale = new Vector3(horizontalScale, yScale, horizontalScale);
        }
        else
        {
            rendererTargetScale = Vector3.one;
        }
    }

    private void LateUpdate()
    {
        // set renderer actual rotation and scale
        playerRenderer.transform.localScale = Vector3.Lerp(playerRenderer.transform.localScale, rendererTargetScale, 0.1f);
        playerRenderer.transform.localRotation = Quaternion.Lerp(playerRenderer.transform.localRotation, rendererTargetRotation, 0.1f);
    }

    private bool GetGroundInfo(out float distance)
    {
        bool isGround = false;
        distance = 0;
        float distanceToTheGround = -1; // no ground
        float sphereRadius = playerCollider.radius * 0.5f;
        if (Physics.SphereCast(this.transform.position, sphereRadius, -groundCheckDistance * Vector3.up, out RaycastHit hit, groundCheckDistance))
        {
            // found the ground!
            distance = hit.distance - (playerCollider.height / 2.0f);
            DebugMessageWithTimestamp("Raycast hit with distance to the ground = " + distanceToTheGround);
            isGround = true;
        }
        Debug.Log("grounded = " + grounded);
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
