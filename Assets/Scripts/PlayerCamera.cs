using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

public class PlayerCamera : MonoBehaviour
{
	public cameraShake cameraShake;
	public float mouseSensitivity = 100f;
	Vector3 xRotation;
	public bool canMove, isHolding, canHover = true, isInSlowMo;

	public Slider shootSlider; // used in Interactable
	private Vector3 shootSliderPos;
	public Transform grabPos; // used in Interactable
	public Transform lookPos; // used in Enemy

	public Canvas canvas; // used in Interactable for weightPrefabSpawn

	Camera cam;
	float zoomVelocity;
	public float targetFOV = 60f, smoothTime = 0.2f;
	[HideInInspector] public DepthOfField depthOfField;
    private float currentFocusDist;

	public GameObject[] pointers;
	public GameObject activePointer, feather, brick, anvil, skull, shadow;
	int currentIndexPointer = -1;
	public TMP_Text objectName;

	[HideInInspector] public Interactable previouslyHovered, current;
	RaycastHit hit;
    Ray ray;


	// Use this for initialization
	void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
		cam = Camera.main;
		mouseSensitivity = PlayerPrefs.GetFloat("mouseSensitivity");
		activePointer.SetActive(true);

		PostProcessVolume volume = FindObjectOfType<PostProcessVolume>();
        volume.profile.TryGetSettings(out depthOfField);

        currentFocusDist = depthOfField.focusDistance.value;
	}

	// Update is called once per frame
	void Update()
	{
		if (canMove)
		{
			float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.unscaledDeltaTime;
			float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.unscaledDeltaTime;

			xRotation.x -= mouseY;
			xRotation.x = Mathf.Clamp(xRotation.x, -80f, 80f); //lock up down rotation

			xRotation.y += mouseX;
			xRotation.y = Mathf.Clamp(xRotation.y, -62f, 62f); //lock left right rotation

			transform.localRotation = Quaternion.Euler(xRotation.x, xRotation.y, 0f);
		}

		cam.fieldOfView = Mathf.SmoothDamp(cam.fieldOfView, targetFOV, ref zoomVelocity, smoothTime, Mathf.Infinity, Time.unscaledDeltaTime);

		if (canHover)
			CheckHover();
		else
			PointerChange(0); // point		
	}

	void CheckHover()   // check if camera ray is hovering on an interactable
	{
		if (isInSlowMo)
		{
			ray = cam.ScreenPointToRay(Input.mousePosition);

			// First try normal raycast
			if (Physics.Raycast(ray, out hit, Mathf.Infinity, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide))
			{
				Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red);

				float targetDist;
				targetDist = Mathf.Clamp(hit.distance, 0.5f, 30f);

				// Smooth transition of focus
				currentFocusDist = Mathf.Lerp(currentFocusDist, targetDist, Time.unscaledDeltaTime * 100);
				depthOfField.focusDistance.value = currentFocusDist;

				// Skip this hit if dragging something else
				if (previouslyHovered != null && previouslyHovered.isDragging && hit.transform != previouslyHovered.transform)
					return;

				if (hit.transform.CompareTag("Interactable"))
				{
					current = hit.transform.GetComponent<Interactable>();

					if (current.isInSlowMo && Vector3.Distance(transform.position, current.transform.position) <= 2.5f)
					{
						// If we're hovering over a new object, reset the previous one
						if (previouslyHovered != current)
						{
							if (previouslyHovered != null)
							{
								previouslyHovered.isHovered = false;
								previouslyHovered.WeightSymbolFollow(false);
							}

							previouslyHovered = current;
							current.WeightSymbolFollow(true); // Show symbol when you enter a new object
						}

						current.isHovered = true;

						if (!current.isShooted)
						{
							// Pointer changes
							if (!current.isDragging && !current.isChargingShoot && current.isHovered)
							{
								PointerChange(1); // hand
								objectName.text = current.name; // get the name of the object hovering
							}
							else if (current.isDragging && !current.isRotating && !current.isChargingShoot)
								PointerChange(2); // grab
							else if (current.isChargingShoot)
								PointerChange(0); // point
							else if (current.isRotating)
								PointerChange(3); // null

							// Hide symbol if dragging or charging
							if (current.isDragging || current.isChargingShoot)
								current.WeightSymbolFollow(false);
						}
						else
						{
							current.WeightSymbolFollow(false);
							PointerChange(0); // default pointer
							objectName.text = "";
							shadow.SetActive(false);
						}
					}

					return; // ✅ Stop here if we hit a normal interactable
				}
			}

			// If no interactable was hit, try "Superhot style" hover with SphereCast
			RaycastHit[] sphereHits = Physics.SphereCastAll(ray, 0.1f, 5f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);

			// 🚫 Don't process small objects if currently dragging something
			if (previouslyHovered != null && previouslyHovered.isDragging)
				return;

			Interactable nearestSmall = null;
			float nearestDist = Mathf.Infinity;

			foreach (RaycastHit sHit in sphereHits)
			{
				Interactable interactable = sHit.transform.GetComponent<Interactable>();
				if (interactable != null && interactable.isSmall) // only check small objects
				{
					float dist = sHit.distance;
					if (dist < nearestDist)
					{
						nearestDist = dist;
						nearestSmall = interactable;
					}
				}
			}

			// If we found a small interactable, handle hover
			if (nearestSmall != null && nearestSmall.isInSlowMo && Vector3.Distance(transform.position, nearestSmall.transform.position) <= 2.5f)
			{
				if (previouslyHovered != nearestSmall)
				{
					if (previouslyHovered != null)
					{
						previouslyHovered.isHovered = false;
						previouslyHovered.WeightSymbolFollow(false);
					}

					previouslyHovered = nearestSmall;
					nearestSmall.WeightSymbolFollow(true);
				}

				nearestSmall.isHovered = true;
				PointerChange(1); // hand
				objectName.text = nearestSmall.name;
				return; // ✅ stop here, no need to clear hover
			}

			// If no valid interactable at all, reset hover
			if (previouslyHovered != null && !previouslyHovered.isDragging)
			{
				previouslyHovered.isHovered = false;
				previouslyHovered.WeightSymbolFollow(false);
				previouslyHovered = null;

				PointerChange(0); // default pointer
				objectName.text = "";
				shadow.SetActive(false);
				isHolding = false;
			}

		}
		else
		{
			PointerChange(0); // point
			objectName.text = "";
			shadow.SetActive(false);
		}
	}

	public void ShootSliderFollow(Vector3 objectPos) // used in Interactable, the shoot slider follows the object's (Interactable) position in the canvas
	{
		shootSliderPos = cam.WorldToScreenPoint(objectPos);
		shootSlider.transform.position = shootSliderPos;
	}

	public void PointerChange(int index) // the mouse cursor image switches to point, hand or grab
	{
		if (index == currentIndexPointer) // used to make the cursor activate once (resolved a bug)
			return;

		if (activePointer != null)
			activePointer.SetActive(false);

		activePointer = pointers[index];
		activePointer.SetActive(true);
		currentIndexPointer = index;
	}

	public void Zoom(float zoomedFOV, float speed)
	{
		targetFOV = zoomedFOV;
		smoothTime = speed;
	}
}
