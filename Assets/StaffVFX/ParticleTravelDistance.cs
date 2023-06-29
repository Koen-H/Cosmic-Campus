using UnityEngine;
using UnityEngine.Serialization;

public class ParticleTravelDistance : MonoBehaviour
{
    public float distance = 10f; // Desired travel distance
    private float duration = 2f; // Duration in seconds

    [FormerlySerializedAs("particleSystem")]
    public ParticleSystem particleSys;

    private void Start()
    {
        particleSys = GetComponent<ParticleSystem>();
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

        particleSys.Emit(emitParams, 1);
    }

    private Vector3 CalculateVelocity()
    {
        float velocityMagnitude = (distance * 6) / duration;
        Vector3 velocity = transform.forward * velocityMagnitude; // Adjust direction as needed

        return velocity;
    }
}
