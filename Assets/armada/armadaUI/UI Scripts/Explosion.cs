using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    
    public float timeSlowScale = 0.1f;
    public float explosionRadius = 5f; // The radius within which the explosion affects objects
    public float explosionForce = 700f; // The force of the explosion
    public LayerMask explosionLayerMask; // Layer mask to filter which objects are affected

    void Start()
    {
        ApplyExplosionForce();



        void ApplyExplosionForce()
        {
            // Find all colliders within the explosion radius
            Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, explosionLayerMask);

            foreach (Collider nearbyObject in colliders)
            {
                Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // Apply the explosion force
                    rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, 1f, ForceMode.Impulse);
                }
            }
            Time.timeScale = timeSlowScale;
            
            




        }
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
    }
}