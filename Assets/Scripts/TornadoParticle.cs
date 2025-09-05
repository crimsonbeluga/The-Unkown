using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class TornadoParticle : MonoBehaviour
{
    void Start()
    {
        var ps = GetComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = true;
        main.startLifetime = 2.5f;
        main.startSpeed = 3f;
        main.startSize = 0.05f;
        main.startColor = Color.black; // fully opaque
        main.maxParticles = 15000;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        var emission = ps.emission;
        emission.rateOverTime = 9000f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 20f;
        shape.radius = 1.8f;
        shape.length = 7f;
        shape.arc = 360f;
        shape.radiusThickness = 1f;

        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.orbitalY = 8f;
        velocity.y = new ParticleSystem.MinMaxCurve(5f, 8f);

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve flatCurve = new AnimationCurve();
        flatCurve.AddKey(0f, 1f);
        flatCurve.AddKey(1f, 1f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, flatCurve);

        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 4f;
        noise.frequency = 1.2f;
        noise.scrollSpeed = 0.4f;

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        var mat = new Material(Shader.Find("Particles/Standard Unlit"));
        mat.color = Color.black; // solid black
        mat.renderQueue = 2450; // Optional: render early, avoid blend artifacts
        renderer.material = mat;
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sortingOrder = 1;

        ps.Play();
    }
}
