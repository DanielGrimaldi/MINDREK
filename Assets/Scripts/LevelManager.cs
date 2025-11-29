using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    Smash smash;
    PlayerCamera playerCamera;
    GlitchEffect glitchEffect;
    public RawImage stripesMaterial;
    public Camera camForText;
    int numOfInteractables, numOfEnemies;
    [HideInInspector] public bool startGame, preGame, resetScene, lostGame, wonGame, isInSlowMo, glitch, fogReturn;
    public float lerpSpeed, fogSpeed, highScoreTimer;
    float velocity = 0f, duration = 0.4f, elapsed = 0;
    GameObject[] interactables, enemies;
    public GameObject timer, levelText, instructions, newBestText, yourTimeText, enemyKillFlash, eggFlash, table, backroundImage, startGameObjectsShow, allObjects, shootMedium, shootHeavy;
    public TMP_Text timeGot, bestTimeGot;
    Vector3 originalTablePos;
    List<Transform> childrenOfAllObjects = new List<Transform>();
    public ParticleSystem fogParticles, tableDust;
    public string levelName;

    // Start is called before the first frame update
    void Start()
    {
        numOfInteractables = GameObject.FindGameObjectsWithTag("Interactable").Length;
        numOfEnemies = GameObject.FindGameObjectsWithTag("Enemy").Length;

        interactables = GameObject.FindGameObjectsWithTag("Interactable");

        // create a list of all the enemy objects found in scene, deactivate them at start of game
        enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
            enemy.SetActive(false);

        PlayerPrefs.SetInt(SceneManager.GetActiveScene().name + "_Completed", 1); // to mark this level as played
        PlayerPrefs.Save();

        smash = FindObjectOfType<Smash>();
        playerCamera = FindObjectOfType<PlayerCamera>();
        glitchEffect = FindObjectOfType<GlitchEffect>();

        preGame = true;
        levelName = SceneManager.GetActiveScene().name;
        //PlayerPrefs.SetFloat("HighScore_" + levelName, 5000);
        RenderSettings.fogStartDistance = 0.1f;
        RenderSettings.fogEndDistance = 0.11f;

        startGameObjectsShow.SetActive(false);
        originalTablePos = table.transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        // at the start of the scene, before smashing
        if (preGame)
        {
            RenderSettings.fogStartDistance = Mathf.SmoothDamp(RenderSettings.fogStartDistance, 4f, ref velocity, lerpSpeed / 2.5f);
            RenderSettings.fogEndDistance = RenderSettings.fogStartDistance + 1f;

        }
        // if table is smashed, game starts
        if (startGame)
        {
            RenderSettings.fogStartDistance = Mathf.Lerp(RenderSettings.fogStartDistance, 12, lerpSpeed * Time.unscaledDeltaTime);
            RenderSettings.fogEndDistance = RenderSettings.fogStartDistance + 4f;

            float newScale = Mathf.Min(fogParticles.transform.localScale.x + fogSpeed / 1.3f * Time.unscaledDeltaTime, 4);
            fogParticles.transform.localScale = Vector3.one * newScale;
        }

        // reset the fog when you lose/ win game
        if (resetScene)
        {
            lerpSpeed = 0.3f;

            if (!wonGame)  //works only when player loses
            {
                RenderSettings.fogStartDistance = Mathf.SmoothDamp(RenderSettings.fogStartDistance, 0.01f, ref velocity, lerpSpeed / 6f, Mathf.Infinity, Time.unscaledDeltaTime);
                RenderSettings.fogEndDistance = Mathf.SmoothDamp(RenderSettings.fogEndDistance, 0.012f, ref velocity, lerpSpeed / 5.7f, Mathf.Infinity, Time.unscaledDeltaTime);
            }
            // make the fog particle move towards player only when player wins
            if (wonGame)
            {
                float currentScale = fogParticles.transform.localScale.x;
                float targetScale = currentScale - fogSpeed * 3f * Time.unscaledDeltaTime;
                targetScale = Mathf.Clamp(targetScale, 1, 17);
                float newScalee = Mathf.Lerp(currentScale, targetScale, 0.4f); // 0.5f is the Lerp "smoothness"
                // Apply scale uniformly
                fogParticles.transform.localScale = Vector3.one * newScalee;

                RenderSettings.fogStartDistance = Mathf.SmoothDamp(RenderSettings.fogStartDistance, 4f, ref velocity, lerpSpeed / 0f, Mathf.Infinity, Time.unscaledDeltaTime);
                RenderSettings.fogEndDistance = Mathf.SmoothDamp(RenderSettings.fogEndDistance, 5f, ref velocity, lerpSpeed / 0f, Mathf.Infinity, Time.unscaledDeltaTime);

                // makes the object shrink
                for (int i = childrenOfAllObjects.Count - 1; i >= 0; i--)
                {
                    Transform child = childrenOfAllObjects[i];
                    if (child == null)
                    {
                        childrenOfAllObjects.RemoveAt(i);
                        continue;
                    }

                    // Shrink smoothly
                    child.localScale = Vector3.Lerp(child.localScale, Vector3.zero, 14f * Time.unscaledDeltaTime);

                    // Destroy when small
                    if (child.localScale.magnitude < 0.01f)
                    {
                        Destroy(child.gameObject);
                        childrenOfAllObjects.RemoveAt(i);
                    }
                }
            }      
        }

        if (glitch)
        {
            if (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float v = Mathf.Sin(elapsed / duration * Mathf.PI) * 3f;
                glitchEffect.glitchIntensity = v;
            }
            else
            {
                glitchEffect.glitchIntensity = 0;
            }

            smash.effects = false;
        }

        stripesMaterial.material.SetFloat("_UnscaledTime", Time.unscaledTime); // used to make stripes move on low Timescale

        // set the FOV of the camera that sees only 3d Text to the FOV of the original camera
        camForText.fieldOfView = Camera.main.fieldOfView / 1.1f;

        // make the backround image shrink when player loses game
        if (lostGame)
        {
            if (backroundImage.transform.localScale.x > 0f)
                backroundImage.transform.localScale -= new Vector3(1, 1, 1) * Time.unscaledDeltaTime;
        }

        ShakeTable();

        // restart game
        if (Input.GetKeyDown(KeyCode.Q))
            ReloadScene();
        // quit game
        if (Input.GetKeyDown(KeyCode.Escape))
            QuitLevel();
        if (Input.GetKeyDown(KeyCode.Return) && wonGame)
            NextScene();
    }

    public void StartGame()
    {
        startGame = true;
        preGame = false;
        StartCoroutine(InSlowMo()); // everything is officialy in slowmotion
        timer.SetActive(true);
        startGameObjectsShow.SetActive(true);

        // deactivate the level texts, removing each character
        foreach (Transform text in levelText.transform)
        {
            TextEffect script = text.GetComponent<TextEffect>();
            if (script != null)
                script.Dissapear();
        }

        foreach (GameObject enemy in enemies)
            enemy.SetActive(true);
    }

    public void WinGame()
    {
        wonGame = true;
        startGame = false;
        isInSlowMo = false;
        timer.GetComponent<Timer>().StopTimer();
        smash.StopSlowMoCoroutine();
        smash.effects = false;
        Time.timeScale = 1;
        Time.fixedDeltaTime = 0.01f * Time.timeScale;
        StartCoroutine(WonGameReset());
    }
    public void LoseGame()
    {
        lostGame = true;
        startGame = false;
        isInSlowMo = false;
        timer.GetComponent<Timer>().StopTimer();
        smash.StopSlowMoCoroutine();
        smash.effects = false;
        Time.timeScale = 0;
        Time.fixedDeltaTime = 0.01f * Time.timeScale;
        StartCoroutine(LostGameReset());
    }
    IEnumerator WonGameReset()
    {
        smash.canStopSlowMo = false;
        playerCamera.canHover = false;
        playerCamera.depthOfField.focusDistance.value = 0.8f;
        playerCamera.Zoom(Camera.main.fieldOfView, 0.7f);
        Camera.main.fieldOfView = 67f; // gives effect
        smash.bloom.intensity.value = 2.8f;
        CheckHighScore();
        timer.SetActive(false);

        // Smoothly reduce time scale to 0
        while (Time.timeScale > 0 + 0.016f)
        {
            Time.timeScale = Mathf.Lerp(Time.timeScale, 0.014f, Time.unscaledDeltaTime * 2.3f);
            Time.fixedDeltaTime = 0.01f * Time.timeScale;
            yield return null;
        }

        Time.timeScale = 0.014f; // Snap exactly
        Time.fixedDeltaTime = 0.01f * Time.timeScale;
        instructions.SetActive(true);
        StartCoroutine(ResetNormalSpeed());

        yield return new WaitForSecondsRealtime(0.6f);
        velocity = 0;
        resetScene = true;

        yield return new WaitForSecondsRealtime(0.3f);
        foreach (GameObject interactable in interactables)
            interactable.transform.SetParent(allObjects.transform);
        foreach (GameObject enemy in enemies)
            enemy.transform.SetParent(allObjects.transform);
        for (int i = allObjects.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = allObjects.transform.GetChild(i);
            child.SetParent(null);
            child.gameObject.isStatic = false;
            childrenOfAllObjects.Add(child);
        }   
    }

    IEnumerator LostGameReset()
    {
        smash.canStopSlowMo = false;
        playerCamera.canMove = false;
        playerCamera.canHover = false;
        playerCamera.depthOfField.focusDistance.value = 0.8f;
        glitch = true;

        yield return new WaitForSecondsRealtime(1);
        timer.SetActive(false);
        velocity = 0;
        resetScene = true;
        tableDust.gameObject.SetActive(false);

        yield return new WaitForSecondsRealtime(1);
        ReloadScene();
    }

    void ShakeTable()
	{
		Collider[] hits = Physics.OverlapSphere(playerCamera.lookPos.transform.position, 2.7f);

        bool enemyFound = false;

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                enemyFound = true;
                break; // stop looping, we only care if at least one exists
            }
        }

        if (enemyFound)
            table.transform.localPosition = originalTablePos + Random.insideUnitSphere * 0.015f;
        if((!enemyFound || lostGame || wonGame) && table != null)
            table.transform.localPosition = originalTablePos; // reset if no enemies nearby
    }

    public void ReduceNumOfInteractables()
    {
        numOfInteractables -= 1;

        if (numOfInteractables == 0)
        {
            StopSlowMo();
        }
    }

    public void ReduceNumOfEnemies()
    {
        numOfEnemies -= 1;

        if (numOfEnemies == 0)
        {
            WinGame();
        }
    }

    void CheckHighScore()
    {
        Timer t = timer.GetComponent<Timer>();
        float currentTime = t.timer;

        timeGot.text = t.timerText.text;
        timeGot.gameObject.SetActive(true); // Show 3D timer in front of player

        // Use unique key per level
        string highScoreKey = "HighScore_" + levelName;

        // Load saved best time
        float savedBestTime = PlayerPrefs.GetFloat(highScoreKey, 0);

        // If there's no saved score yet or current is better
        if (savedBestTime == 0 || currentTime < savedBestTime)
        {
            PlayerPrefs.SetFloat(highScoreKey, currentTime); // Save new high score
            PlayerPrefs.Save(); // Ensure it writes to disk
            bestTimeGot.text = timeGot.text; // Show the new best

            if (currentTime < savedBestTime) // only if you get a better time than before, show the "new best" text
            {
                newBestText.SetActive(true);
                yourTimeText.SetActive(false);
            }
        }
        else
        {
            // Show the previously saved best time
            int seconds = (int)savedBestTime;
            int centiseconds = (int)((savedBestTime - seconds) * 1000);
            bestTimeGot.text = seconds.ToString("00") + "." + centiseconds.ToString("000");
        }
    }

    void StopSlowMo()
    {
        smash.canStopSlowMo = false;
        smash.StopSlowMotion();
    }

    IEnumerator InSlowMo()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        isInSlowMo = true;
    }

    IEnumerator ResetNormalSpeed() // after winning, reset the slowMotion to Timescale = 1
    {
        yield return new WaitForSecondsRealtime(0.02f);
        // Smoothly reset time scale to 1
        while (Time.timeScale < 1 - 0.01f)
        {
            Time.timeScale = Mathf.Lerp(Time.timeScale, 1, Time.unscaledDeltaTime * 6f);
            Time.fixedDeltaTime = 0.01f * Time.timeScale;
            yield return null;
        }

        Time.timeScale = 1; // Snap exactly
        Time.fixedDeltaTime = 0.01f * Time.timeScale;
    }

    public void ReloadScene()
    {
        Time.timeScale = 1;
        Time.fixedDeltaTime = 0.01f * Time.timeScale;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void NextScene()
    {
        Time.timeScale = 1;
        Time.fixedDeltaTime = 0.01f * Time.timeScale;

        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentIndex + 1);
    }
    
    public void QuitLevel()
    {
        Time.timeScale = 1;
        Time.fixedDeltaTime = 0.01f * Time.timeScale;
        //Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene("Menu");
    }
}
