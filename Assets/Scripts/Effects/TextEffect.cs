using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextEffect : MonoBehaviour
{
    TMP_Text textDisplay;
    public string finalText;
    public float delay;
    public float characterRevealDelay = 0.05f;
    public float scrambleDuration = 0.3f;

    private string chars = " ";
    // "!@#$%^&*?£"
    public AudioSource sound;
    public bool isRageText;
    public float timeOfRageText;

    void Start()
    {
        textDisplay = GetComponent<TMP_Text>();
        finalText = textDisplay.text;
        textDisplay.text = "";

        StartCoroutine(RevealText());
    }

    IEnumerator RevealText()
    {
        yield return new WaitForSecondsRealtime(delay);
        sound.Play();

        for (int i = 0; i < finalText.Length; i++)
        {
            float t = 0f;
            while (t < scrambleDuration)
            {
                t += Time.unscaledDeltaTime;
                char randomChar = chars[Random.Range(0, chars.Length)];
                string prefix = finalText.Substring(0, i); // revealed part
                textDisplay.text = prefix + randomChar;
                yield return null;
            }

            // lock in the correct character
            textDisplay.text = finalText.Substring(0, i + 1);
            yield return new WaitForSecondsRealtime(characterRevealDelay);
        }

        sound.Stop();
        if (isRageText)
            StartCoroutine(WaitToDissapear());
    }

    public void Dissapear()
    {
        StartCoroutine(RemoveCharacters());
    }

    IEnumerator RemoveCharacters()
    {
        for (int i = finalText.Length - 1; i >= 0; i--)
        {
            textDisplay.text = finalText.Substring(0, i);
            yield return new WaitForSecondsRealtime(characterRevealDelay);
        }

        gameObject.SetActive(false);
    }
    IEnumerator WaitToDissapear()
    {
        yield return new WaitForSecondsRealtime(timeOfRageText);
        Dissapear();
    }
}
