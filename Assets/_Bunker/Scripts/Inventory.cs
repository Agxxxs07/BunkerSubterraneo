using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>Inventario del jugador y linterna (F para encender/apagar una vez recogida).</summary>
public class Inventory : MonoBehaviour
{
    [SerializeField] private Text hudText;
    [SerializeField] private Light flashlight;
    [SerializeField] private GameObject flashlightViewModel;
    [SerializeField] private InputActionReference toggleFlashlight;

    private readonly List<string> items = new List<string>();
    public bool HasFlashlight { get; private set; }

    private void OnEnable() { if (toggleFlashlight != null) toggleFlashlight.action.Enable(); }
    private void Start()
    {
        if (flashlight != null) flashlight.enabled = false;
        if (flashlightViewModel != null) flashlightViewModel.SetActive(false);
        Refresh();
    }

    public void Add(PickupItem item)
    {
        items.Add(item.ItemName);
        string msg = item.ItemName + " recogida";
        if (item.Kind == ItemKind.Linterna)
        {
            HasFlashlight = true;
            if (flashlightViewModel != null) flashlightViewModel.SetActive(true);
            if (flashlight != null) flashlight.enabled = true;
            msg = "Linterna recogida · F para encender o apagar";
        }
        if (BunkerStateManager.Instance != null) BunkerStateManager.Instance.Notify(msg);
        Refresh();
    }

    private void Update()
    {
        if (HasFlashlight && flashlight != null && toggleFlashlight != null && toggleFlashlight.action.WasPressedThisFrame())
            flashlight.enabled = !flashlight.enabled;
    }

    private void Refresh()
    {
        if (hudText == null) return;
        hudText.text = items.Count == 0 ? "" : "INVENTARIO\n" + string.Join("\n", items);
    }
}
