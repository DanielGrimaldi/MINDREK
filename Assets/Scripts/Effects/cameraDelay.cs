using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraDelay : MonoBehaviour
{
    Vector3 vectOffset;
    float lastCall;
    [SerializeField] GameObject goFollow;
    public float speed;
    
    void Awake()
    {

    }
    void Start()
    {
        vectOffset = transform.position - goFollow.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time - lastCall >= 0.2f)
            delay();
    }

    void delay()
    {
        //transform.position = goFollow.transform.position + vectOffset;
        transform.rotation = Quaternion.Slerp(transform.rotation, goFollow.transform.rotation, speed * Time.unscaledDeltaTime);
    }
}
