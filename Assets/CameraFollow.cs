using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 5, -8);
    public float smoothTime = 0.25f;
    public float lookAhead = 1.5f;
    Vector3 velocity;

    Rigidbody targetRb;

    void Awake()
    {
        if (target == null)
            target = GameObject.FindWithTag("Player").transform;
        targetRb = target.GetComponent<Rigidbody>();
    }
    void LateUpdate()
    {
        Rigidbody rb = target.GetComponent<Rigidbody>();
        Vector3 flat = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        Vector3 goal = target.position + offset + flat.normalized * lookAhead;
        transform.position = Vector3.SmoothDamp(transform.position, goal, ref velocity, smoothTime);
        transform.LookAt(target.position);
    }
}