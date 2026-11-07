using UnityEngine;

/// <summary>Meta del recorrido (Cámara B). Muestra el mensaje final al llegar.</summary>
[RequireComponent(typeof(Collider))]
public class GoalTrigger : MonoBehaviour
{
    [SerializeField] private InteractionPrompt prompt;
    [SerializeField] private string message = "Objetivo alcanzado: Cámara B asegurada";
    [SerializeField] private string playerTag = "Player";
    private bool reached;

    private void OnTriggerEnter(Collider other)
    {
        if (reached || !other.CompareTag(playerTag)) return;
        reached = true;
        if (prompt != null) prompt.Show(message);
    }
}
