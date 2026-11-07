using UnityEngine;

/// <summary>Genera en código los sonidos del nivel (sin archivos de audio): sirena y estruendo.</summary>
public static class ProceduralAudio
{
    public static AudioClip Siren(float seconds = 3f, int rate = 44100)
    {
        int n = Mathf.RoundToInt(seconds * rate); var data = new float[n]; float phase = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / rate;
            float f = 520f + 330f * (0.5f - 0.5f * Mathf.Cos(2f * Mathf.PI * t / seconds));   // sube y baja en cada ciclo
            phase += 2f * Mathf.PI * f / rate;
            float s = Mathf.Sin(phase) + 0.35f * Mathf.Sin(2f * phase) + 0.15f * Mathf.Sin(3f * phase);
            data[i] = s * 0.25f;
        }
        var clip = AudioClip.Create("Sirena", n, 1, rate, false); clip.SetData(data, 0); return clip;
    }

    public static AudioClip Rumble(float seconds = 3.5f, int rate = 44100, int seed = 7)
    {
        int n = Mathf.RoundToInt(seconds * rate); var data = new float[n]; var rng = new System.Random(seed);
        float lp1 = 0f, lp2 = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / rate;
            float white = (float)(rng.NextDouble() * 2.0 - 1.0);
            lp1 += 0.02f * (white - lp1); lp2 += 0.02f * (lp1 - lp2);             // ruido filtrado = retumbo grave
            float env = Mathf.Min(1f, t * 25f) * Mathf.Exp(-t * 1.1f);            // golpe y decaimiento
            float thump = Mathf.Sin(2f * Mathf.PI * 38f * t) * Mathf.Exp(-t * 3f);  // golpe sordo
            data[i] = Mathf.Clamp((lp2 * 9f + thump * 0.6f) * env, -1f, 1f);
        }
        var clip = AudioClip.Create("Estruendo", n, 1, rate, false); clip.SetData(data, 0); return clip;
    }
}
