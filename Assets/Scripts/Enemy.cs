using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    PlayerCamera playerCamera;
    LevelManager levelManager;
    private Collider mainCollider;

    public Animator animator;
    public float health, moveSpeed, changeColorSpeed, distance;
    public GameObject enemyChild, bloodLightParticle, bloodMediumParticle, bloodHeavyParticle;
    private NavMeshAgent agent;

    private Rigidbody[] ragdollBodies;
    private Collider[] ragdollColliders;
    [HideInInspector] public Renderer rendererr;
    Transform body;
    public Color originalColor, colorTarget, deadColor, hitFlashColor, nearTableColor;
    bool isRagdoll, isIntelligent;
    public bool alreadyHit; // used in interactable

    void Awake()
    {
        mainCollider = GetComponent<Collider>();

        // Get all rigidbodies and colliders in children
        ragdollBodies = GetComponentsInChildren<Rigidbody>();
        ragdollColliders = GetComponentsInChildren<Collider>();

        // Disable ragdoll parts at start
        SetRagdollState(false);

        playerCamera = FindObjectOfType<PlayerCamera>();
        levelManager = FindObjectOfType<LevelManager>();

        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
            isIntelligent = true; // isIntelligent means that the enemy is able to move around obstacles

        // Search for the child in this GameObject's hierarchy only
        body = FindChildByName(transform, "Player");
        rendererr = body.GetComponent<Renderer>();
        // Ensure unique material instance so only this enemy changes
        rendererr.material = new Material(rendererr.material);

        originalColor = rendererr.material.color;
        colorTarget = originalColor;
    }

    // makes the animation start at a random frame (to make all the enemeis desynchronized)
    void OnEnable()
    {
        RuntimeAnimatorController rac = animator.runtimeAnimatorController;
        AnimationClip clip = rac.animationClips[0]; // or find the one you want

        float clipLength = clip.length;
        float timeInSeconds = UnityEngine.Random.Range(2f, 5f);
        float normalizedTime = timeInSeconds / clipLength;

        animator.Play(clip.name, 0, normalizedTime);
    }

    void Update()
    {
        if (health > 0)
        {
            if (!isIntelligent)
            {
                transform.LookAt(playerCamera.lookPos);
                transform.position += transform.forward * moveSpeed * Time.deltaTime;
            }
            else
            {
                agent.destination = playerCamera.transform.position * Time.deltaTime;
            }

            foreach (var rb in ragdollBodies)
            {
                rb.transform.position = rb.transform.position; // optionally use stored animator bones here
                rb.transform.rotation = rb.transform.rotation;
            }
        }

        rendererr.material.color = Color.Lerp(rendererr.material.color, colorTarget, Time.unscaledDeltaTime / changeColorSpeed);

        distance = Vector3.Distance(transform.position, playerCamera.lookPos.transform.position) - 3f;
        // if (distance <= 1.7f && animator.enabled)
        // {
        //     // Normalize between 0 (close) and 1 (far)
        //     float t = Mathf.Clamp01(distance / 3.4f);
        //     // Lerp between RED (close) and WHITE (far)
        //     playerCamera.grain.intensity.value = Mathf.Lerp(0f, 1f, t);
        // }
        if (distance <= 0.8f && animator.enabled)
        {
            // Normalize between 0 (close) and 1 (far)
            float t = Mathf.Clamp01(distance / 0.8f);
            // Lerp between RED (close) and WHITE (far)
            colorTarget = Color.Lerp(nearTableColor, originalColor, t);
            //Time.deltaTime / 0.06f
        }

        if (distance <= 0 && health > 0 && !levelManager.lostGame)
            levelManager.LoseGame();
    }

    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Interactable"))
        {
            if (collision.gameObject.GetComponent<Interactable>() != null)
            {
                foreach (var col in ragdollColliders) // activate all limb's collider
                    col.enabled = true;
            }
        }
    }

    public void LoseHealth(float damage)
    {
        if (!isRagdoll && !levelManager.lostGame)
        {
            health -= damage;
            if (health <= 0)
            {
                SoundManager.instance.enemyDie.Play();
                changeColorSpeed = 0.5f;
                colorTarget = deadColor;
                levelManager.enemyKillFlash.GetComponent<Animator>().CrossFade("enemyKillFlash",0);
                ActivateRagdoll();
            }
            else
            {
                SoundManager.instance.hitLimb.Play();
                rendererr.material.color = hitFlashColor;
            }
        }
    }

    public void ActivateRagdoll()
    {
        animator.enabled = false;

        if (agent != null)
            agent.enabled = false;

        if (mainCollider != null)
            mainCollider.enabled = false;

        SetRagdollState(true);
        //StartCoroutine(EnemyKillCam());
        isRagdoll = true;
        gameObject.tag = "Untagged";
        levelManager.ReduceNumOfEnemies();

        int enemyKillCount = PlayerPrefs.GetInt("EnemyKillCount");
        enemyKillCount += 1;
        PlayerPrefs.SetInt("EnemyKillCount", enemyKillCount);
    }

    private void SetRagdollState(bool isRagdoll)
    {
        foreach (var rb in ragdollBodies)
        {
            rb.isKinematic = !isRagdoll;
        }

        foreach (var col in ragdollColliders)
        {
            if (col == mainCollider) continue; // skip the root collider
            col.enabled = isRagdoll;
        }
    }

    IEnumerator EnemyKillCam()
    {
        Time.timeScale = 1;

        yield return new WaitForSecondsRealtime(0.015f);
        if(playerCamera.gameObject.GetComponent<Smash>().canStopSlowMo)
        {
            Time.timeScale = 0.02f;
            Time.fixedDeltaTime = 0.01f * Time.timeScale;
        }      
    }

    // Recursive search to find any nested child by name
    Transform FindChildByName(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform result = FindChildByName(child, name);
            if (result != null)
                return result;
        }
        return null;
    }
}
