using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Cuenta regresiva hasta el impacto. Al llegar a 0 avisa a BunkerStateManager para iniciar el bombardeo.
/// Por debajo de "warningAt" segundos el reloj se pone rojo y pulsa.
/// </summary>
public class AttackCountdown : MonoBehaviour
{
    [SerializeField] private float duration = 180f;
    [SerializeField] private float warningAt = 30f;
    [SerializeField] private Text timerText;
    [SerializeField] private Text captionText;
    [SerializeField] private Color normalColor = new Color(1f, 0.85f, 0.55f);
    [SerializeField] private Color warningColor = new Color(1f, 0.15f, 0.1f);

    public float Remaining { get; private set; }
    public bool Running { get; private set; } = true;
    public bool Finished { get; private set; }

    private void Start() { Remaining = duration; Refresh(); }

    public void Stop() { Running = false; }

    private void Update()
    {
        if (!Running || Finished) return;
        Remaining -= Time.deltaTime;
        if (Remaining <= 0f)
        {
            Remaining = 0f; Finished = true; Running = false; Refresh();
            if (BunkerStateManager.Instance != null) BunkerStateManager.Instance.OnAttackStarted();
            return;
        }
        Refresh();
    }

    private void Refresh()
    {
        if (timerText == null) return;
        int m = Mathf.FloorToInt(Remaining / 60f), s = Mathf.FloorToInt(Remaining % 60f), d = Mathf.FloorToInt(Remaining * 10f % 10f);
        timerText.text = Remaining > 0f ? $"{m:00}:{s:00}.{d}" : "00:00.0";
        bool warn = Remaining <= warningAt;
        float k = warn ? 0.55f + 0.45f * Mathf.Abs(Mathf.Sin(Time.time * Mathf.PI * 2f)) : 1f;
        Color c = warn ? warningColor : normalColor; c.a = k;
        timerText.color = c;
        timerText.fontSize = warn ? 58 : 50;
        if (captionText != null) captionText.text = Remaining > 0f ? "IMPACTO ESTIMADO EN" : "IMPACTO";
    }
}
