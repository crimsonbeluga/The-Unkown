// File: LivingDarknessParticles.cs

using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class LivingDarknessParticles : MonoBehaviour
{
    [Tooltip("Assign a dark smoke-like material with alpha transparency")]
    public Material darknessMaterial;

    void Start()
    {
        CreateEmitter("CoreEmitter", 5f, 0.05f, 0.15f, 2f, 40000f, 4000, 6000, 10);
        CreateEmitter("MidEmitter", 10f, 0.05f, 0.15f, 4f, 30000f, 3000, 5000, 9);
        CreateEmitter("OuterEmitter", 15f, 0.05f, 0.15f, 6f, 20000f, 2000, 4000, 8);
    }

    void CreateEmitter(string name, float radius, float minSize, float maxSize, float lifetime, float rateOverTime, int burstMin, int burstMax, int sortingOrder)
    {
        GameObject emitterGO = new GameObject(name);
        emitterGO.transform.position = this.transform.position;

        var ps = emitterGO.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor = Color.black;
        main.startSize = new ParticleSystem.MinMaxCurve(minSize, maxSize);
        main.startLifetime = lifetime;
        main.maxParticles = 500000;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.05f, 0.3f);

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new(Color.black, 0f), new(Color.black, 1f) },
            new GradientAlphaKey[] { new(0f, 0f), new(1f, 0.3f), new(0f, 1f) }
        );
        colorOverLifetime.color = grad;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 0.15f),
            new Keyframe(0.5f, 0.3f),
            new Keyframe(1f, 0.1f)
        ));

        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.4f;
        noise.frequency = 1.2f;
        noise.scrollSpeed = 0.25f;
        noise.octaveCount = 2;
        noise.quality = ParticleSystemNoiseQuality.Medium;

        var emission = ps.emission;
        emission.rateOverTime = rateOverTime;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(1f, new ParticleSystem.MinMaxCurve(burstMin, burstMax)),
            new ParticleSystem.Burst(3.5f, new ParticleSystem.MinMaxCurve(burstMin, burstMax)),
            new ParticleSystem.Burst(6f, new ParticleSystem.MinMaxCurve(burstMin, burstMax))
        });

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = radius;
        shape.radiusThickness = 1f;

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = darknessMaterial;
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sortingOrder = sortingOrder;
    }
}
