using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShield : MonoBehaviour // USED FOR THE OBJECTS THAT THESE ENEMIES HOLD
{
    Interactable interactable;
    public GameObject particleHit;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Interactable"))
        {
            SoundManager.instance.PlaySound(SoundManager.instance.hitShield);
            ContactPoint contact = other.contacts[0];
            Vector3 position = contact.point;

            interactable = other.gameObject.GetComponent<Interactable>();
            interactable.rb.velocity = Vector3.zero;
            interactable.rb.useGravity = true;
            interactable.rb.AddExplosionForce(30, position, 2, 0, ForceMode.Impulse);
            Instantiate(particleHit, position, Quaternion.LookRotation(-contact.normal));
            interactable.enabled = false;
        }
    }
}
