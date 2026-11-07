using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Catre: E para acostarse (la vista baja a la almohada y la pantalla se oscurece al dormir),
/// E otra vez para levantarse junto a la cama.
/// </summary>
public class Bed : Interactable
{
    [SerializeField] private Transform headPoint;      // posición y orientación de la cabeza acostada
    [SerializeField] private Transform standPoint;     // donde queda el jugador al levantarse
    [SerializeField] private InputActionReference interactAction;
    [SerializeField] private CanvasGroup sleepOverlay;
    [SerializeField] private Text sleepText;
    [SerializeField] private float moveTime = 1.1f;
    [SerializeField] private float sleepFadeTime = 3f;

    private bool occupied, busy;
    private FirstPersonController fpc;
    private CharacterController cc;
    private PlayerInteractor interactor;
    private Transform pivot;
    private Vector3 savedLocalPos;
    private Quaternion savedLocalRot;

    public bool Occupied => occupied;
    public override string Prompt => "E · Acostarse a dormir";
    public override bool CanInteract => !occupied && !busy;

    public override void Interact(PlayerInteractor player)
    {
        if (occupied || busy) return;
        interactor = player; fpc = player.GetComponent<FirstPersonController>(); cc = player.GetComponent<CharacterController>();
        pivot = player.GetComponentInChildren<Camera>().transform;
        StartCoroutine(LieDown());
    }

    private IEnumerator LieDown()
    {
        busy = true;
        fpc.enabled = false; interactor.enabled = false;
        savedLocalPos = pivot.localPosition; savedLocalRot = pivot.localRotation;
        Vector3 p0 = pivot.position; Quaternion r0 = pivot.rotation;
        for (float t = 0; t < 1f; t += Time.deltaTime / moveTime)
        {
            float k = Mathf.SmoothStep(0f, 1f, t);
            pivot.position = Vector3.Lerp(p0, headPoint.position, k);
            pivot.rotation = Quaternion.Slerp(r0, headPoint.rotation, k);
            yield return null;
        }
        pivot.SetPositionAndRotation(headPoint.position, headPoint.rotation);
        occupied = true; busy = false;
        if (sleepText != null) sleepText.text = "Durmiendo...\nE para levantarte";
        float a = 0f;
        while (occupied && a < 0.88f)
        {
            a = Mathf.MoveTowards(a, 0.88f, Time.deltaTime / sleepFadeTime);
            if (sleepOverlay != null) sleepOverlay.alpha = a;
            yield return null;
        }
    }

    private void Update()
    {
        if (occupied && !busy && interactAction != null && interactAction.action.WasPressedThisFrame())
            StartCoroutine(GetUp());
    }

    private IEnumerator GetUp()
    {
        busy = true; occupied = false;
        float a0 = sleepOverlay != null ? sleepOverlay.alpha : 0f;
        Vector3 standPos = standPoint.position;
        // mover la raíz del jugador junto a la cama, y llevar la cámara de vuelta a la altura de los ojos
        cc.enabled = false; fpc.transform.position = standPos; fpc.transform.rotation = Quaternion.Euler(0f, standPoint.eulerAngles.y, 0f); cc.enabled = true;
        Vector3 p0 = pivot.localPosition; Quaternion r0 = pivot.localRotation;
        for (float t = 0; t < 1f; t += Time.deltaTime / moveTime)
        {
            float k = Mathf.SmoothStep(0f, 1f, t);
            pivot.localPosition = Vector3.Lerp(fpc.transform.InverseTransformPoint(headPoint.position), savedLocalPos, k);
            pivot.localRotation = Quaternion.Slerp(Quaternion.Inverse(fpc.transform.rotation) * headPoint.rotation, Quaternion.identity, k);
            if (sleepOverlay != null) sleepOverlay.alpha = Mathf.Lerp(a0, 0f, k);
            yield return null;
        }
        pivot.localPosition = savedLocalPos; pivot.localRotation = Quaternion.identity;
        if (sleepOverlay != null) sleepOverlay.alpha = 0f;
        fpc.ResetPitch();
        fpc.enabled = true; interactor.enabled = true; busy = false;
    }
}
