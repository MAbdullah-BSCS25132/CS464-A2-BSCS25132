using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 5, -8);
    public float smoothTime = 0.25f;
    public float lookAhead = 1.5f;

    [Header("Turning tracks")]
    public bool followHeading = false;   // tick this in ball levels with turns
    public float headingSpeed = 2f;      // how fast the camera swings behind the ball

    Rigidbody targetRb;
    Vector3 velocity;
    Vector3 currentAhead;
    float heading;                       // camera turn angle in degrees

    void Awake()
    {
        if (target == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) target = p.transform;
            else { Debug.LogError("No object with the Player tag!"); enabled = false; return; }
        }
        targetRb = target.GetComponent<Rigidbody>();
    }

    void LateUpdate()
    {
        Vector3 flat = new Vector3(targetRb.linearVelocity.x, 0, targetRb.linearVelocity.z);

        // Turn the camera toward the direction the ball is moving
        if (followHeading && flat.magnitude > 1f)
        {
            float wanted = Mathf.Atan2(flat.x, flat.z) * Mathf.Rad2Deg;
            heading = Mathf.LerpAngle(heading, wanted, 1f - Mathf.Exp(-headingSpeed * Time.deltaTime));
        }
        Quaternion rot = followHeading ? Quaternion.Euler(0, heading, 0) : Quaternion.identity;

        // Look-ahead (smooth, no sudden jump)
        Vector3 aheadGoal = Vector3.ClampMagnitude(flat, 1f) * lookAhead;
        currentAhead = Vector3.Lerp(currentAhead, aheadGoal, 3f * Time.deltaTime);

        Vector3 goal = target.position + rot * offset + currentAhead;
        transform.position = Vector3.SmoothDamp(transform.position, goal, ref velocity, smoothTime);
        transform.LookAt(target.position);
    }
}