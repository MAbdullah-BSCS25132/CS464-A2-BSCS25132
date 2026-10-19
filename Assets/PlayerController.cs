using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Move")]
    public float startSpeed = 6f;       // speed at the beginning
    public float maxSpeed = 14f;        // speed will never go above this
    public float acceleration = 0.2f;   // how much speed is added every second
    public Transform cam;               // found automatically (Part C2)

    [Header("Jump")]
    public float jumpHeight = 2f;
    public float coyoteTime = 0.12f;    // B1
    public float jumpBufferTime = 0.12f;// B1
    public float fallMultiplier = 2.5f; // B2


    Rigidbody rb;
    float currentSpeed;
    Vector2 input;
    bool grounded;
    bool touchedGround;                 // set by collision callbacks
    float coyoteTimer, bufferTimer;

    void Awake()
    {
        currentSpeed = startSpeed;
        rb = GetComponent<Rigidbody>();
        rb.sleepThreshold = 0f;         // never fall asleep, so contacts keep updating

        if (cam == null)
        {
            if (Camera.main != null) cam = Camera.main.transform;
            else Debug.LogError("No camera with the MainCamera tag!");
        }
    }

    void Update()
    {
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        input = Vector2.ClampMagnitude(input, 1f); // diagonals not faster

        if (Input.GetButtonDown("Jump")) bufferTimer = jumpBufferTime;
        else bufferTimer -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, acceleration * Time.fixedDeltaTime);
        // Ground check (result comes from OnCollisionEnter/Stay)
        grounded = touchedGround;
        touchedGround = false;

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
            bufferTimer = 0f; coyoteTimer = 0f;   // used up
        }

        // Fall faster than you rise (B2)
        if (rb.linearVelocity.y < 0f)
            rb.AddForce(Physics.gravity * (fallMultiplier - 1f), ForceMode.Acceleration);
    }

    // A contact whose normal points up means "I am standing on this".
    // Walls have sideways normals, so they don't count.
    void OnCollisionEnter(Collision collision) { CheckContacts(collision); }
    void OnCollisionStay(Collision collision) { CheckContacts(collision); }

    void CheckContacts(Collision collision)
    {
        foreach (ContactPoint c in collision.contacts)
        {
            if (c.normal.y > 0.5f) { touchedGround = true; return; }
        }
    }

    // Green = on the ground, red = in the air
    void OnDrawGizmos()
    {
        Gizmos.color = grounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position + Vector3.down * 0.5f, 0.2f);
    }
}