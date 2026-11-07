using UnityEngine;

/// <summary>Base para objetos que se usan mirándolos y pulsando E (botón, cofre, objetos recogibles).</summary>
public abstract class Interactable : MonoBehaviour
{
    public abstract string Prompt { get; }
    public virtual bool CanInteract => true;
    public abstract void Interact(PlayerInteractor player);
}
