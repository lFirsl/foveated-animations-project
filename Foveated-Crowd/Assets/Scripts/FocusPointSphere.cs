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
            Debug.Log("Going");
            yield return new WaitForFixedUpdate();
            Collider[] agents = Physics.OverlapSphere(_pos.position, stopThreshold,layermask);
            foreach (var agentCollider in agents)
            {
                Debug.Log("Found a foveation target");
                FoveatedAnimationTarget agent = agentCollider.gameObject.GetComponent<FoveatedAnimationTarget>();
                determineAnimation(agent);
            }

            yield return new WaitForSeconds(animationsUpdateFrequency);
        }
    }

    private void determineAnimation(FoveatedAnimationTarget agent)
    {
        float distance = Vector3.Distance(_pos.position, agent.transform.position);
        agent.RestartAnimation(animationsResetTime);
        if(distance > foveationThreshold) agent.SetFixedFPS(0,animationsResetTime);
        else agent.SetForegroundFPS(animationsResetTime);
    }
}
