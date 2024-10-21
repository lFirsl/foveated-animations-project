using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class CivilianMove : MonoBehaviour
{
    //Private components. Fetched during Start.
    private Animator _anim;
    private NavMeshAgent _agent;
    private Camera _mainCamera;
    
    //Serialized Private Field
    public float wanderRadius;
    public Vector2 wanderTimerRange;
    public float minSpeed;
    public float maxSpeed;
    public float slowFastThreshold;
    public int stopThreshold;
    public bool limitToView = false;
    [SerializeField] private bool protagonist = false;
    
    [SerializeField] private float MinimumAnimationSpeed = 0.8f;
    [SerializeField] private float MaximumAnimationSpeed = 1.2f;
    
    [SerializeField] private float AccelerationRate = 0.5f;
    private float currentMaxSpeed;
    
    //Animator variables
    private int _runningID;
    private int _fastID;
    private bool decelerateRunning = false;
    private const string RunningString = "Running";
    private const string FastString = "Fast";
    void Start()
    {
        _anim = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();
        _mainCamera = Camera.main;
        _agent.SetDestination(RandomNavSphere(transform.position, wanderRadius, -1));
        
        _runningID = Animator.StringToHash(RunningString);
        _fastID = Animator.StringToHash(FastString);

        _anim.speed = Random.Range(MinimumAnimationSpeed, MaximumAnimationSpeed);
        if (protagonist) _agent.avoidancePriority = 1;
        
        StartCoroutine(WanderSystem());
    }

    private void Update()
    {
        if (!_anim.GetBool(_runningID) && _agent.speed > 0.01f)
        {
            _anim.SetBool(_runningID, true);
            if(!protagonist) _agent.avoidancePriority = 50;
            //_anim.SetBool(_fastID, false);
        }
        else if (_anim.GetBool(_runningID))
        {
            if (_agent.speed < 0.01f)
            {
                _anim.SetBool(_runningID, false);
                if(!protagonist) _agent.avoidancePriority = 0;
                //_anim.SetBool(_fastID, false);
            }
            //else if (!_anim.GetBool(_fastID) && _agent.speed > slowFastThreshold) _anim.SetBool(_fastID, true);
            //else if (_anim.GetBool(_fastID)) _anim.SetBool(_fastID, false);
        }
        
        if(_agent.remainingDistance < stopThreshold) StartCoroutine(Decelerate());
    }
    
    private IEnumerator WanderSystem()
    {
        while (this.isActiveAndEnabled)
        {
            _agent.SetDestination(RandomNavSphere(transform.position, wanderRadius, -1));
            currentMaxSpeed = Random.Range(minSpeed, maxSpeed);
            _anim.SetBool(_fastID, !(currentMaxSpeed < slowFastThreshold));

            float wanderTimer = Random.Range(wanderTimerRange.x, wanderTimerRange.y);
            
            //Determine animation to play
            StartRunningAnimation();
            
            yield return new WaitForSeconds(wanderTimer);
        }
    }
    

    private void StartRunningAnimation()
    {
        _agent.nextPosition = transform.position;
        _anim.SetBool(_fastID, currentMaxSpeed >= slowFastThreshold);
        //_anim.SetBool(_runningID,true);
        _agent.speed = currentMaxSpeed;
        //StartCoroutine(Accelerate());
    }

    private IEnumerator Accelerate()
    {
        //TODO
        yield break;
    }
    
    private IEnumerator Decelerate()
    {
        decelerateRunning = true;
        while (_agent.speed > 0.01f)
        {
            _agent.speed = Mathf.Min(0f, _agent.speed - Time.deltaTime * AccelerationRate);
            yield return null;
        }
        decelerateRunning = false;
    }

    
    private Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection;
        NavMeshHit navHit;
        
        //If necessary, ensure that our randomized nav position is within the view.
        //Especially useful for the main focus point - it gets problematic if it leaves the viewport.
        if (limitToView)
        {
            int max = 0;
            while (max < 30)
            {
                Vector3 temp = new Vector3();
                temp.x = Random.Range(0.1f, 0.9f);
                temp.y = Random.Range(0.1f, 0.9f);
                Ray ray = _mainCamera.ViewportPointToRay(temp);
                RaycastHit hit;
                
                if (Physics.Raycast(ray, out hit))
                {
                    NavMesh.SamplePosition (hit.point, out navHit, dist, layermask);
                    return navHit.position;
                }
                //Failed - increment max counter.
                max++;
            }
        }
        
        //IF limitToView is off - or the algorithm fails - use this instead.
        randDirection = Random.insideUnitSphere * (dist + _agent.stoppingDistance) + origin;

        NavMesh.SamplePosition (randDirection, out navHit, dist, layermask);

        return navHit.position;
    }
}

