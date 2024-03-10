using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class CivilianMove : MonoBehaviour
{
    //Private components. Fetched during Start.
    private Animator _anim;
    private NavMeshAgent _agent;
    
    //Serialized Private Field
    public float wanderRadius;
    public Vector2 wanderTimerRange;
    public float minSpeed;
    public float maxSpeed;
    public float slowFastThreshold;
    public int stopThreshold;
    
    //Animator variables
    private int _runningID;
    private int _fastID;
    private const string RunningString = "Running";
    private const string FastString = "Fast";
    void Start()
    {
        _anim = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();
        _agent.SetDestination(RandomNavSphere(transform.position, wanderRadius, -1));
        
        _runningID = Animator.StringToHash(RunningString);
        _fastID = Animator.StringToHash(FastString);
        
        StartCoroutine(WanderSystem());
    }

    private void FixedUpdate()
    {
        CheckStop();
    }
    
    private IEnumerator WanderSystem()
    {
        while (this.isActiveAndEnabled)
        {
            _agent.SetDestination(RandomNavSphere(transform.position, wanderRadius, -1));
            _agent.speed = Random.Range(minSpeed, maxSpeed);

            float wanderTimer = Random.Range(wanderTimerRange.x, wanderTimerRange.y);
            
            //Determine animation to play
            StartRunningAnimation();
            
            yield return new WaitForSeconds(wanderTimer);
        }
    }

    private void StartRunningAnimation()
    {
        _anim.SetBool(_fastID, _agent.speed >= slowFastThreshold);
        _anim.SetBool(_runningID,true);
    }
    
    
    private void CheckStop()
    {
        if (!_agent.pathPending && _agent.remainingDistance < stopThreshold)
        {
            _anim.SetBool(_runningID,false);
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

