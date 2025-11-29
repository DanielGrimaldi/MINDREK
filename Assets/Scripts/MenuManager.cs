using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour
{
    public GameObject mainScreen, levels, playerStats, arrowBack, itchIoButton, levelsTextObj, enemyShieldDummy;
    public TMP_Text enemyKillCountText, smashTableCountText, eggsCollectedCountText;
    public TMP_Text sensitivityValue;
    public int enemyKillCount, smashTableCount, eggsCollectedCount;
    public int devTimeLevelsBeat;
    public GameObject findTheEggsText;
    [SerializeField] Slider sensitivity;

    void Awake()
    {
        enemyKillCount = PlayerPrefs.GetInt("EnemyKillCount");
        smashTableCount = PlayerPrefs.GetInt("SmashTableCount");
        eggsCollectedCount = PlayerPrefs.GetInt("EggsCollectedCount");

        if (PlayerPrefs.GetFloat("mouseSensitivity") == 0) // if player starts game for first time, mak the sensitivity 100
            PlayerPrefs.SetFloat("mouseSensitivity", 100);
        sensitivity.value = PlayerPrefs.GetFloat("mouseSensitivity");
        sensitivityValue.text = ((int)sensitivity.value).ToString();

        StartCoroutine(WarmupRig(enemyShieldDummy));       
        Cursor.lockState = CursorLockMode.None;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GoBackToMainScreen();
        }
    }

    public void Play()
    {
        mainScreen.SetActive(false);
        levels.SetActive(true);
        arrowBack.SetActive(true);
        itchIoButton.SetActive(false);
        levelsTextObj.SetActive(true);
    }
    public void Stats()
    {
        mainScreen.SetActive(false);
        playerStats.SetActive(true);
        arrowBack.SetActive(true);
        itchIoButton.SetActive(false);

        smashTableCountText.text = smashTableCount.ToString();
        enemyKillCountText.text = enemyKillCount.ToString();
        eggsCollectedCountText.text = eggsCollectedCount.ToString() + " / 3";
    }
    public void Quit()
    {
        PlayerPrefs.DeleteAll();
        Application.Quit();
    }

    public void Tutorial()
    {
        SceneManager.LoadScene("Tutorial");
    }

    public void GoBackToMainScreen()
    {
        levels.SetActive(false);
        playerStats.SetActive(false);
        mainScreen.SetActive(true);
        arrowBack.SetActive(false);
        itchIoButton.SetActive(true);
    }
    public void ItchIO()
    {
        Application.OpenURL("https://danielgrimaldi.itch.io/mindrek");
    }

    public void SetMouseSensitivity(Slider sensitivity)
    {
        float mouseSensitivity = sensitivity.value;
        PlayerPrefs.SetFloat("mouseSensitivity", mouseSensitivity);
        sensitivityValue.text = ((int)mouseSensitivity).ToString();
    }

    // To spawn an enemy with shield
    IEnumerator WarmupRig(GameObject enemyPrefab) // this so the level doesn't freeze midgame
    {
        GameObject dummy = Instantiate(enemyPrefab);
        dummy.SetActive(true);
        levels.SetActive(true);

        // Wait one frame to force Unity to build the rig jobs
        yield return null;

        Destroy(dummy);
        levels.SetActive(false);

        if (devTimeLevelsBeat >= 7) // if all levels are golden
        {
            PlayerPrefs.SetInt("SpawnEgg", 1); // spawn egg
            
            if (eggsCollectedCount < 3) // if eggs are still not collected all, show text
                findTheEggsText.SetActive(true);
        }
    }
}
