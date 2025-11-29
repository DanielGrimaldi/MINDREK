using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    public AudioSource smash, hitLimb, enemyDie, throwLight, throwMedium, throwHeavy, hitShield, eggCollected;
    public AudioSource slowMotionStart, slowMoAmbience, roomAmbience; // slowMoAmbience is the sound backround when in slowMotion
    public AudioSource feather, brick, anvil, skull;


    // Add more clips as needed

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void ChangeAmbienceVolume(float value)
    {
        slowMoAmbience.volume = Mathf.Lerp(slowMoAmbience.volume, value, 1 * Time.unscaledDeltaTime);
    }
    public void ChangeRoomVolume(float value)
    {
        roomAmbience.volume = Mathf.Lerp(roomAmbience.volume, value, 1 * Time.unscaledDeltaTime);
    }

    public void PlaySound(AudioSource clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null) return;

        clip.volume = volume;
        clip.pitch = pitch;
        clip.Play();
    }
}
