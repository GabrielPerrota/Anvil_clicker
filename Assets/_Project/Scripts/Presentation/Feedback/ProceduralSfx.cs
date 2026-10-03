using UnityEngine;

namespace AnvilClicker.Presentation
{
    /// <summary>
    /// Placeholder sound effects synthesised at startup (no audio assets yet): metallic partials with an
    /// exponential decay plus a short noise burst for the hammer's impact.
    /// </summary>
    public sealed class ProceduralSfx : MonoBehaviour
    {
        const int SampleRate = 44100;

        [SerializeField, Range(0f, 1f)] float volume = 0.5f;
        [SerializeField] float pitchVariance = 0.08f;
        [SerializeField] int voices = 8;

        AudioSource[] _sources;
        int _nextSource;
        AudioClip _strike;
        AudioClip _critical;
        AudioClip _forged;

        void Awake()
        {
            _sources = new AudioSource[voices];
            for (var i = 0; i < voices; i++)
            {
                var source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.spatialBlend = 0f;
                _sources[i] = source;
            }

            // Inharmonic ratios typical of struck metal bars.
            _strike = CreateMetalClip("strike", 1250f, new[] { 1f, 2.76f, 5.40f }, decay: 18f, noise: 0.35f, length: 0.35f);
            _critical = CreateMetalClip("critical", 820f, new[] { 1f, 2.76f, 5.40f, 8.93f }, decay: 7f, noise: 0.55f, length: 0.8f);
            _forged = CreateChimeClip("forged", 1568f, 2093f, length: 0.6f);
        }

        public void PlayStrike(bool critical) => Play(critical ? _critical : _strike, critical ? 1f : 0.8f);

        public void PlayForged() => Play(_forged, 0.6f, varyPitch: false);

        void Play(AudioClip clip, float gain, bool varyPitch = true)
        {
            var source = _sources[_nextSource];
            _nextSource = (_nextSource + 1) % _sources.Length;

            source.pitch = varyPitch ? 1f + Random.Range(-pitchVariance, pitchVariance) : 1f;
            source.PlayOneShot(clip, volume * gain);
        }

        static AudioClip CreateMetalClip(string name, float baseFrequency, float[] ratios, float decay, float noise, float length)
        {
            var samples = new float[Mathf.CeilToInt(SampleRate * length)];
            var rng = new System.Random(name.GetHashCode());

            for (var i = 0; i < samples.Length; i++)
            {
                var t = (float)i / SampleRate;
                var value = 0f;
                for (var p = 0; p < ratios.Length; p++)
                {
                    // Higher partials die out faster, like a real anvil.
                    var partialDecay = Mathf.Exp(-decay * (1f + p * 0.6f) * t);
                    value += Mathf.Sin(2f * Mathf.PI * baseFrequency * ratios[p] * t) * partialDecay / (p + 1);
                }

                var impact = Mathf.Exp(-t * 180f) * ((float)rng.NextDouble() * 2f - 1f) * noise;
                samples[i] = (value * 0.6f + impact) * 0.8f;
            }

            return ToClip(name, samples);
        }

        static AudioClip CreateChimeClip(string name, float first, float second, float length)
        {
            var samples = new float[Mathf.CeilToInt(SampleRate * length)];
            var secondStart = 0.09f;

            for (var i = 0; i < samples.Length; i++)
            {
                var t = (float)i / SampleRate;
                var a = Mathf.Sin(2f * Mathf.PI * first * t) * Mathf.Exp(-6f * t);
                var b = t < secondStart ? 0f : Mathf.Sin(2f * Mathf.PI * second * (t - secondStart)) * Mathf.Exp(-5f * (t - secondStart));
                samples[i] = (a + b) * 0.35f;
            }

            return ToClip(name, samples);
        }

        static AudioClip ToClip(string name, float[] samples)
        {
            var clip = AudioClip.Create($"sfx_{name}", samples.Length, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
