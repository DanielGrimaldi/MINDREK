using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

public class Smash : MonoBehaviour
{
    PlayerCamera playerCamera;
    LevelManager levelManager;
    public float radius, power;
    public float rbRotationSpeed;
    bool isSlowing, isCharging;
    public bool canCharge = true, canStopSlowMo = true, effects;
    public float slowTimeScale = 0.1f;
    public float slowmoDuration = 5f; // Real-time seconds
    public float transitionSpeed = 2f; // Speed of smoothing
    public float animationTime, lensAnimationTime;
    float targetLens, targetBloom;
    public GameObject table, shockWave;
    public Slider chargeSmashSlider;
    Coroutine slowMotionCoroutine, stopSlowMo;

    public PostProcessVolume postProcessVolume;
    private Vignette vignette;
    private ChromaticAberration chromatic;
    [SerializeField] public Bloom bloom; // used anche in levelManager
    private LensDistortion lensDistortion;

    // Start is called before the first frame update
    void Start()
    {
        playerCamera = FindObjectOfType<PlayerCamera>();
        levelManager = FindObjectOfType<LevelManager>();

        postProcessVolume.profile.TryGetSettings(out vignette);
        postProcessVolume.profile.TryGetSettings(out chromatic);
        postProcessVolume.profile.TryGetSettings(out bloom);
        postProcessVolume.profile.TryGetSettings(out lensDistortion);

        effects = false;
        vignette.intensity.value = 0.4f;
        chromatic.intensity.value = 0.13f;

        transitionSpeed = 8f;
        targetBloom = 1.5f;
        targetLens = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Space) && canCharge) // charging the smash
        {
            chargeSmashSlider.gameObject.SetActive(true);
            chargeSmashSlider.value += Time.unscaledDeltaTime;
            Camera.main.fieldOfView += Time.deltaTime * 60f;
            playerCamera.Zoom(Camera.main.fieldOfView, 0.2f);
            lensDistortion.intensity.value -= Time.deltaTime * 250f;

            if (chargeSmashSlider.value >= chargeSmashSlider.maxValue) // SMASH
            {
                chargeSmashSlider.gameObject.SetActive(false);
                SmashTable();
                canCharge = false;
                effects = true;
            }

            isCharging = true;
        }

        if (Input.GetKeyUp(KeyCode.Space)) // release the charging
        {
            isCharging = false;
            //playerCamera.Zoom(60, 0.2f);
        }

        if (!isCharging) // decrease slider value after release
        {
            if (chargeSmashSlider.value > 0)
            {
                chargeSmashSlider.value -= 1.4f * Time.unscaledDeltaTime;
                chargeSmashSlider.value = Mathf.Clamp(chargeSmashSlider.value, 0, chargeSmashSlider.maxValue);
            }
            else
            {
                chargeSmashSlider.gameObject.SetActive(false);
            }
        }

        if (effects)
        {
            vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, 0.5f, animationTime * Time.unscaledDeltaTime);
            chromatic.intensity.value = Mathf.Lerp(chromatic.intensity.value, 0.6f, animationTime * Time.unscaledDeltaTime);
            SoundManager.instance.ChangeAmbienceVolume(1);
            SoundManager.instance.ChangeRoomVolume(0.2f);
        }
        else
        {
            vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, 0.4f, animationTime * Time.unscaledDeltaTime);
            chromatic.intensity.value = Mathf.Lerp(chromatic.intensity.value, 0.14f, animationTime * Time.unscaledDeltaTime);
            SoundManager.instance.ChangeAmbienceVolume(0);
            SoundManager.instance.ChangeRoomVolume(1);
        }

        lensDistortion.intensity.value = Mathf.Lerp(lensDistortion.intensity.value, targetLens, lensAnimationTime * Time.unscaledDeltaTime);
        bloom.intensity.value = Mathf.Lerp(bloom.intensity.value, targetBloom, 0.5f * Time.unscaledDeltaTime);
    }

    void SmashTable()
    {
        Vector3 explosionPos = table.transform.position;
        Collider[] colliders = Physics.OverlapSphere(explosionPos, radius);
        lensAnimationTime = 0.8f;

        int smashedTableCount = PlayerPrefs.GetInt("SmashTableCount");
        smashedTableCount += 1;
        PlayerPrefs.SetInt("SmashTableCount", smashedTableCount);

        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.AddForce(Vector3.up * power, ForceMode.Impulse);

                // Pick a random spin direction
                Vector3 randomDir = Random.onUnitSphere;

                // Pick a random spin speed
                float spinSpeed = Random.Range(-rbRotationSpeed, rbRotationSpeed);

                // Apply angular velocity
                rb.angularVelocity = randomDir * spinSpeed;
            }
        }

        shockWave.SetActive(true); // the shockWave will move upwards all the objects of the room that have a rigidbody

        if (levelManager != null)
            levelManager.StartGame();

        playerCamera.cameraShake.enabled = true;
        playerCamera.cameraShake.shakeAmount = 0.27f;
        playerCamera.cameraShake.shakeDuration = 0.8f;
        playerCamera.cameraShake.decreaseFactor = 0.45f;
        playerCamera.Zoom(Camera.main.fieldOfView, 0.1f);
        targetLens = 0f;
        bloom.intensity.value = 2f;
        chargeSmashSlider.gameObject.SetActive(false);
        SoundManager.instance.PlaySound(SoundManager.instance.smash);
        TriggerSlowMotion();
    }

    public void TriggerSlowMotion()
    {
        if (!isSlowing)
            slowMotionCoroutine = StartCoroutine(SlowMotionRoutine());
    }

    public IEnumerator SlowMotionRoutine()
    {
        isSlowing = true;
        SoundManager.instance.PlaySound(SoundManager.instance.slowMotionStart);
        yield return new WaitForSecondsRealtime(0.2f);
        playerCamera.isInSlowMo = true;

        // Smoothly reduce time scale
        while (Time.timeScale > slowTimeScale + 0.01f)
        {
            Time.timeScale = Mathf.Lerp(Time.timeScale, slowTimeScale, Time.unscaledDeltaTime * transitionSpeed);
            Time.fixedDeltaTime = 0.01f * Time.timeScale;
            yield return null;
        }
        Time.timeScale = slowTimeScale; // Snap exactly
        Time.fixedDeltaTime = 0.01f * Time.timeScale;

        // Wait in real time (unaffected by timescale)
        yield return new WaitForSecondsRealtime(slowmoDuration);

        if (canStopSlowMo) // depends by LevelManager, it can be turned off if all objects are thrown
            StopSlowMotion();
    }

    public void StopSlowMotion()
    {
        stopSlowMo = StartCoroutine(StopSlowMo());
    }

    IEnumerator StopSlowMo()
    {
        transitionSpeed = 8f;

        //Make object in hand of player reset proprieties(gravity, scale, rotation)
        Vector3 explosionPos = table.transform.position;
        Collider[] colliders = Physics.OverlapSphere(explosionPos, radius);

        foreach (Collider hit in colliders)
        {
            Interactable interactable = hit.GetComponent<Interactable>();

            if (interactable != null)
                interactable.ResetProprieties();
        }

        playerCamera.isInSlowMo = false;
        effects = false;

        // Smoothly restore time scale
        while (Time.timeScale < 1f - 0.01f)
        {
            Time.timeScale = Mathf.Lerp(Time.timeScale, 1f, Time.unscaledDeltaTime * transitionSpeed);
            Time.fixedDeltaTime = 0.01f * Time.timeScale;
            yield return null;
        }

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.01f;

        isSlowing = false;
        //canCharge = true;
    }

    public void StopSlowMoCoroutine() // stops the coroutines, because they interfere with the timescale
    {
        if (slowMotionCoroutine != null)
            StopCoroutine(slowMotionCoroutine);
        if (stopSlowMo != null)
            StopCoroutine(stopSlowMo);
    }
}
