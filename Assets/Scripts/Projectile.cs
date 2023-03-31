using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float explosionRadiusMultipier = 0.8f;

    public static UnityAction OnExplosion;

    private Rigidbody rb;

    private void Start()
    {
        Ball.OnShot += StartFlying;
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Player" || collision.gameObject.tag == "Ground")
        {
            return;
        }

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, transform.localScale.magnitude * explosionRadiusMultipier);

        foreach (var collider in hitColliders)
        {
            if (collider.tag == "Obstacle")
            {
                collider.gameObject.GetComponent<Obstacle>().SimulateInfection();
            }
        }
        OnExplosion?.Invoke();
        Destroy(gameObject);
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
