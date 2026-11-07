using System.Collections;
using UnityEngine;

/// <summary>Botón rojo de seguridad de la Cámara B. Al pulsarlo con E asegura la cámara (luces en verde).</summary>
public class RedButton : Interactable
{
    [SerializeField] private Transform cap;
    [SerializeField] private float pressDepth = 0.025f;

    public override string Prompt
    {
        get
        {
            var m = BunkerStateManager.Instance;
            if (m != null && m.State == BunkerState.Alarma) return "Botón de seguridad · primero abre la puerta blindada";
            return "E · Pulsar botón de seguridad";
        }
    }

    public override bool CanInteract
    {
        get { var m = BunkerStateManager.Instance; return m == null || m.State == BunkerState.Alarma || m.State == BunkerState.PuertaAbierta; }
    }

    public override void Interact(PlayerInteractor player)
    {
        StartCoroutine(Press());
        var m = BunkerStateManager.Instance;
        if (m != null && !m.SecureChamber() && m.State == BunkerState.Alarma) m.Notify("La puerta blindada sigue cerrada");
    }

    private IEnumerator Press()
    {
        if (cap == null) yield break;
        Vector3 start = cap.localPosition, pressed = start + new Vector3(0f, 0f, pressDepth); // +Z local = hacia la pared
        for (float t = 0; t < 1f; t += Time.deltaTime / 0.12f) { cap.localPosition = Vector3.Lerp(start, pressed, t); yield return null; }
        for (float t = 0; t < 1f; t += Time.deltaTime / 0.25f) { cap.localPosition = Vector3.Lerp(pressed, start, t); yield return null; }
        cap.localPosition = start;
    }
}
