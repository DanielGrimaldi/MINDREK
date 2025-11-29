using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraShake : MonoBehaviour
{
	[Header("Shake Settings")]
    Transform camTransform;
	
	// How long the object should shake for.
	public float shakeDuration;
	
	// Amplitude of the shake. A larger value shakes the camera harder.
	public float shakeAmount;
	public float decreaseFactor;
	public bool enable;
	Vector3 originalPos;
	
	void Awake()
	{
		if (camTransform == null)
		{
			camTransform = GetComponent(typeof(Transform)) as Transform;
		}
	}
	
	void OnEnable()
	{
		if (!enable)
		{
			originalPos = camTransform.localPosition;
			shakeDuration = 0.5f;
			shakeAmount = 0.06f;
			decreaseFactor = 0.2f;	
		}
	}

	void Update()
	{
		if (shakeDuration > 0) 
        {
            gameObject.transform.localPosition = originalPos + Random.insideUnitSphere * shakeAmount;
            shakeDuration -= Time.unscaledDeltaTime;
            shakeAmount -= Time.unscaledDeltaTime * decreaseFactor;
            
            if (shakeAmount <= 0) 
			{
				shakeAmount = 0;
				this.enabled = false;
			}               
        }
        else 
        {
            shakeDuration = 0f;
            gameObject.transform.localPosition = originalPos;
			enabled = false;
        }
	}
}
