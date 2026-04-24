using System.Collections.Generic;
using UnityEngine;

public class ParticleWorld : MonoBehaviour
{
    private static List<Particle> particles = new(); // Hace una lista para ls partículas

    public static void Register(Particle p) // Cada partícula al momento de crerse, se registra
    {
        if (!particles.Contains(p)) particles.Add(p);
    }

    public static void Unregister(Particle p) // Cuando se desactiva, pues la quita de la lista
    {
        particles.Remove(p);
    }

    private void OnEnable()
    {
        if (SimulationManager.Instance == null) return;
        SimulationManager.Instance.OnSimulationStep += Step; // Cada paso de la simuación se suscribe al evento
        SimulationManager.Instance.OnSimulationReset += ResetAll; // Las reinicia
    }

    private void OnDisable()
    {
        if (SimulationManager.Instance == null) return;
        SimulationManager.Instance.OnSimulationStep -= Step;
        SimulationManager.Instance.OnSimulationReset -= ResetAll;
    }

    private void Step(float dt) // Los pasos dependen del tiempo
    {
        // 1. Integración: cada partícula actualiza su velocidad según fuerzas
        foreach (Particle p in particles) p.Integrate(dt);

        // 2. Colisiones contra el mundo
        foreach (Particle p in particles) p.Move(dt);

        // 3. Colisiones entre partículas
        ResolveParticlePairs();

        // 4. Sincronizar visuales
        foreach (Particle p in particles) p.UpdateVisuals();
    }

    private void ResolveParticlePairs() // Por parejas, recorre las partículas y a cada una la une con otra, si esas están en colisión calcul las fuerzas y colisiones
    {
        for (int i = 0; i < particles.Count; i++)
        {
            for (int j = i + 1; j < particles.Count; j++)
            {
                ResolvePair(particles[i], particles[j]);
            }
        }
    }

    public static void ResolvePair(Particle a, Particle b)
    {
        Vector3 delta = b.Position - a.Position;
        float dist = delta.magnitude;
        float sumR = a.Radius + b.Radius;

        // ¿Se están tocando?
        if (dist >= sumR || dist <= Mathf.Epsilon) return;

        // Normal de colisión: de a hacia b.
        Vector3 normal = delta / dist;

        // 1. Corregir penetración (separar las partículas)
        float overlap = sumR - dist;
        float totalMass = a.Mass + b.Mass;
        float aShare = b.Mass / totalMass;
        float bShare = a.Mass / totalMass;

        a.Position -= normal * (overlap * aShare);
        b.Position += normal * (overlap * bShare);

        // 2. Velocidad relativa a lo largo de la normal
        Vector3 relVel = b.Velocity - a.Velocity;
        float vRelN = Vector3.Dot(relVel, normal);

        // Si ya se están separando, no hay que hacer nada.
        if (vRelN > 0f) return;

        // 3. Impulso de colisión
        float e = 0.5f * (a.Restitution + b.Restitution);
        float jImpulse = -(1f + e) * vRelN / (1f / a.Mass + 1f / b.Mass);

        Vector3 impulse = jImpulse * normal;
        a.AddImpulse(-impulse);
        b.AddImpulse(impulse);
    }

    private void ResetAll()
    {
        foreach (Particle p in particles) p.ResetParticle();
    }
}
