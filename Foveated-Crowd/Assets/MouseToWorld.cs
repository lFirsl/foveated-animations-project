using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class MouseToWorld : MonoBehaviour
{
    public Camera mainCamera;
    [SerializeField] private LayerMask _layerMask;

    void Update()
    {
        // Check if the left mouse button is clicked
        if (Input.GetMouseButtonDown(0))
        {
            // Cast a ray from the mouse position into the world
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Check if the ray hits something in the world
            if (Physics.Raycast(ray, out hit,Mathf.Infinity,_layerMask))
            {
                // Get the point where the ray hits the ground
                Vector3 targetPosition = hit.point;

                // Now you have the position in the 3D world where the mouse was clicked
                Debug.Log("Clicked at: " + targetPosition);

                // You can use this position for whatever you need, like moving units or placing objects
            }
        }
    }
}
