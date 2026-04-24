using UnityEngine;

public class Particle : MonoBehaviour
{
    [Header("Initial Conditions")] //Condiciones iniciales de la partícula
    public Vector3 initialPosition = new Vector3(0, 10, 0); // Posición inicial
    public Vector3 initialVelocity = Vector3.zero; // Velocidad inicial

    [Header("Physical Parameters")]
    public Vector3 gravity = new Vector3(0, -9.81f, 0); // Gravedad que actua sobre la simulación
    [Min(0f)] public float dragCoefficient = 0.1f; // Resistencia del aire
    [Min(0.0001f)] public float massFactor = 1f; // Masa de la partícula
    [Range(0f, 1f)] public float restitution = 0.8f; // Coeficiente de elasticidad
    [Range(0f, 1f)] public float friction = 0.2f; // Coeficiente de fricción

    [Header("Collision")] // Para hacr la colición con cierta máscara
    public LayerMask collisionMask;
    [Min(0f)] public float skin = 0.001f;

    private Vector3 position; // La posición
    private Vector3 velocity; // La velocidad
    private Vector3 accumulatedForce; // Para el futuro
    private float mass; //Masa
    private float radius; //Radio

    private void OnEnable() { ParticleWorld.Register(this); } // Es como un simulation manager de la partícula
    private void OnDisable() { ParticleWorld.Unregister(this); }

    private void OnValidate()
    {
        ComputeMass();
        ComputeRadius();
    }

    private void Start() 
    {
        ResetParticle();
    }

    public void Integrate(float dt) // Integra con respecto al tiempo, sobre el tiempo se suman las fuerzas
    {
        Vector3 dragForce = -dragCoefficient * velocity; // La resistencua del aire afecta la velocidad de la partícula
        Vector3 acceleration = gravity + (accumulatedForce + dragForce) / mass; // Suma de todas las aceleraciones que actúan sobre el cuerpo

        velocity += acceleration * dt; // Aceleración  traves del tiempo
        accumulatedForce = Vector3.zero;  // Cuado colisionan le fuerza acumulada se vuelve cero para que no agarren más velocidad
    }

    public void Move(float dt) // Para el movimiento si coliiona con algo
    {
        Vector3 displacement = velocity * dt; // Desplazamiento
        float distance = displacement.magnitude; // Magnitud de la vlocidad

        if (distance <= Mathf.Epsilon) return; // Si la distancia es menor o igual a un valor muy pequeño ya se queda quieto le dice uando se tiene que detener

        Vector3 direction = displacement / distance; // Dirección del desplazamiento, divide el desplzamiento en la distancia
        Vector3 castOrigin = position - direction * skin; // Crear una colisión y un poco más
        float castDist = distance + skin; // Cuánto se extiende la colisión

        if (Physics.SphereCast(castOrigin, radius, direction,
                               out RaycastHit hit, castDist, collisionMask, // Guarda qué golpea
                               QueryTriggerInteraction.Ignore))
        {
            float travel = Mathf.Max(0f, hit.distance - skin); // Dirección de viaje de lo que se golpeó
            position += direction * travel;
            ReflectVelocity(hit.normal); // Refleja la velocidad

            Debug.DrawRay(hit.point, hit.normal, Color.green);
        }
        else
        {
            position += displacement;
        }
    }

    public void UpdateVisuals()
    {
        transform.position = position;
    }

    public void ResetParticle() // Reinicia la partícula
    {
        ComputeMass();
        ComputeRadius();

        position = initialPosition;
        velocity = initialVelocity;
        accumulatedForce = Vector3.zero;

        transform.position = position;
    }

    private void ComputeMass() //
    {
        Vector3 scale = transform.localScale;
        float volume = scale.x * scale.y * scale.z;
        mass = Mathf.Max(0.0001f, volume * massFactor);
    }

    private void ComputeRadius()
    {
        Vector3 scale = transform.localScale;
        float maxScale = Mathf.Max(scale.x, scale.y, scale.z);
        radius = maxScale * 0.5f;
    }

    public void ReflectVelocity(Vector3 normal) // Velocidad reflejada
    {
        Vector3 vNormal = Vector3.Dot(velocity, normal) * normal;
        Vector3 vTangent = velocity - vNormal;
        velocity = vTangent * (1f - friction) - vNormal * restitution;
    }

    public void AddForce(Vector3 force) => accumulatedForce += force;
    public void AddImpulse(Vector3 impulse) => velocity += impulse / mass; // A la velocidad se le agrega un impulso

    public Vector3 Position
    {
        get => position;
        set
        {
            position = value;
            transform.position = value;
        }
    }

    public Vector3 Velocity
    {
        get => velocity;
        set => velocity = value;
    }

    public float Mass => mass;

    public float Radius => radius;

    public float Restitution => restitution;
}