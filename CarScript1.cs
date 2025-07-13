using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarScript : MonoBehaviour
{
    private WheelJoint2D[] wheelJoints;
    private JointMotor2D Fwheel;
    private JointMotor2D Bwheel;

    private bool grounded;
    private float deceleration = -400f;
    private float gravity = 9.81f;
    private float angleCar = 0;
    private float acceleration = 500f;
    private float maxSpeed = -800f;
    private float maxBackSpeed = 600f;
    private float brakeForce = 1000f;

    [Header("Wheel Setup")]
    public float wheelSize = 0.2f;               // Ground check radius
    public LayerMask ground;                    // Layer for ground detection
    public Transform bWheel;                    // Assign rear wheel Transform in Inspector

    void Start()
    {
        // Get both WheelJoint2D components attached to this GameObject
        wheelJoints = GetComponents<WheelJoint2D>();

        if (wheelJoints.Length < 2)
        {
            Debug.LogError("❌ Not enough WheelJoint2D components on the car. Need 2 (Front and Back).");
            return;
        }

        // Get initial motor settings from joints
        Fwheel = wheelJoints[0].motor;
        Bwheel = wheelJoints[1].motor;

        // Check for assigned rear wheel
        if (bWheel == null)
        {
            Debug.LogError("❌ Rear wheel Transform (bWheel) is not assigned in the Inspector.");
        }
    }

    void FixedUpdate()
    {
        // Don't run if wheel references are missing
        if (wheelJoints.Length < 2 || bWheel == null) return;

        // Ground check using OverlapCircle
        grounded = Physics2D.OverlapCircle(bWheel.position, wheelSize, ground);

        // Calculate current Z rotation angle
        angleCar = transform.localEulerAngles.z;
        if (angleCar > 180f) angleCar -= 360f;

        if (grounded)
        {
            float slopeEffect = gravity * Mathf.PI * (angleCar / 180f) * 80f;

            // Accelerate forward
            if (Input.GetKey(KeyCode.D))
            {
                Bwheel.motorSpeed = Mathf.Clamp(
                    Bwheel.motorSpeed - (acceleration - slopeEffect) * Time.deltaTime,
                    maxSpeed,
                    maxBackSpeed
                );
            }

            // Decelerate when releasing D
            if (Input.GetKey(KeyCode.A))
            {
                float decel = Bwheel.motorSpeed < 0 ? deceleration : -deceleration;

                Bwheel.motorSpeed = Mathf.Clamp(
                    Bwheel.motorSpeed - (decel - slopeEffect) * Time.deltaTime,
                    maxSpeed,
                    maxBackSpeed
                );
            }

            // Apply braking with Spacebar
            if (Input.GetKey(KeyCode.Space))
            {
                Bwheel.motorSpeed = Mathf.MoveTowards(Bwheel.motorSpeed, 0, brakeForce * Time.deltaTime);
                Fwheel.motorSpeed = Mathf.MoveTowards(Fwheel.motorSpeed, 0, brakeForce * Time.deltaTime);
            }
            else
            {
                // Sync front wheel to rear wheel speed if not braking
                Fwheel = Bwheel;
            }
        }

        // Reassign modified motor values to joints
        wheelJoints[0].motor = Fwheel;
        wheelJoints[1].motor = Bwheel;
    }
}