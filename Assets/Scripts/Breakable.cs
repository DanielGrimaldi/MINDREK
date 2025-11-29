using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breakable : MonoBehaviour
{
    Interactable interactable;
    public GameObject fractured, instantiated;
    public float explosionForce;
    bool m;
    // Start is called before the first frame update
    void Start()
    {
        interactable = GetComponent<Interactable>();
    }

    // Update is called once per frame
    void Update()
    {
        if (interactable != null) // works only if script is attached to interactable
        {
            if (!interactable.hasNotHit) // if object hit the enemy
            {
                if(!m)
                    instantiated = Instantiate(fractured, transform.position, transform.rotation);
                m = true;
                // Apply explosion force to each rigidbody
                foreach (Transform shard in instantiated.GetComponentsInChildren<Transform>())
                {
                    shard.SetParent(GameObject.Find("ALL OBJECTS").transform);
                    // shard.localScale = Vector3.Lerp(shard.localScale, Vector3.zero, 14f * Time.unscaledDeltaTime);

                    // // Destroy when small
                    // if (shard.localScale.magnitude < 0.01f)
                    //     Destroy(shard.gameObject);
                    //shard.GetComponent<Rigidbody>().AddExplosionForce(explosionForce, transform.position, 2, 0, ForceMode.Impulse);
                    Destroy(shard.gameObject, 4f);
                }

                    //interactable.rb.AddExplosionForce(explosionForce, transform.position, 2, 0, ForceMode.Impulse);
                    // Vector3 velocity = interactable.rb.velocity;
                    // rb.velocity = velocity;                      
                
                StartCoroutine(Wait());
            }
        }
    }
    
    void OnCollisionEnter(Collision collision) // works only for window glass
    {
        if(collision.gameObject.CompareTag("Interactable"))
        {
            GameObject glass = Instantiate(fractured, transform.position, transform.rotation);
            glass.transform.SetParent(GameObject.Find("ALL OBJECTS").transform);
            Destroy(gameObject);
        }
    }

    IEnumerator Wait()
    {
        yield return new WaitForSecondsRealtime(0.02f);
        interactable.gameObject.SetActive(false);
        Destroy(instantiated, 4f);
    }
}
