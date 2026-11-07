using UnityEngine;

public enum ItemKind { Linterna, Herramienta, Suministro }

/// <summary>Objeto que se recoge con E y pasa al inventario del jugador.</summary>
public class PickupItem : Interactable
{
    [SerializeField] private string itemName = "Objeto";
    [SerializeField] private ItemKind kind = ItemKind.Herramienta;
    private bool available = true;

    public string ItemName => itemName;
    public ItemKind Kind => kind;
    public override string Prompt => "E · Recoger " + itemName.ToLower();
    public override bool CanInteract => available;

    public void SetAvailable(bool value)
    {
        available = value;
        foreach (var c in GetComponentsInChildren<Collider>(true)) c.enabled = value;
    }

    public override void Interact(PlayerInteractor player)
    {
        if (player.Inventory != null) player.Inventory.Add(this);
        gameObject.SetActive(false);
    }
}
