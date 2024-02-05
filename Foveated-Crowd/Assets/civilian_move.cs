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
    [SerializeField] private float stopThreshold = 0.5f;
     
    //Private variables
    private bool playing = false;
    
    //Animator variables
    [SerializeField] int runningID;
    [SerializeField] int fastID;
    void Start()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.SetDestination(RandomNavSphere(transform.position, wanderRadius, -1));
        StartCoroutine(WanderSystem(wanderTimer));
        
        //Getting hashes for animator parameters
        runningID = Animator.StringToHash("Running");
        fastID = Animator.StringToHash("Fast");
        anim.SetBool(runningID, false);
        anim.SetBool(fastID,false);
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
            StartRunningAnimation();
            
            yield return new WaitForSeconds(wanderTimer);
        }
    }

    private void StartRunningAnimation()
    {
        if(anim.speed > slowFastThreshold) anim.SetBool("Fast",true);
        else anim.SetBool(fastID,false);
        anim.SetBool(runningID,true);
    }
    private void CheckStop()
    {
        if (anim.GetBool(runningID) == true && agent.remainingDistance < stopThreshold)
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
