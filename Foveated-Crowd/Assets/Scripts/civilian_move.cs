using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;
using Random = UnityEngine.Random;

public class CivilianMove : MonoBehaviour
{
    //Private components. Fetched during Start.
    private Animator anim;
    private NavMeshAgent agent;
    
    //Serialized Private Field
    [SerializeField] private float wanderRadius;
    [SerializeField] private float wanderTimer;
    [SerializeField] private float minSpeed;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float slowFastThreshold;
    [SerializeField] private int stopThreshold;
    [SerializeField] private int lowFpsFrames = 30;
    
    //Animator variables
    [SerializeField] private int runningID;
    [SerializeField] private int fastID;
    private const string runningString = "Running";
    private const string fastString = "Fast";
    void Start()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.SetDestination(RandomNavSphere(transform.position, wanderRadius, -1));
        
        runningID = Animator.StringToHash(runningString);
        fastID = Animator.StringToHash(fastString);
        
        StartCoroutine(WanderSystem(wanderTimer));
    }

    private void FixedUpdate()
    {
        CheckStop();
    }
    
    private IEnumerator WanderSystem(float waitTime)
    {
        while (this.isActiveAndEnabled)
        {
            agent.SetDestination(RandomNavSphere(transform.position, wanderRadius, -1));
            agent.speed = Random.Range(minSpeed, maxSpeed);
            
            //Determine animation to play
            StartRunningAnimation();
            
            yield return new WaitForSeconds(wanderTimer);
        }
    }

    private void StartRunningAnimation()
    {
        anim.SetBool(fastID, agent.speed >= slowFastThreshold);
        anim.SetBool(runningID,true);
    }
    
    
    private void CheckStop()
    {
        if (!agent.pathPending && agent.remainingDistance < stopThreshold)
        {
            anim.SetBool(runningID,false);
        }
    }

    
    private static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask) {
        Vector3 randDirection = Random.insideUnitSphere * dist;
 
        randDirection += origin;
 
        NavMeshHit navHit;
 
        NavMesh.SamplePosition (randDirection, out navHit, dist, layermask);
 
        return navHit.position;
    }
}
