using UnityEngine;

public class ParticleTravelDistance : MonoBehaviour
{
    public float distance = 10f; // Desired travel distance
    private float duration = 2f; // Duration in seconds

    public ParticleSystem particleSystem;

    private void Start()
    {
        particleSystem = GetComponent<ParticleSystem>();
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    EmitParticles();
        //}
        EmitParticles();
    }

    private void EmitParticles()
    {
        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
        emitParams.velocity = CalculateVelocity();

        particleSystem.Emit(emitParams, 1);
    }

    private Vector3 CalculateVelocity()
    {
        float velocityMagnitude = (distance * 6) / duration;
        Vector3 velocity = transform.forward * velocityMagnitude; // Adjust direction as needed

        return velocity;
    }
}
