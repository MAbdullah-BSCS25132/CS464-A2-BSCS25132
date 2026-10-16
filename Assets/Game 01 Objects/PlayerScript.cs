using UnityEngine;
using UnityEngine.InputSystem;

public class SimplePlayer : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpHeight = 1.5f;
    public float gravity = -20f;

    float verticalVelocity;
    float groundY;

    void Start()
    {
        groundY = transform.position.y;
    }

    void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        // Forward = -Z, right = +X
        float x = 0f, z = 0f;
        if (kb.wKey.isPressed) z = 1f;
        if (kb.sKey.isPressed) z = -1f;
        if (kb.dKey.isPressed) x = 1f;
        if (kb.aKey.isPressed) x = -1f;

        Vector3 pos = transform.position;
        pos += new Vector3(x, 0f, z).normalized * moveSpeed * Time.deltaTime;

        // Jump
        bool grounded = pos.y <= groundY && verticalVelocity <= 0f;
        if (grounded && kb.spaceKey.wasPressedThisFrame)
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

        // Fake gravity
        verticalVelocity += gravity * Time.deltaTime;
        pos.y += verticalVelocity * Time.deltaTime;

        if (pos.y <= groundY)
        {
            pos.y = groundY;
            verticalVelocity = 0f;
        }

        transform.position = pos;
    }
}