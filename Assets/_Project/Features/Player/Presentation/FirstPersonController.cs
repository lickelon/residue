using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public sealed class FirstPersonController : MonoBehaviour
{
    [SerializeField] private Camera viewCamera;
    [SerializeField] private float walkSpeed = 3.2f;
    [SerializeField] private float sprintSpeed = 5.2f;
    [SerializeField] private float mouseSensitivity = 0.09f;
    [SerializeField] private float gravity = -18f;
    [SerializeField] private float cameraHeight = 1.62f;

    private CharacterController controller;
    private float pitch;
    private float verticalVelocity;
    private bool cursorLocked = true;

    public Camera ViewCamera => viewCamera;
    public float CurrentYaw => transform.eulerAngles.y;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (viewCamera == null)
        {
            viewCamera = GetComponentInChildren<Camera>();
        }

        if (viewCamera != null)
        {
            viewCamera.transform.localPosition = new Vector3(0f, cameraHeight, 0f);
        }
    }

    private void Start()
    {
        SetCursorLocked(true);
    }

    private void Update()
    {
        ToggleCursor();
        Look();
        Move();
    }

    private void ToggleCursor()
    {
        if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            return;
        }

        SetCursorLocked(!cursorLocked);
    }

    private void Look()
    {
        if (!cursorLocked || Mouse.current == null || viewCamera == null)
        {
            return;
        }

        Vector2 delta = Mouse.current.delta.ReadValue();
        transform.Rotate(Vector3.up, delta.x * mouseSensitivity, Space.World);
        pitch = Mathf.Clamp(pitch - delta.y * mouseSensitivity, -82f, 82f);
        viewCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void Move()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        Vector2 input = Vector2.zero;
        if (Keyboard.current.wKey.isPressed)
        {
            input.y += 1f;
        }
        if (Keyboard.current.sKey.isPressed)
        {
            input.y -= 1f;
        }
        if (Keyboard.current.dKey.isPressed)
        {
            input.x += 1f;
        }
        if (Keyboard.current.aKey.isPressed)
        {
            input.x -= 1f;
        }

        input = Vector2.ClampMagnitude(input, 1f);
        float speed = Keyboard.current.leftShiftKey.isPressed ? sprintSpeed : walkSpeed;
        Vector3 move = transform.right * input.x + transform.forward * input.y;

        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -1f;
        }

        verticalVelocity += gravity * Time.deltaTime;
        Vector3 velocity = move * speed + Vector3.up * verticalVelocity;
        controller.Move(velocity * Time.deltaTime);
    }

    private void SetCursorLocked(bool locked)
    {
        cursorLocked = locked;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
