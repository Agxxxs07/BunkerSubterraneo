using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Raycast desde la cámara: detecta el Interactable al que mira el jugador (hasta 2.5 m),
/// muestra su mensaje y lo usa con la acción Interact (E).
/// </summary>
public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private Camera viewCamera;
    [SerializeField] private InputActionReference interactAction;
    [SerializeField] private InteractionPrompt lookPrompt;
    [SerializeField] private float reach = 2.5f;
    [SerializeField] private LayerMask mask = ~0;

    public Inventory Inventory { get; private set; }
    private Interactable current;

    private void Awake() { Inventory = GetComponent<Inventory>(); if (viewCamera == null) viewCamera = GetComponentInChildren<Camera>(); }
    private void OnEnable() { if (interactAction != null) interactAction.action.Enable(); }
    private void OnDisable() { current = null; if (lookPrompt != null) lookPrompt.Hide(); }

    private void Update()
    {
        if (BunkerStateManager.Instance != null && BunkerStateManager.Instance.GameOver) { if (lookPrompt != null) lookPrompt.Hide(); return; }
        Interactable found = null;
        var ray = new Ray(viewCamera.transform.position, viewCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, reach, mask, QueryTriggerInteraction.Ignore))
            found = hit.collider.GetComponentInParent<Interactable>();
        if (found != null && !found.CanInteract) found = null;

        if (found != current)
        {
            current = found;
            if (lookPrompt != null) { if (current != null) lookPrompt.Show(current.Prompt); else lookPrompt.Hide(); }
        }
        else if (current != null && lookPrompt != null) lookPrompt.Show(current.Prompt);

        if (current != null && interactAction != null && interactAction.action.WasPressedThisFrame())
        {
            current.Interact(this);
            current = null;
            if (lookPrompt != null) lookPrompt.Hide();
        }
    }
}
