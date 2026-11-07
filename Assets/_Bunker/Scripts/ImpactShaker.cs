using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Bombardeo: sacude la cámara, hace parpadear las luces, suelta polvo del techo y reproduce estruendos.
/// Va en el CameraPivot. Se ejecuta antes que FirstPersonController para quitar el desplazamiento del
/// cuadro anterior y lo vuelve a aplicar en LateUpdate.
/// </summary>
[DefaultExecutionOrder(-100)]
public class ImpactShaker : MonoBehaviour
{
    [SerializeField] private float positionAmplitude = 0.09f;
    [SerializeField] private float rotationAmplitude = 2.6f;
    [SerializeField] private Vector2 intervalRange = new Vector2(3.5f, 7f);
    [SerializeField] private ParticleSystem[] dust;
    [SerializeField] private AudioSource rumbleSource;

    private float trauma;          // 0..1, decae con el tiempo
    private float damping = 1f;    // 0.45 cuando el bunker está sellado (temblor amortiguado)
    private Vector3 appliedPos;
    private Quaternion appliedRot = Quaternion.identity;
    private readonly Dictionary<Light, float> baseIntensity = new Dictionary<Light, float>();
    private bool running;

    public bool Running => running;

    private void Start()
    {
        foreach (var l in FindObjectsByType<Light>(FindObjectsSortMode.None))
            if (l.type == LightType.Point && l.GetComponentInParent<Camera>() == null) baseIntensity[l] = l.intensity;
        if (rumbleSource != null && rumbleSource.clip == null) rumbleSource.clip = ProceduralAudio.Rumble();
    }

    public void SetDamping(float d) { damping = d; }

    public void StartBombardment()
    {
        if (running) return;
        running = true;
        StartCoroutine(Bombard());
    }

    private IEnumerator Bombard()
    {
        Impact(1f);
        while (running)
        {
            yield return new WaitForSeconds(Random.Range(intervalRange.x, intervalRange.y));
            Impact(Random.Range(0.45f, 0.9f));
        }
    }

    public void Impact(float strength)
    {
        trauma = Mathf.Clamp01(trauma + strength);
        if (rumbleSource != null) { rumbleSource.pitch = Random.Range(0.85f, 1.1f); rumbleSource.volume = Mathf.Lerp(0.5f, 1f, strength) * Mathf.Lerp(0.6f, 1f, damping); rumbleSource.Play(); }
        if (dust != null) foreach (var d in dust) if (d != null) d.Emit(Mathf.RoundToInt(220 * strength * damping));
    }

    private void Update()
    {
        // quitar el desplazamiento aplicado en el cuadro anterior
        transform.localPosition -= appliedPos;
        transform.localRotation = Quaternion.Inverse(appliedRot) * transform.localRotation;
        appliedPos = Vector3.zero; appliedRot = Quaternion.identity;
    }

    private void LateUpdate()
    {
        trauma = Mathf.Max(0f, trauma - Time.deltaTime * 0.45f);
        float shake = trauma * trauma * damping;
        if (shake > 0.0001f)
        {
            float t = Time.time * 22f;
            appliedPos = new Vector3(Mathf.PerlinNoise(t, 0.1f) - 0.5f, Mathf.PerlinNoise(0.2f, t) - 0.5f, Mathf.PerlinNoise(t, t * 0.3f) - 0.5f) * 2f * positionAmplitude * shake;
            appliedRot = Quaternion.Euler((Mathf.PerlinNoise(t * 0.8f, 3f) - 0.5f) * 2f * rotationAmplitude * shake,
                                          (Mathf.PerlinNoise(5f, t * 0.8f) - 0.5f) * 2f * rotationAmplitude * shake,
                                          (Mathf.PerlinNoise(t * 0.6f, 9f) - 0.5f) * 2f * rotationAmplitude * shake);
            transform.localPosition += appliedPos;
            transform.localRotation = transform.localRotation * appliedRot;
        }
        // parpadeo de luces durante el temblor
        foreach (var kv in baseIntensity)
        {
            if (kv.Key == null || kv.Key.GetComponent<EmergencyLightTag>() != null) continue;
            float flicker = trauma > 0.15f ? Mathf.Lerp(1f, Random.Range(0.15f, 1f), trauma) : 1f;
            kv.Key.intensity = kv.Value * flicker;
        }
    }
}
