using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class civilian_move : MonoBehaviour
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
     
    //Private variables
    private bool playing = false;
    
    //Animator variables
    /*[SerializeField] private string runningID = "Running";
    [SerializeField] private string fastID = "Fast";*/
    void Start()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.SetDestination(RandomNavSphere(transform.position, wanderRadius, -1));
        StartCoroutine(WanderSystem(wanderTimer));
    }

    private void FixedUpdate()
    {
        CheckStop();
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (playing)
            {
                anim.StopPlayback();
                playing = false;
            }
            else
            {
                anim.StartPlayback();
                playing = true;
            }
        }
    }
    
    private IEnumerator WanderSystem(float waitTime)
    {
        while (this.isActiveAndEnabled)
        {
            agent.SetDestination(RandomNavSphere(transform.position, wanderRadius, -1));
            agent.speed = Random.Range(minSpeed, maxSpeed);
            
            //Determine animation to play
            if(playing) StartRunningAnimation();
            
            yield return new WaitForSeconds(wanderTimer);
        }
    }

    private void StartRunningAnimation()
    {
        anim.SetBool("Fast", agent.speed >= slowFastThreshold);
        anim.SetBool("Running",true);
    }
    private void CheckStop()
    {
        if (!agent.pathPending && agent.remainingDistance < stopThreshold)
        {
            anim.SetBool("Running",false);
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
