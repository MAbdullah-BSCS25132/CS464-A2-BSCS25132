using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class BallController : MonoBehaviour
{
    public float maxSpeed = 8f;
    public float acceleration = 35f;      // higher = quicker turning
    public float jumpHeight = 2f;
    public float coyoteTime = 0.12f;      // B1
    public float jumpBufferTime = 0.12f;  // B1
    public float fallMultiplier = 2.5f;   // B2
    public float popAmount = 0.6f;        // B3: shrinks to 60% on landing
    public float popTime = 0.2f;

    Rigidbody rb;
    Transform cam;
    Vector2 input;
    bool grounded, touched;
    float coyoteTimer, bufferTimer;
    Vector3 baseScale;
    Coroutine popRoutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cam = Camera.main.transform;
        baseScale = transform.localScale;
    }

    void Update()
    {
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        input = Vector2.ClampMagnitude(input, 1f);
        if (Input.GetButtonDown("Jump")) bufferTimer = jumpBufferTime;
        else bufferTimer -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        grounded = touched;
        touched = false;
        coyoteTimer = grounded ? coyoteTime : coyoteTimer - Time.fixedDeltaTime;

        // move relative to the camera, with quick turning
        Vector3 f = cam.forward; f.y = 0; f.Normalize();
        Vector3 r = cam.right; r.y = 0; r.Normalize();
        Vector3 wanted = (f * input.y + r * input.x) * maxSpeed;

        Vector3 v = rb.linearVelocity;
        Vector3 flat = new Vector3(v.x, 0f, v.z);
        flat = Vector3.MoveTowards(flat, wanted, acceleration * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector3(flat.x, v.y, flat.z);

        // jump (both timers used up)
        if (bufferTimer > 0f && coyoteTimer > 0f)
        {
            float jv = Mathf.Sqrt(2f * Mathf.Abs(Physics.gravity.y) * jumpHeight);
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jv, rb.linearVelocity.z);
            bufferTimer = 0f; coyoteTimer = 0f;
        }

        // fall faster than you rise
        if (rb.linearVelocity.y < 0f)
            rb.AddForce(Physics.gravity * (fallMultiplier - 1f), ForceMode.Acceleration);
    }

    void OnCollisionEnter(Collision c)
    {
        foreach (ContactPoint p in c.contacts)
        {
            if (p.normal.y > 0.5f)
            {
                touched = true;
                // hard landing -> squash
                if (Mathf.Abs(c.relativeVelocity.y) > 3f)
                {
                    if (popRoutine != null) StopCoroutine(popRoutine);
                    popRoutine = StartCoroutine(Pop());
                }
                break;
            }
        }
    }

    void OnCollisionStay(Collision c)
    {
        foreach (ContactPoint p in c.contacts)
            if (p.normal.y > 0.5f) touched = true;
    }

    // B3: landing squash with an easing curve
    IEnumerator Pop()
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / popTime;
            float k = Mathf.Sin(Mathf.Clamp01(t) * Mathf.PI);   // 0 -> 1 -> 0
            float eased = k * k * (3f - 2f * k);                 // smoothstep
            transform.localScale = baseScale * Mathf.Lerp(1f, popAmount, eased);
            yield return null;
        }
        transform.localScale = baseScale;
    }
}