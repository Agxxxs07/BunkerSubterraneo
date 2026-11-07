using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

/// <summary>
/// Mecánica de la puerta blindada (zona Trigger + tecla de acción).
/// Va en la "Zona_Trigger" (BoxCollider isTrigger 2.7 x 2.5 x 2.8 m) frente a la puerta.
/// Flujo: entrar al trigger -> mensaje "Presiona E" -> E -> la puerta rota 90° sobre su pivote (bisagra)
/// con Quaternion.Slerp. Casos: salir del trigger oculta el mensaje, una puerta abierta no vuelve a girar
/// y la tecla solo funciona dentro del trigger.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class DoorInteraction : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Transform de la puerta con el origen en la bisagra (prefab PF_Puerta).")]
    [SerializeField] private Transform doorPivot;
    [SerializeField] private InputActionReference interactAction;
    [SerializeField] private InteractionPrompt prompt;

    [Header("Apertura")]
    [Tooltip("Grados sobre el eje Y del motor (Z en Blender). Negativo = abre hacia la Cámara B.")]
    [SerializeField] private float openAngle = -90f;
    [SerializeField] private float openDuration = 1.1f;
    [SerializeField] private AnimationCurve easing = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string promptText = "Presiona E para abrir la puerta blindada";

    [Header("Eventos")]
    public UnityEvent onDoorOpened;
    public UnityEvent onDoorClosed;

    private bool playerInside;
    private bool isOpen;
    private bool isMoving;
    private Quaternion closedRotation;
    private Quaternion openRotation;

    public bool IsOpen => isOpen;
    public bool IsMoving => isMoving;

    private void Reset()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void Awake()
    {
        GetComponent<BoxCollider>().isTrigger = true;
        if (doorPivot != null)
        {
            closedRotation = doorPivot.localRotation;
            openRotation = closedRotation * Quaternion.Euler(0f, openAngle, 0f);
        }
    }

    private void OnEnable() { if (interactAction != null) interactAction.action.Enable(); }
    private void OnDisable() { if (interactAction != null) interactAction.action.Disable(); }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInside = true;
        if (!isOpen && !isMoving && prompt != null) prompt.Show(promptText);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInside = false;
        if (prompt != null) prompt.Hide();
    }

    private void Update()
    {
        // La tecla E solo se atiende dentro del trigger
        if (!playerInside || interactAction == null) return;
        if (BunkerStateManager.Instance != null && BunkerStateManager.Instance.GameOver) return;
        if (!interactAction.action.WasPressedThisFrame()) return;
        // Si ya está abierta (o abriéndose), no gira otros 90°
        if (isOpen || isMoving) return;
        StartCoroutine(OpenRoutine());
    }

    private IEnumerator OpenRoutine()
    {
        isMoving = true;
        if (prompt != null) prompt.Hide();
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, openDuration);
            doorPivot.localRotation = Quaternion.Slerp(closedRotation, openRotation, easing.Evaluate(Mathf.Clamp01(t)));
            yield return null;
        }
        doorPivot.localRotation = openRotation;
        isMoving = false;
        isOpen = true;
        onDoorOpened?.Invoke();
    }

    /// <summary>Cierra la puerta (usado desde dentro de la Cámara B con L).</summary>
    public void Close()
    {
        if (!isOpen || isMoving) return;
        StartCoroutine(CloseRoutine());
    }

    private IEnumerator CloseRoutine()
    {
        isMoving = true;
        Quaternion from = doorPivot.localRotation;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, openDuration);
            doorPivot.localRotation = Quaternion.Slerp(from, closedRotation, easing.Evaluate(Mathf.Clamp01(t)));
            yield return null;
        }
        doorPivot.localRotation = closedRotation;
        isMoving = false;
        isOpen = false;
        onDoorClosed?.Invoke();
    }

    private void OnDrawGizmos()
    {
        var box = GetComponent<BoxCollider>();
        if (box == null) return;
        Gizmos.color = new Color(1f, 0.55f, 0.1f, 0.25f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(box.center, box.size);
        Gizmos.color = new Color(1f, 0.55f, 0.1f, 1f);
        Gizmos.DrawWireCube(box.center, box.size);
    }
}
