using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controlador en primera persona del Bunker Subterráneo.
/// Usa CharacterController (sin Rigidbody) y el nuevo Input System:
/// WASD / stick izquierdo para moverse, ratón / stick derecho para mirar, Shift para correr.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Input System")]
    [SerializeField] private InputActionReference move;
    [SerializeField] private InputActionReference look;
    [SerializeField] private InputActionReference sprint;
    [SerializeField] private InputActionReference pause;

    [Header("Cámara")]
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private float mouseSensitivity = 0.08f;
    [SerializeField] private float maxPitch = 85f;

    [Header("Movimiento (m/s, escala 1 unidad = 1 m)")]
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float sprintSpeed = 5.5f;
    [SerializeField] private float acceleration = 12f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Balanceo de cámara")]
    [SerializeField] private float bobAmplitude = 0.035f;
    [SerializeField] private float bobFrequency = 1.9f;

    private CharacterController controller;
    private Vector3 planarVelocity;
    private float verticalVelocity;
    private float pitch;
    private float bobPhase;
    private float cameraBaseHeight;
    private bool inputEnabled = true;

    public bool InputEnabled { get => inputEnabled; set => inputEnabled = value; }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (cameraPivot != null) cameraBaseHeight = cameraPivot.localPosition.y;
    }

    private void OnEnable()
    {
        foreach (var a in new[] { move, look, sprint, pause })
            if (a != null) a.action.Enable();
        LockCursor(true);
    }

    private void OnDisable()
    {
        foreach (var a in new[] { move, look, sprint, pause })
            if (a != null) a.action.Disable();
    }

    private void Update()
    {
        if (pause != null && pause.action.WasPressedThisFrame())
            LockCursor(Cursor.lockState != CursorLockMode.Locked);

        bool active = inputEnabled && Cursor.lockState == CursorLockMode.Locked;
        HandleLook(active);
        HandleMove(active);
    }

    private void HandleLook(bool active)
    {
        if (!active || look == null) return;
        Vector2 delta = look.action.ReadValue<Vector2>() * mouseSensitivity;
        transform.Rotate(0f, delta.x, 0f);
        pitch = Mathf.Clamp(pitch - delta.y, -maxPitch, maxPitch);
        if (cameraPivot != null) cameraPivot.localEulerAngles = new Vector3(pitch, 0f, 0f);
    }

    private void HandleMove(bool active)
    {
        Vector2 input = active && move != null ? move.action.ReadValue<Vector2>() : Vector2.zero;
        input = Vector2.ClampMagnitude(input, 1f);
        bool running = active && sprint != null && sprint.action.IsPressed();
        float speed = running ? sprintSpeed : walkSpeed;

        Vector3 target = (transform.right * input.x + transform.forward * input.y) * speed;
        planarVelocity = Vector3.MoveTowards(planarVelocity, target, acceleration * Time.deltaTime);

        if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
        verticalVelocity += gravity * Time.deltaTime;

        Vector3 motion = planarVelocity + Vector3.up * verticalVelocity;
        controller.Move(motion * Time.deltaTime);

        // Balanceo sutil de la cámara al caminar
        if (cameraPivot != null)
        {
            float horizontalSpeed = new Vector3(controller.velocity.x, 0f, controller.velocity.z).magnitude;
            if (controller.isGrounded && horizontalSpeed > 0.1f)
                bobPhase += Time.deltaTime * bobFrequency * (horizontalSpeed / walkSpeed) * Mathf.PI * 2f;
            float offset = Mathf.Sin(bobPhase) * bobAmplitude * Mathf.Clamp01(horizontalSpeed / walkSpeed);
            Vector3 p = cameraPivot.localPosition;
            p.y = Mathf.Lerp(p.y, cameraBaseHeight + offset, 10f * Time.deltaTime);
            cameraPivot.localPosition = p;
        }
    }

    public void ResetPitch() { pitch = 0f; if (cameraPivot != null) cameraPivot.localEulerAngles = Vector3.zero; }

    public static void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
