using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SuicideObject : MonoBehaviour
{
    Interactable interactable;
    LevelManager levelManager;
    Smash smash;
    PlayerCamera playerCamera;
    public GameObject stressEndsHereText, demoText, pointer, instructions, timeGot, blood;
    public GameObject heartBeat, table, plane;
    bool b, m, shrinkObjects;

    // Start is called before the first frame update
    void Start()
    {
        levelManager = FindObjectOfType<LevelManager>();
        playerCamera = FindObjectOfType<PlayerCamera>();
        smash = FindObjectOfType<Smash>();
        interactable = GetComponent<Interactable>();

        interactable.isSuicide = true; // interactable can't be dragged nor rotated
        smash.rbRotationSpeed = 0f;

        StartCoroutine(Wait());
    }

    // Update is called once per frame
    void Update()
    {
        if (interactable.isShooted)
        {
            if (!b)
                StartCoroutine(Shooted());
                
            b = true;
            m = true;
            playerCamera.mouseSensitivity = 0;
        }
        if (shrinkObjects)
        {
            table.transform.localScale = Vector3.Lerp(table.transform.localScale, Vector3.zero, 14f * Time.unscaledDeltaTime);
            plane.transform.localScale = Vector3.Lerp(plane.transform.localScale, Vector3.zero, 14f * Time.unscaledDeltaTime);
            // Destroy when small
            if (table.transform.localScale.magnitude < 0.01f)
                Destroy(table.gameObject);
            if (plane.transform.localScale.magnitude < 0.01f)
                Destroy(plane.gameObject);
        }
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(0.2f);
        interactable.rb.mass = 1f;
        smash.slowmoDuration = 1000f;
    }

    IEnumerator Shooted()
    {
        yield return new WaitForSecondsRealtime(0.02f);
        interactable.rb.velocity *= 0f;
        //interactable.transform.LookAt(playerCamera.transform);
        if (Time.timeScale > 0)
            interactable.rb.AddForce(transform.right * 2000f / Time.timeScale); // throw the object towards the player
        interactable.rb.useGravity = false;
        heartBeat.SetActive(false);
        pointer.SetActive(false);

        yield return new WaitForSecondsRealtime(0.2f);
        blood.SetActive(true);
        playerCamera.depthOfField.focusDistance.value = 0.8f;

        yield return new WaitForSecondsRealtime(2f);
        stressEndsHereText.SetActive(true);

        yield return new WaitForSecondsRealtime(2f);
        stressEndsHereText.SetActive(true);

        yield return new WaitForSecondsRealtime(4f);
        demoText.SetActive(true);

        yield return new WaitForSecondsRealtime(1f);
        levelManager.resetScene = true;
        levelManager.startGame = false;
        levelManager.wonGame = true;
        //levelManager.detectChildOfAllObjects();
        shrinkObjects = true;

        

        yield return new WaitForSecondsRealtime(9f);
        SceneManager.LoadScene("Menu");
    }
}
