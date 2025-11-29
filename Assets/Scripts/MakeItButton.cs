using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MakeItButton : MonoBehaviour
{
    public UnityEvent unityEvent = new UnityEvent();
    Ray ray;
    RaycastHit hit;

    public float scaleSpeed, hoverScaleMultiplier;
    private Vector3 originalScale, targetScale;
    public AudioSource hoverSound;
    bool playedSound;

    // Start is called before the first frame update
    void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
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
                unityEvent.Invoke();
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
        }
        else
        {
            targetScale = originalScale;
            playedSound = false;
        }
        
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * scaleSpeed);
    }
}
