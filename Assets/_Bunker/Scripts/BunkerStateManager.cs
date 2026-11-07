using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum BunkerState { Alarma, PuertaAbierta, Asegurada, Sellada }

/// <summary>
/// Estado global del bunker y flujo de la partida.
/// Alarma (rojo intermitente) -> PuertaAbierta (naranja) -> Asegurada (verde, botón rojo) -> Sellada (puerta cerrada con L).
/// Cuando AttackCountdown llega a 0 empieza el bombardeo: si el bunker está sellado el jugador sobrevive;
/// si no, la partida termina y se puede reintentar con R.
/// </summary>
public class BunkerStateManager : MonoBehaviour
{
    public static BunkerStateManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private Text objectiveText;
    [SerializeField] private InteractionPrompt introBanner;
    [SerializeField] private InteractionPrompt notice;
    [SerializeField] private CanvasGroup endPanel;
    [SerializeField] private Text endTitle;
    [SerializeField] private Text endBody;
    [SerializeField] private float introDuration = 9f;
    [SerializeField, TextArea] private string introMessage =
        "ALERTA NACIONAL · Se confirma el lanzamiento de misiles. La Tercera Guerra Mundial ha comenzado.\nLlega a la puerta blindada y sella la Cámara B antes del impacto.";

    [Header("Sistemas")]
    [SerializeField] private AttackCountdown countdown;
    [SerializeField] private ImpactShaker shaker;
    [SerializeField] private AudioSource siren;
    [SerializeField] private FirstPersonController player;
    [SerializeField] private InputActionReference restartAction;
    [SerializeField] private float failDelay = 4f;

    [Header("Colores de las luces")]
    [SerializeField] private Color alarmColor = new Color(1f, 0.04f, 0.02f);
    [SerializeField] private Color openColor = new Color(1f, 0.42f, 0.04f);
    [SerializeField] private Color secureColor = new Color(0.2f, 1f, 0.35f);

    public BunkerState State { get; private set; } = BunkerState.Alarma;
    public bool AttackStarted { get; private set; }
    public bool GameOver { get; private set; }
    private bool ended;

    private void Awake() { Instance = this; }
    private void OnEnable() { if (restartAction != null) restartAction.action.Enable(); }

    private void Start()
    {
        if (endPanel != null) { endPanel.alpha = 0f; endPanel.blocksRaycasts = false; }
        if (siren != null) { if (siren.clip == null) siren.clip = ProceduralAudio.Siren(); siren.loop = true; siren.volume = 0.35f; siren.Play(); }
        Apply();
        if (introBanner != null) { introBanner.Show(introMessage); Invoke(nameof(HideIntro), introDuration); }
    }

    private void HideIntro() { if (introBanner != null) introBanner.Hide(); }

    private void Update()
    {
        if (ended && restartAction != null && restartAction.action.WasPressedThisFrame())
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnDoorOpened()
    {
        if (State != BunkerState.Alarma || GameOver) return;
        State = BunkerState.PuertaAbierta;
        HideIntro();
        Apply();
    }

    /// <summary>Botón rojo de la Cámara B.</summary>
    public bool SecureChamber()
    {
        if (State != BunkerState.PuertaAbierta || GameOver) return false;
        State = BunkerState.Asegurada;
        Apply();
        Notify("Cámara B asegurada");
        return true;
    }

    /// <summary>Puerta cerrada desde dentro con L.</summary>
    public void OnDoorLocked()
    {
        if (GameOver) return;
        if (State == BunkerState.Asegurada)
        {
            State = BunkerState.Sellada; Apply();
            if (shaker != null) shaker.SetDamping(0.45f);
            if (siren != null) siren.volume = 0.08f;          // la sirena se oye amortiguada tras la puerta
            Notify("Puerta bloqueada. Bunker sellado");
        }
        else Notify("Puerta cerrada");
    }

    /// <summary>La cuenta regresiva llegó a 0.</summary>
    public void OnAttackStarted()
    {
        if (AttackStarted) return;
        AttackStarted = true;
        if (siren != null) siren.Stop();
        if (shaker != null) shaker.StartBombardment();
        if (State == BunkerState.Sellada) StartCoroutine(Survived());
        else StartCoroutine(Failed());
    }

    private IEnumerator Survived()
    {
        if (objectiveText != null) objectiveText.text = "Estás a salvo · El bombardeo continúa afuera. Descansa en la cama";
        yield return new WaitForSeconds(2.5f);
        ShowEnd("SOBREVIVISTE", "El bunker resistió el primer impacto.\nPuedes seguir explorando o pulsar R para jugar de nuevo.", new Color(0.02f, 0.12f, 0.05f, 0.75f), false);
        yield return new WaitForSeconds(6f);
        if (endPanel != null) endPanel.alpha = 0f;
    }

    private IEnumerator Failed()
    {
        GameOver = true;
        if (objectiveText != null) objectiveText.text = "¡IMPACTO! No sellaste la Cámara B a tiempo";
        yield return new WaitForSeconds(failDelay);
        if (player != null) player.InputEnabled = false;
        ShowEnd("NO LLEGASTE A TIEMPO", "El impacto alcanzó el bunker antes de que la Cámara B quedara sellada.\nPulsa R para reintentar.", new Color(0.2f, 0.01f, 0.01f, 0.85f), true);
    }

    private void ShowEnd(string title, string body, Color bg, bool stay)
    {
        ended = true;
        if (endPanel == null) return;
        var img = endPanel.GetComponent<Image>(); if (img != null) img.color = bg;
        if (endTitle != null) endTitle.text = title;
        if (endBody != null) endBody.text = body;
        StartCoroutine(Fade(endPanel, 1f, 1.2f));
    }

    private static IEnumerator Fade(CanvasGroup g, float to, float time)
    {
        float from = g.alpha;
        for (float t = 0; t < 1f; t += Time.deltaTime / time) { g.alpha = Mathf.Lerp(from, to, t); yield return null; }
        g.alpha = to;
    }

    public void Notify(string text, float seconds = 3f)
    {
        if (notice != null) notice.ShowFor(text, seconds);
    }

    private void Apply()
    {
        Color c; float hz; bool blink;
        switch (State)
        {
            case BunkerState.Alarma: c = alarmColor; hz = 1.6f; blink = true; break;
            case BunkerState.PuertaAbierta: c = openColor; hz = 0.7f; blink = false; break;
            default: c = secureColor; hz = 0f; blink = false; break;
        }
        foreach (var l in FindObjectsByType<EmergencyLight>(FindObjectsSortMode.None)) l.SetMode(c, hz, blink);

        if (objectiveText == null) return;
        switch (State)
        {
            case BunkerState.Alarma: objectiveText.text = "OBJETIVO: Llega a la puerta blindada antes del impacto"; break;
            case BunkerState.PuertaAbierta: objectiveText.text = "OBJETIVO: Lee el aviso y pulsa el botón rojo de la Cámara B"; break;
            case BunkerState.Asegurada: objectiveText.text = "OBJETIVO: Cierra la puerta blindada desde dentro con L"; break;
            case BunkerState.Sellada: objectiveText.text = "Bunker sellado · Espera el impacto dentro del refugio"; break;
        }
    }
}
