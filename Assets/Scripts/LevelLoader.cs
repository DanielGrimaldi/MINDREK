using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public float scaleSpeed, hoverScaleMultiplier;
    private Vector3 originalScale, targetScale;
    public GameObject lockImage, numRoomText, crown;
    public AudioSource hoverSound;
    public TMP_Text record, devTimeText;
    bool playedSound;
    public int levelPlayed;
    public float devTime; // time of dev to beat
    public Color goldenColor, recordColor;
    Ray ray;
    RaycastHit hit;

    // Start is called before the first frame update
    void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;

        // check if level is unlocked
        levelPlayed = PlayerPrefs.GetInt(gameObject.name + "_Completed");
        
        if (levelPlayed == 1) // if room has been played
        {
            lockImage.SetActive(false);
            numRoomText.SetActive(true);
            record.gameObject.SetActive(true);

            string highScoreKey = "HighScore_" + gameObject.name;
            float savedBestTime = PlayerPrefs.GetFloat(highScoreKey, 0);

            if (savedBestTime == 0) // If there's no saved score yet
                record.text = "--.--";
            else
            {
                // set the record text to the best time of the room
                int seconds = (int)savedBestTime;
                int centiseconds = (int)((savedBestTime - seconds) * 1000);
                record.text = seconds.ToString("00") + "." + centiseconds.ToString("000");

                //check if beat dev time
                if (savedBestTime <= devTime) // if beat dev time, change color of level to golden
                {
                    numRoomText.GetComponent<TMP_Text>().color = goldenColor;
                    MenuManager menuManager = FindObjectOfType<MenuManager>();
                    record.color = recordColor;
                    crown.SetActive(true);
                    menuManager.devTimeLevelsBeat += 1; // say to the manager that this levels has beat dev time
                }

                // devTimetext gets the dev time and transforms it in time
                int secondss = (int)devTime;
                int centisecondss = (int)((devTime - secondss) * 1000);
                devTimeText.text = "DEV: " + secondss.ToString("00") + "." + centisecondss.ToString("000");
            }
        }
        if (levelPlayed == 0) // if room has NOT been played or completed
        {
            lockImage.SetActive(true);
            numRoomText.SetActive(false);
            record.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        CheckClick();
        CheckHover();
    }

    void CheckClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == gameObject)
            {
                if (!lockImage.activeInHierarchy)
                {
                    SceneManager.LoadScene(gameObject.name);
                }
            }
        }
    }

    void CheckHover()
    {
        if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == gameObject)
        {
            targetScale = originalScale * hoverScaleMultiplier;
            if (!playedSound)
            {
                hoverSound.Play();
                playedSound = true;
            }
            if (!lockImage.activeInHierarchy) // only if level in unlocked, show dev time to beat
                devTimeText.gameObject.SetActive(true);
        }
        else
        {
            targetScale = originalScale;
            playedSound = false;

            if (!lockImage.activeInHierarchy)
                devTimeText.gameObject.SetActive(false);
        }

        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * scaleSpeed);
    }
}
