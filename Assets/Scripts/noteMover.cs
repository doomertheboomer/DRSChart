using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class noteMover : MonoBehaviour
{
    public bool isPoint;
    public bool isSkidStart;
    public bool isSkidEnd;
    public GameObject skidEnd;
    public int kind = 0;

    private bool isDragging = false;
    private Vector3 offset;

    void OnMouseDown()
    {
        // Calculate the offset between the object's position and the mouse position
        offset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        isDragging = true;
    }

    void OnMouseDrag()
    {
        if (isDragging)
        {
            // Update the object's position based on the mouse position
            Vector3 newPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition) + offset;
            transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);
        }
    }

    void OnMouseUp()
    {
        isDragging = false;
    }
}
