using System.Collections;
using UnityEngine;

/// <summary>Cofre de suministros: E abre la tapa sobre su bisagra y habilita los objetos de su interior.</summary>
public class ChestInteractable : Interactable
{
    [SerializeField] private Transform lid;
    [SerializeField] private float openAngle = -105f;
    [SerializeField] private float duration = 0.9f;
    [SerializeField] private PickupItem[] contents;
    private bool open, moving;

    public override string Prompt => "E · Abrir cofre";
    public override bool CanInteract => !open && !moving;

    private void Start() { foreach (var c in contents) if (c != null) c.SetAvailable(false); }

    public override void Interact(PlayerInteractor player) { if (!open && !moving) StartCoroutine(Open()); }

    private IEnumerator Open()
    {
        moving = true;
        Quaternion a = lid.localRotation, b = a * Quaternion.Euler(openAngle, 0f, 0f);
        for (float t = 0; t < 1f; t += Time.deltaTime / duration)
        {
            lid.localRotation = Quaternion.Slerp(a, b, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        lid.localRotation = b;
        moving = false; open = true;
        foreach (var c in contents) if (c != null) c.SetAvailable(true);
    }
}
