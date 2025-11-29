using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCameraMouseFollow : MonoBehaviour
{
    [Header("Settings")]
    public float moveAmount = 0.5f; // how far the camera can move
    public float smoothSpeed = 5f;  // how quickly it moves

    private Vector3 initialPos;

    void Start()
    {
        initialPos = transform.position;
    }

    void Update()
    {
        // Mouse position normalized (0 to 1)
        Vector2 mousePos = new Vector2(
            Input.mousePosition.x / Screen.width,
            Input.mousePosition.y / Screen.height
        );

        // Convert to range -1 to 1 (center is 0, edges are -1 or 1)
        Vector2 offset = new Vector2(
            (mousePos.x - 0.5f) * 2f,
            (mousePos.y - 0.5f) * 2f
        );

        // Target camera position
        Vector3 targetPos = initialPos + new Vector3(offset.x, offset.y, 0) * moveAmount;

        // Smooth movement
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smoothSpeed);
    }
}
