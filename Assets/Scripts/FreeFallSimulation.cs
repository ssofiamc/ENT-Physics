using UnityEngine;

public class FreeFallSimulation : MonoBehaviour
{
    [Header("Physical Parameters")]
    public float initialHeight = 10f;
    public float initialVelocity = 0f;
    public float groundHeight = 0f;
    public float gravity = -9.81f;
    [Range(0f, 1f)] public float restitution = .8f;
    [Min(0f)] public float dragCoefficient = 0.1f;
    [Min(0.0001f)] public float massFactor = 1f;
    public float stopThreshold = 0.05f;

    private float velocity;
    private float position;
    private float mass;
    private bool isActive = true;

    private void OnEnable()
    {
        if (SimulationManager.Instance != null) SimulationManager.Instance.OnSimulationStep += Step;
        if (SimulationManager.Instance != null) SimulationManager.Instance.OnSimulationReset += ResetSimulation;
    }

    private void OnDisable()
    {
        if (SimulationManager.Instance != null) SimulationManager.Instance.OnSimulationStep -= Step;
        if (SimulationManager.Instance != null) SimulationManager.Instance.OnSimulationReset -= ResetSimulation;
    }

    private void OnValidate() => ComputeMass();

    private void Start() => ResetSimulation();

    private void Step(float dt)
    {
        if (!isActive) return;

        float dragAcceleration = (-dragCoefficient * velocity) / mass;
        float totalAcceleration = gravity + dragAcceleration;

        velocity += totalAcceleration * dt;
        position += velocity * dt;

        if (position <= groundHeight)
        {
            position = groundHeight;
            velocity = -velocity * restitution;
            if (Mathf.Abs(velocity) < stopThreshold)
            {
                velocity = 0f;
                isActive = false;
            }
        }

        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        transform.position = new Vector3(
            transform.position.x,
            position,
            transform.position.z
        );
    }

    public void ResetSimulation()
    {
        ComputeMass();

        velocity = initialVelocity;
        position = initialHeight;
        isActive = true;

        transform.position = new Vector3(
            transform.position.x,
            initialHeight,
            transform.position.z
        );
    }

    private void ComputeMass()
    {
        Vector3 scale = transform.localScale;
        float volume = scale.x * scale.y * scale.z;
        mass = Mathf.Max(0.0001f, volume * massFactor);
    }
}