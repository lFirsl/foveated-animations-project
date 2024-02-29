using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusPointSphere : MonoBehaviour
{
    //Public
    public bool shouldStop = true;
    public uint farFPS = 5;
    //Private
    [SerializeField] private float stopThreshold = 10;
    [SerializeField] private float foveationThreshold = 5;
    [SerializeField] private float animationsUpdateFrequency = 1f;
    
    //Internal Variables
    [SerializeField] private LayerMask layermask;
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
            yield return new WaitForFixedUpdate();
            int numOfAgents = Physics.OverlapSphereNonAlloc(_pos.position, stopThreshold,agents,layermask);
            for(int i = 0; i < numOfAgents; i++)
            {
                FoveatedAnimationTarget agent = agents[i].gameObject.GetComponent<FoveatedAnimationTarget>();
                DetermineAnimation(agent);
            }
            yield return new WaitForSeconds(animationsUpdateFrequency);
        }
    }

    private void DetermineAnimation(FoveatedAnimationTarget agent)
    {
        float distance = Vector3.Distance(_pos.position, agent.transform.position);
        agent.RestartAnimation(animationsResetTime);
        if(distance > foveationThreshold) agent.SetFixedFPS(0,animationsResetTime);
        else agent.SetForegroundFPS(animationsResetTime);
    }
}
