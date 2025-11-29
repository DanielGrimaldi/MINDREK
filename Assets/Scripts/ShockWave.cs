using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShockWave : MonoBehaviour
{
    Smash smash;
    public GameObject secondShockWave;
    public Vector3 scaleGrow;
    public Material shockWaveMat;
    public float power, rbRotationSpeed, distortionDecreaseAmount = 0.05f;
    public float time;
    // Keep track of bodies we already launched
    private HashSet<Rigidbody> launchedBodies = new HashSet<Rigidbody>();
    
    // Start is called before the first frame update
    void Start()
    {
        smash = FindObjectOfType<Smash>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale += scaleGrow * Time.unscaledDeltaTime;
        secondShockWave.transform.localScale += scaleGrow * 1.2f * Time.unscaledDeltaTime;

        // time -= Time.unscaledDeltaTime;
        // if (time <= 0)
        // {
        //     Destroy(gameObject);
        // }

        float currentDistortion = shockWaveMat.GetFloat("_DistortionStrength");

        // Decrease the distortion strength, making sure it doesn't go below zero
        float newDistortion = Mathf.Max(currentDistortion - distortionDecreaseAmount * Time.deltaTime, 0f);

        // Apply the new distortion strength to the material
        shockWaveMat.SetFloat("_DistortionStrength", newDistortion);
        
        if(currentDistortion <= 0)
        {
            shockWaveMat.SetFloat("_DistortionStrength", 0.03f);
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;

        if (rb != null && !launchedBodies.Contains(rb) && !other.CompareTag("Interactable"))
        {
            // if (other.gameObject.CompareTag("Interactable"))
            //     rb.AddForce(Vector3.up * smash.power, ForceMode.Impulse);
            // else
            //     rb.AddExplosionForce(smash.power * 10, transform.position, 20f, 5, ForceMode.Impulse);
            rb.AddForce(Vector3.up * power, ForceMode.Impulse);

            // give a slight rotation to all object after smash
            Vector3 randomAngularVelocity = new Vector3(
            Random.Range(0, rbRotationSpeed),
            Random.Range(0, rbRotationSpeed),
            Random.Range(0, rbRotationSpeed));

            rb.angularVelocity = randomAngularVelocity;
            launchedBodies.Add(rb);
        }

        
    }
}
