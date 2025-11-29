using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Egg : MonoBehaviour
{
    LevelManager levelManager;
    public GameObject particleDestroy;


    // Start is called before the first frame update
    void Start()
    {
        levelManager = FindObjectOfType<LevelManager>();
        int eggSpawn = PlayerPrefs.GetInt("SpawnEgg");
        int eggCollected = PlayerPrefs.GetInt("Egg_" + SceneManager.GetActiveScene().name);

        if (eggSpawn == 0 || eggCollected == 1) // spawn only if all levels have beaten dev time, or if it has already been collected
            Destroy(gameObject);                // else destroy itself         
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Interactable"))
        {
            Interactable interactable = other.GetComponent<Interactable>();
            interactable.hasNotHit = false;
            interactable.rb.useGravity = true;
            interactable.enabled = false;
            Collected();
        }
    }

    void Collected()
    {
        Instantiate(particleDestroy, transform.position, transform.rotation);

        int eggsCollected = PlayerPrefs.GetInt("EggsCollectedCount");
        eggsCollected += 1;
        PlayerPrefs.SetInt("EggsCollectedCount", eggsCollected);

        PlayerPrefs.SetInt("Egg_" + SceneManager.GetActiveScene().name, 1); // the egg of this scene is set to collected

        SoundManager.instance.PlaySound(SoundManager.instance.eggCollected);
        Destroy(gameObject);
    }
}
