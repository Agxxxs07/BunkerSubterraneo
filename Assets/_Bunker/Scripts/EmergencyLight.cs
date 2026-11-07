using UnityEngine;

/// <summary>
/// Luz de emergencia. BunkerStateManager le asigna color y modo:
/// rojo parpadeante (alarma), naranja con pulso lento (puerta abierta) y verde fijo (asegurada).
/// </summary>
public class EmergencyLight : MonoBehaviour
{
    [SerializeField] private Light targetLight;
    [SerializeField] private Renderer lensRenderer;
    [SerializeField] private int lensMaterialIndex = 1;
    [SerializeField] private float baseIntensity = 2.5f;
    [SerializeField] private float emissionBoost = 5f;
    [SerializeField] private float steadyFactor = 0.45f;

    private Color color = new Color(1f, 0.04f, 0.02f);
    private float hz = 1.6f;
    private bool blink = true;
    private float phase;
    private MaterialPropertyBlock block;
    private static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");
    private static readonly int ColorId = Shader.PropertyToID("_BaseColor");

    private void Awake() { block = new MaterialPropertyBlock(); phase = transform.position.x * 0.07f; }

    public void SetMode(Color c, float frequency, bool squareBlink)
    {
        color = c; hz = frequency; blink = squareBlink;
    }

    private void Update()
    {
        float k;
        if (hz <= 0f) k = steadyFactor;
        else
        {
            float s = Mathf.Sin((Time.time + phase) * hz * Mathf.PI * 2f);
            k = blink ? (s > 0f ? 1f : 0.05f) : 0.3f + 0.7f * (0.5f + 0.5f * s);
        }
        if (targetLight != null) { targetLight.color = color; targetLight.intensity = baseIntensity * k; }
        if (lensRenderer != null)
        {
            if (block == null) block = new MaterialPropertyBlock();
            lensRenderer.GetPropertyBlock(block, lensMaterialIndex);
            block.SetColor(ColorId, color);
            block.SetColor(EmissionId, color * emissionBoost * k);
            lensRenderer.SetPropertyBlock(block, lensMaterialIndex);
        }
    }
}
