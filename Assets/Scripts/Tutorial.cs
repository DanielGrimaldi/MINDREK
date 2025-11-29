using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    LevelManager levelManager;
    public GameObject holdSpaceBarText, allInstructions;
    public GameObject cube, sphere;

    // Start is called before the first frame update
    void Awake()
    {
        levelManager = FindObjectOfType<LevelManager>();
        StartCoroutine(StartScene());
        int num = Random.Range(0, 10);
        if (num < 9)
        {
            cube.SetActive(true);
            cube.tag = "Interactable";
        }        
        if (num == 9)
        {
            sphere.SetActive(true);
            sphere.tag = "Interactable";
        }  
            
    }

    // Update is called once per frame
    void Update()
    {
        if (levelManager.startGame)
        {
            holdSpaceBarText.SetActive(false);
            allInstructions.SetActive(true);
        }
        if (levelManager.wonGame)
        {
            allInstructions.SetActive(false);
        }
        if (levelManager.lostGame)
        {
            allInstructions.SetActive(false);
        }
    }

    IEnumerator StartScene()
    {
        yield return new WaitForSeconds(1.3f);
        holdSpaceBarText.SetActive(true);
    }
}
