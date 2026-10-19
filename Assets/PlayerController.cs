using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Move")]
    public float startSpeed = 6f;       // speed at the beginning
    public float maxSpeed = 14f;        // speed never goes above this
    public float acceleration = 0.2f;   // speed added every second
    public bool autoRun = false;   // tick this in runner levels
    public Transform cam;               // found automatically (Part C2)

    [Header("Jump")]
    public float jumpHeight = 2f;
    public float coyoteTime = 0.12f;    // B1
    public float jumpBufferTime = 0.12f;// B1
    public float fallMultiplier = 2.5f; // B2

    [Header("Juice (B3)")]
    public Transform visual;            // the part that gets squashed (optional)
    public float squashAmount = 0.7f;   // 0.7 = 70% of normal height
    public float squashTime = 0.15f;    // seconds

    Rigidbody rb;
    Vector2 input;
    bool grounded;
    bool touchedGround;
    bool wasGrounded;
    float coyoteTimer, bufferTimer;
    float currentSpeed;
    float prevVy;
    Vector3 baseScale;
    Coroutine squashRoutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.sleepThreshold = 0f;
        currentSpeed = startSpeed;

        if (visual == null) visual = transform;
        baseScale = visual.localScale;

        if (cam == null)
        {
            if (Camera.main != null) cam = Camera.main.transform;
            else Debug.LogError("No camera with the MainCamera tag!");
        }
    }

    void Update()
    {
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (autoRun) input.y = 1f;     // always run forward
        input = Vector2.ClampMagnitude(input, 1f);

        if (Input.GetButtonDown("Jump")) bufferTimer = jumpBufferTime;
        else bufferTimer -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        // Speed grows with time
        currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, acceleration * Time.fixedDeltaTime);

        // Ground check (result comes from the collision methods below)
        grounded = touchedGround;
        touchedGround = false;

        // Landing squash (B3)
        if (grounded && !wasGrounded && prevVy < -4f)
        {
            if (squashRoutine != null) StopCoroutine(squashRoutine);
            squashRoutine = StartCoroutine(Squash());
        }
        wasGrounded = grounded;

        coyoteTimer = grounded ? coyoteTime : coyoteTimer - Time.fixedDeltaTime;

        // Move relative to camera (C2)
        Vector3 f = cam.forward; f.y = 0; f.Normalize();
        Vector3 r = cam.right; r.y = 0; r.Normalize();
        Vector3 dir = f * input.y + r * input.x;
        Vector3 v = rb.linearVelocity;
        rb.linearVelocity = new Vector3(dir.x * currentSpeed, v.y, dir.z * currentSpeed);

        // Jump (both timers get used up)
        if (bufferTimer > 0f && coyoteTimer > 0f)
        {
            float g = Mathf.Abs(Physics.gravity.y);
            float jumpSpeed = Mathf.Sqrt(2f * g * jumpHeight);
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpSpeed, ForceMode.VelocityChange);
            bufferTimer = 0f; coyoteTimer = 0f;
        }

        // Fall faster than you rise (B2)
        if (rb.linearVelocity.y < 0f)
            rb.AddForce(Physics.gravity * (fallMultiplier - 1f), ForceMode.Acceleration);

        prevVy = rb.linearVelocity.y;   // remember for the landing check
    }

    void OnCollisionEnter(Collision collision) { CheckContacts(collision); }
    void OnCollisionStay(Collision collision) { CheckContacts(collision); }

    void CheckContacts(Collision collision)
    {
        foreach (ContactPoint c in collision.contacts)
        {
            if (c.normal.y > 0.5f) { touchedGround = true; return; }
        }
    }

    // Squash effect with an easing curve (B3)
    IEnumerator Squash()
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / squashTime;
            float k = Mathf.Sin(Mathf.Clamp01(t) * Mathf.PI);  // goes 0 -> 1 -> 0
            float eased = k * k * (3f - 2f * k);                // smoothstep easing
            float y = Mathf.Lerp(1f, squashAmount, eased);
            float xz = 1f + (1f - y) * 0.5f;                    // wider when flat
            visual.localScale = new Vector3(baseScale.x * xz, baseScale.y * y, baseScale.z * xz);
            yield return null;
        }
        visual.localScale = baseScale;
    }

    // Green = on the ground, red = in the air
    void OnDrawGizmos()
    {
        Gizmos.color = grounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position + Vector3.down * 0.5f, 0.2f);
    }
}