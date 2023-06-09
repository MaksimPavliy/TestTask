using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [SerializeField] private Material projectileMaterial;

    private MeshRenderer meshRenderer;
    private new ParticleSystem particleSystem;
    private BoxCollider boxCollider;
    private float particleEmissionDelay = 0.8f;

    public float ParticleEmissionDelay => particleEmissionDelay;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        particleSystem = GetComponentInChildren<ParticleSystem>();
        boxCollider = GetComponent<BoxCollider>();
    }

    public void SimulateInfection()
    {
        boxCollider.enabled = false;
        meshRenderer.material.color = projectileMaterial.color;

        StartCoroutine(DestroyObstacle());
    }

    private IEnumerator DestroyObstacle()
    { 
        yield return new WaitForSeconds(particleEmissionDelay);

        meshRenderer.enabled = false;
        particleSystem.Play();

        while (particleSystem.isPlaying)
        {
            yield return null;
        }
        Destroy(gameObject);
    }
}
