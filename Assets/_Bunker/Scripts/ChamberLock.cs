using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Volumen trigger que cubre el interior de la Cámara B. Dentro de él, la tecla L (Lock)
/// cierra la puerta blindada desde dentro.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class ChamberLock : MonoBehaviour
{
    [SerializeField] private DoorInteraction door;
    [SerializeField] private InputActionReference lockAction;
    [SerializeField] private string playerTag = "Player";
    private bool inside;

    private void Awake() { GetComponent<BoxCollider>().isTrigger = true; }
    private void OnEnable() { if (lockAction != null) lockAction.action.Enable(); }
    private void OnTriggerEnter(Collider other) { if (other.CompareTag(playerTag)) inside = true; }
    private void OnTriggerExit(Collider other) { if (other.CompareTag(playerTag)) inside = false; }

    private void Update()
    {
        if (!inside || door == null || lockAction == null || !lockAction.action.WasPressedThisFrame()) return;
        if (BunkerStateManager.Instance != null && BunkerStateManager.Instance.GameOver) return;
        if (door.IsOpen && !door.IsMoving)
        {
            door.Close();
            if (BunkerStateManager.Instance != null) BunkerStateManager.Instance.OnDoorLocked();
        }
    }
}
