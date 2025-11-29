using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Interactable : MonoBehaviour
{
    PlayerCamera playerCamera;
    Enemy enemy;
    LevelManager levelManager;
    [HideInInspector] public Rigidbody rb;
    public Camera cam;
    [HideInInspector] public bool isHovered, isDragging, isRotating, isShooted, isChargingShoot, isInSlowMo;
    bool symbolPopOut, soundPlayed;
    [HideInInspector] public bool isDeadly, isSmall, isSuicide, hasNotHit = true; // used here and in deadlyObject
    public float shootForce, dragForce, rotationSpeed;
    public float damage;
    public float hoverScaleMultiplier;
    public float scaleSpeed;
    private Vector3 originalScale, targetScale;
    private string[] weightCategories = {"light", "medium", "heavy", "deadly"};
    public string objectWeight;
    float weightShootCharge;
    [HideInInspector] public GameObject weightSymbolPrefab, weightSymbol, bloodType, shootParticle;
    [HideInInspector] public AudioSource weightSymbolSound, throwSound;
    private Renderer rendererr;
    float targetMetallic, targetSmoothness, currentMetallic, currentSmoothness;
    public Color hoverColor;
    Color originalColor, targetColor;
    public PhysicMaterial bounciness;
    Animator weightSymbolAnim;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cam = Camera.main;
        playerCamera = FindObjectOfType<PlayerCamera>();
        levelManager = FindObjectOfType<LevelManager>();

        originalScale = transform.localScale;
        targetScale = originalScale;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        if (objectWeight == "light")
        {
            damage = 0.5f;
            rb.mass = 0.9f;
            shootForce = 3500;
            dragForce = 800f;
            weightShootCharge = 0;
            weightSymbolPrefab = playerCamera.feather;
            weightSymbolSound = SoundManager.instance.feather;
            isSmall = true;
            //throwSound = SoundManager.instance.throwLight;
        }
        if (objectWeight == "medium")
        {
            damage = 1f;
            rb.mass = 1f;
            shootForce = 3000;
            dragForce = 400f;
            weightShootCharge = 0.5f;
            shootParticle = levelManager.shootMedium;
            weightSymbolPrefab = playerCamera.brick;
            weightSymbolSound = SoundManager.instance.brick;
            //throwSound = SoundManager.instance.throwMedium;
        }
        if (objectWeight == "heavy")
        {
            damage = 1.5f;
            rb.mass = 1.1f;
            shootForce = 2700;
            dragForce = 150f;
            weightShootCharge = 1;
            shootParticle = levelManager.shootHeavy;
            weightSymbolPrefab = playerCamera.anvil;
            weightSymbolSound = SoundManager.instance.anvil;
            //throwSound = SoundManager.instance.throwHeavy;
        }
        if (objectWeight == "deadly")
        {
            damage = 0.5f;
            rb.mass = 0.9f;
            shootForce = 3300;
            dragForce = 800f;
            weightShootCharge = 0;
            weightSymbolPrefab = playerCamera.skull;
            weightSymbolSound = SoundManager.instance.skull;
            //throwSound = SoundManager.instance.throwLight;
        }

        weightSymbol = Instantiate(weightSymbolPrefab, playerCamera.canvas.transform);
        weightSymbolAnim = weightSymbol.GetComponent<Animator>();
        throwSound = SoundManager.instance.throwLight;

        // used to prevent the symbol being on top of hand cursor image
        weightSymbol.transform.SetParent(playerCamera.canvas.transform.Find("WEIGHTS"));

        rendererr = GetComponent<Renderer>();
        // Ensure unique material instance so only this object changes
        rendererr.material = new Material(rendererr.material);
        originalColor = rendererr.material.color;
        targetColor = originalColor;
        targetMetallic = 0.5f;
        targetSmoothness = 1;
        GetComponent<Collider>().material = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isShooted) //check if it has been thrown, if it hasn't then it can be modified
        {
            CheckHover();

            if (isInSlowMo && Vector3.Distance(transform.position, cam.gameObject.transform.position) <= 2.5f)
            {
                if (isHovered)
                {
                    if (!isSuicide)
                    {
                        HandleDragging();
                        HandleRotation();
                    }

                    HandleShooting();

                    if (!isDragging)
                    {
                        targetScale = originalScale * hoverScaleMultiplier;
                        targetColor = hoverColor;
                        targetMetallic = 0;
                        targetSmoothness = 0;
                    }
                    else
                    {
                        targetScale = originalScale;
                        targetColor = originalColor;
                        targetMetallic = 0.5f;
                        targetSmoothness = 1;
                    }

                    rb.useGravity = false;
                    rb.velocity = Vector3.zero; // Stop any movement
                    rb.angularVelocity = Vector3.zero; // Stop any rotation

                    // find hitPoint to cast shadow
                    Ray ray = new Ray(transform.position, Vector3.down);
                    if (Physics.Raycast(ray, out RaycastHit hit, 100, 1 << LayerMask.NameToLayer("Plane")))
                        playerCamera.shadow.transform.position = hit.point + Vector3.up * 0.01f;
                    playerCamera.shadow.SetActive(true);
                }

                // makes the weightSymbol always follow the object
                Vector3 weightSymbolPos = cam.WorldToScreenPoint(transform.position);
                weightSymbol.transform.position = weightSymbolPos;
            }
        }
        else
            targetColor = originalColor;

        SmoothScale();
        ChangeColor();

        //if (Time.timeScale <= 0.07f && !levelManager.lostGame)
        if(levelManager.isInSlowMo && !levelManager.lostGame)
        {
            isInSlowMo = true;
        }
        if (!levelManager.isInSlowMo || levelManager.lostGame)
        {
            isInSlowMo = false;
            WeightSymbolFollow(false);
            rb.WakeUp();
            ResetProprieties();
        }
    }

    void CheckHover()
    {
        // If not hovering anymore
        if (!isHovered)
        {
            targetScale = originalScale;
            targetColor = originalColor;
            targetMetallic = 0.5f;
            targetSmoothness = 1;
            isDragging = false;
            rb.useGravity = true;
        }
    }

    void HandleDragging()
    {            
        if (Input.GetMouseButtonDown(0))
        {
            // Store distance from camera so object follows correctly
            float distance = Vector3.Distance(transform.position, cam.transform.position);
            playerCamera.grabPos.position = cam.transform.position + cam.transform.forward * distance;

            rb.useGravity = false;   // disable gravity while held
            isDragging = true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            rb.useGravity = true;    // restore gravity when released
        }

        if (isDragging)
        {
            // Smooth movement toward grab position in UN-SCALED time
            Vector3 toTarget = playerCamera.grabPos.position - rb.position;

            // Apply force toward grab point (scaled by unscaledDeltaTime)
            rb.AddForce(toTarget * dragForce, ForceMode.VelocityChange);
            playerCamera.isHolding = true;
        }
        else
        {
            playerCamera.isHolding = false;
        }
    }

    void HandleRotation()
    {
        if(!playerCamera.shootSlider.gameObject.activeInHierarchy) // if it's not charging to shoot, then can rotate object
        {
            // check if key is pressed to drag
            if (isDragging && Input.GetKey(KeyCode.R))
            {
                isRotating = true;
                playerCamera.canMove = false;
                playerCamera.Zoom(47f, 0.2f);
                rb.velocity = Vector3.zero; // Stop any movement
                rb.angularVelocity = Vector3.zero; // Stop any rotation
            }
            // check if key is up to stop dragging
            if (Input.GetKeyUp(KeyCode.R) || Input.GetMouseButtonUp(0))
            {
                isRotating = false;
                playerCamera.canMove = true;
                playerCamera.Zoom(60f, 0.2f);
            }

            if(isRotating)
            {
                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = Input.GetAxis("Mouse Y");

                Vector3 camRight = cam.transform.right;
                Vector3 camUp = cam.transform.up;

                // Apply rotation around camera axes
                transform.Rotate(camUp, -mouseX * rotationSpeed * Time.unscaledDeltaTime, Space.World);
                transform.Rotate(camRight, mouseY * rotationSpeed * Time.unscaledDeltaTime, Space.World);

                // Move interactable at centre of screen
                transform.position = Vector3.Lerp(transform.position, playerCamera.grabPos.position, 10 * Time.unscaledDeltaTime);
            }
        }
    }
    
    void HandleShooting()
    {
        // charge the shooting
        if (Input.GetMouseButtonDown(1))
        {
            playerCamera.shootSlider.gameObject.SetActive(true);
            playerCamera.shootSlider.maxValue = weightShootCharge; // change time to charge the shoot based on object weight

            isChargingShoot = true;
            playerCamera.canMove = false;
            targetScale = originalScale;
            targetColor = originalColor;
            targetMetallic = 0.5f;
            targetSmoothness = 1;
        }

        if (isChargingShoot)
        {
            playerCamera.ShootSliderFollow(transform.position);
            playerCamera.shootSlider.value += Time.unscaledDeltaTime;
        }

        // SHOOT
        if (playerCamera.shootSlider.value >= playerCamera.shootSlider.maxValue && playerCamera.shootSlider.gameObject.activeInHierarchy)
        {
            StartCoroutine(Shoot());
        }

        // if it's charging the shooting and you release the mouse button before finishing the charge, then stop charging
        if(Input.GetMouseButtonUp(1) && playerCamera.shootSlider.gameObject.activeInHierarchy)
        {
            playerCamera.shootSlider.gameObject.SetActive(false);
            playerCamera.shootSlider.value = 0;
            isChargingShoot = false;
            playerCamera.canMove = true;
        }
        
    }

    IEnumerator Shoot()
    {
        isShooted = true;
        isChargingShoot = false;
        Vector3 shootDirection = cam.transform.forward;
        GetComponent<Collider>().material = bounciness;
        //throwSound.pitch = UnityEngine.Random.Range(0.6f, 1f); // to make the throw sound different every time
        SoundManager.instance.PlaySound(throwSound);
        ResetProprieties();

        if (levelManager != null)
            levelManager.ReduceNumOfInteractables();

        // works only if it's the only object remaining
        if (!playerCamera.gameObject.GetComponent<Smash>().canStopSlowMo)
            shootForce = 2000f;
            
        rb.useGravity = false;
        rb.isKinematic = true;

        yield return new WaitForSecondsRealtime(0.0001f);
        rb.isKinematic = false;
        rb.AddForce(shootDirection.normalized * shootForce / Time.timeScale);
        playerCamera.cameraShake.enabled = true;      
            // Vector3 randomDir = UnityEngine.Random.onUnitSphere;
            // float spinSpeed = UnityEngine.Random.Range(-11, 11);
            // rb.angularVelocity = randomDir * spinSpeed;  
        if(shootParticle != null)
            Instantiate(shootParticle, transform.position, Quaternion.identity);

        Time.timeScale = 1;
        
        yield return new WaitForSecondsRealtime(0.015f);

        // gives a cool effect, works only if it's not the only object remaining
        if (playerCamera.gameObject.GetComponent<Smash>().canStopSlowMo)
        {
            Time.timeScale = 0.03f;
            Time.fixedDeltaTime = 0.01f * Time.timeScale;
        }
        else
        {
            Time.fixedDeltaTime = 0.01f * Time.timeScale;
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        // check collision with enemy limbs
        if (collision.gameObject.CompareTag("Limb") && hasNotHit && !isDeadly) // if it isn't a deadly object, make the hit logic
        {
            Hit(collision);
        }
    }

    public void Hit(Collision collision)
    {
        enemy = collision.collider.GetComponentInParent<Enemy>();
        EnemyHit();

        if (enemy.health <= 0 && !enemy.alreadyHit)
        {
            // create a knockBack for the enemy
            Rigidbody limbRb = collision.gameObject.GetComponent<Rigidbody>();
            float pushForce = 22000f * (damage * 3f); // Adjust as needed
            Vector3 pushDirection = limbRb.transform.position - transform.position; // back along local X-axis
            limbRb.AddForce(pushDirection.normalized * pushForce, ForceMode.Impulse);

            foreach (ContactPoint contact in collision.contacts)
            {
                Vector3 hitPoint = contact.point; // The exact point of contact
                Vector3 hitNormal = contact.normal; // The normal at the contact

                if (objectWeight == "light")
                    bloodType = enemy.bloodLightParticle;
                if (objectWeight == "medium" || objectWeight == "deadly")
                    bloodType = enemy.bloodMediumParticle;
                if (objectWeight == "heavy")
                    bloodType = enemy.bloodHeavyParticle;

                var blood = Instantiate(bloodType, hitPoint, Quaternion.LookRotation(hitNormal));
                blood.transform.SetParent(enemy.transform); // this way the blood dissapears when the enemy is destroyed
                enemy.alreadyHit = true; // so other objects don't emitt other blood if they hit enemy again
            }
        }
    }

    void EnemyHit()
    {
        enemy.LoseHealth(damage);
        hasNotHit = false;
        rb.useGravity = true;
        enabled = false;
    }

    void SmoothScale()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * scaleSpeed);
    }
    void ChangeColor()
    {
        rendererr.material.color = Color.Lerp(rendererr.material.color, targetColor, Time.unscaledDeltaTime / 0.2f);
        currentMetallic = Mathf.Lerp(currentMetallic, targetMetallic,  Time.unscaledDeltaTime / 0.2f);
        //currentSmoothness = Mathf.Lerp(currentSmoothness, targetSmoothness,  Time.unscaledDeltaTime / 0.1f);

        rendererr.material.SetFloat("_Metallic", currentMetallic);
        //rendererr.material.SetFloat("_Glossiness", currentSmoothness);
    }

    public void WeightSymbolFollow(bool show)
    {
        if (show)
        {
            weightSymbolAnim.SetBool("dissapear", false);
            symbolPopOut = true;
        }
        else
        {
            weightSymbolAnim.SetBool("dissapear", true);
            symbolPopOut = false;
            soundPlayed = false;
        }

        if (!soundPlayed && symbolPopOut && !isShooted) // makes the sound play only once
        {
            weightSymbolSound.Play();
            soundPlayed = true;
        }
    }

    public void ResetProprieties()
    {
        rb.useGravity = true;
        rb.isKinematic = false;
        isHovered = false;
        isDragging = false;
        isRotating = false;
        targetScale = originalScale;
        targetColor = originalColor;
        targetMetallic = 0.5f;
        targetSmoothness = 0;
        rb.constraints = RigidbodyConstraints.None;
        playerCamera.shootSlider.value = playerCamera.shootSlider.minValue;
        playerCamera.shootSlider.gameObject.SetActive(false);
        playerCamera.shadow.SetActive(false);
        playerCamera.Zoom(60f, 0.2f);
        playerCamera.isHolding = false;
        if(!levelManager.lostGame)
            playerCamera.canMove = true;
        WeightSymbolFollow(false);
    }
}
