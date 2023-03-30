using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [SerializeField] private Material projectileMaterial;

    private MeshRenderer meshRenderer;
    private new ParticleSystem particleSystem;
    private BoxCollider boxCollider;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        particleSystem = GetComponentInChildren<ParticleSystem>();
        boxCollider = GetComponent<BoxCollider>();
    }

    public void SimulateInfection()
    {
        meshRenderer.material.color = projectileMaterial.color;
        boxCollider.isTrigger = true;

        StartCoroutine(DestroyObstacle());
    }

    private IEnumerator DestroyObstacle()
    { 
        yield return new WaitForSeconds(0.8f);

        meshRenderer.enabled = false;
        particleSystem.Play();

        while (particleSystem.isPlaying)
        {
            yield return null;
        }
        Destroy(gameObject);
    }
}
