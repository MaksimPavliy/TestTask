using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float explosionRadiusMultipier = 0.8f;

    public static UnityAction<float> OnExplosion;

    private Rigidbody rb;

    private void Start()
    {
        Ball.OnShot += StartFlying;
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        float delay = 0;
        Obstacle obstacle = null;

        if (collision.gameObject.tag == "Player" || collision.gameObject.tag == "Ground")
        {
            return;
        }

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, transform.localScale.magnitude * explosionRadiusMultipier);

        foreach (var collider in hitColliders)
        {
            if (collider.tag == "Obstacle")
            {
                obstacle = collider.gameObject.GetComponent<Obstacle>();
                obstacle.SimulateInfection();
            }
        }
        if (obstacle)
        {
            delay = obstacle.ParticleEmissionDelay;
        }
        OnExplosion?.Invoke(delay);
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Portal")
        {
            OnExplosion?.Invoke(0);
            Destroy(gameObject);
        }
    }

    private void StartFlying(Vector3 shotDirection)
    {
        Vector3 shotVelocity = projectileSpeed * shotDirection;
        shotVelocity.y = 0;
        rb.velocity = shotVelocity;
    }

    private void OnDestroy()
    {
        Ball.OnShot -= StartFlying;
    }
}
