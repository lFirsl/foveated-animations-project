using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusPointSphere : MonoBehaviour
{
    //Public
    public Camera camera;
    public bool shouldStop = true;
    public uint farFPS = 5;
    //Private
    [SerializeField] private float stopThreshold = 10;
    [SerializeField] private float foveationThreshold = 5;
    [SerializeField] private float animationsUpdateFrequency = 1f;
    
    //Internal Variables
    [SerializeField] private LayerMask layermask;
    [SerializeField] private LayerMask _rayLayerMask;
    [SerializeField] private float animationsResetTime = 0.5f;
    Collider[] agents = new Collider[300];
    
    //Objects
    private Transform _pos;
    
    void Start()
    {
        _pos = gameObject.GetComponent<Transform>();
        Debug.Log("Started");

        StartCoroutine(UpdateAnimations());
    }
    

    private IEnumerator UpdateAnimations()
    {
        while (true)
        {
            // Cast a ray from the mouse position into the world
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Vector3 targetPosition;

            // Check if the ray hits something in the world
            if (Physics.Raycast(ray, out hit,Mathf.Infinity,_rayLayerMask))
            {
                // Get the point where the ray hits the ground
                targetPosition = hit.point;
            }
            else
            {
                targetPosition = Input.mousePosition;
            }
            yield return new WaitForFixedUpdate();
            int numOfAgents = Physics.OverlapSphereNonAlloc(targetPosition, stopThreshold,agents,layermask);
            for(int i = 0; i < numOfAgents; i++)
            {
                FoveatedAnimationTarget agent = agents[i].gameObject.GetComponent<FoveatedAnimationTarget>();
                DetermineAnimation(agent,targetPosition);
            }
            yield return new WaitForSeconds(animationsUpdateFrequency);
        }
    }

    private void DetermineAnimation(FoveatedAnimationTarget agent,Vector3 centre)
    {
        float distance = Vector3.Distance(centre, agent.transform.position);
        agent.RestartAnimation(animationsResetTime);
        if(distance > foveationThreshold) agent.SetFixedFPS(0,animationsResetTime);
        else agent.SetForegroundFPS(animationsResetTime);
    }
}
