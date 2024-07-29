using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public float explosionForce = 500f;
    public float explosionRadius = 5f;
    public ParticleSystem explosionEffect;
    public float timeSlowDuration = 2f; // Duration of the slow motion effect
    public float timeSlowScale = 0.1f; // Scale of the time slowdown

    void Start()
    {
        // Play the explosion effect
        ParticleSystem effect = Instantiate(explosionEffect, transform.position, transform.rotation);
        effect.Play();

        // Apply force to nearby objects
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider nearbyObject in colliders)
        {
            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }
        }

        // Start the time slowdown coroutine
        StartCoroutine(SlowDownTime());

      
    }

    IEnumerator SlowDownTime()
    {
        Time.timeScale = timeSlowScale;
        yield return new WaitForSecondsRealtime(timeSlowDuration);
        Time.timeScale = 1f;
    }
}