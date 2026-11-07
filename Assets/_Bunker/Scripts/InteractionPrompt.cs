using UnityEngine;
using UnityEngine.UI;

/// <summary>Mensaje en pantalla con aparición suave. ShowFor lo oculta solo después de unos segundos.</summary>
[RequireComponent(typeof(CanvasGroup))]
public class InteractionPrompt : MonoBehaviour
{
    [SerializeField] private Text label;
    [SerializeField] private float fadeSpeed = 8f;
    private CanvasGroup group;
    private float target;
    private float hideAt = -1f;

    private void Awake() { group = GetComponent<CanvasGroup>(); group.alpha = 0f; }

    public void Show(string text) { if (label != null) label.text = text; target = 1f; hideAt = -1f; }
    public void ShowFor(string text, float seconds) { Show(text); hideAt = Time.time + seconds; }
    public void Hide() { target = 0f; hideAt = -1f; }
    public bool IsShowing => target > 0f;

    private void Update()
    {
        if (hideAt > 0f && Time.time >= hideAt) Hide();
        group.alpha = Mathf.MoveTowards(group.alpha, target, fadeSpeed * Time.deltaTime);
    }
}
