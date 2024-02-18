using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class goal_move : MonoBehaviour
{
    //Private components. Fetched during Start.
    private Animator _anim;
    private NavMeshAgent _agent;
    
    //Serialized Private Field

    [SerializeField] private float speed;
    [SerializeField] private float slowFastThreshold;
    [SerializeField] private int stopThreshold;
    [SerializeField] private int currentGoal = 0;
    [SerializeField] private GameObject[] goals;
    
    //Animator variables
    private int _runningID;
    private int _fastID;
    private const string RunningString = "Running";
    private const string FastString = "Fast";

    private int nrOfGoals;
    
    void Start()
    {
        _anim = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();
        _agent.SetDestination(goals[currentGoal].transform.position);
        _agent.speed = speed;
        nrOfGoals = goals.Length;
        
        _runningID = Animator.StringToHash(RunningString);
        _fastID = Animator.StringToHash(FastString);
        StartRunningAnimation();
    }

    private void FixedUpdate()
    {
        CheckDestinationReached();
    }
    
    private void StartRunningAnimation()
    {
        _anim.SetBool(_fastID, _agent.speed >= slowFastThreshold);
        _anim.SetBool(_runningID,true);
    }
    
    
    private void CheckDestinationReached()
    {
        if (!_agent.pathPending && _agent.remainingDistance < stopThreshold)
        {
            if (currentGoal < nrOfGoals - 1) currentGoal++;
            else currentGoal = 0;
            _agent.SetDestination(goals[currentGoal].transform.position);
        }
    }
}
