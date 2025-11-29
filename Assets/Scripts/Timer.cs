using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    [SerializeField] public float timer = 0f;
    public bool isRunning = true;
    public TMP_Text timerText;

    void Update()
    {
        if (isRunning)
        {
            timer += Time.unscaledDeltaTime;

            int seconds = (int)timer;
            int centiseconds = (int)((timer - seconds) * 1000);
            timerText.text = seconds.ToString("00") + "." + centiseconds.ToString("000");
        }
    }

    public void StartTimer()
    {
        timer = 0f;
        isRunning = true;
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void ResetTimer()
    {
        timer = 0f;
    }
}
