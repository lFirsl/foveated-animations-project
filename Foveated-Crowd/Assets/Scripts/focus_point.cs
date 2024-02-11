using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;

public class FocusPoint : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject[] crowdAgents;
    public List<FoveatedAnimationTarget> crowdAnimators;
    
    //Private
    [SerializeField] private float stopThreshold = 10;
    [SerializeField] private float foveationThreshold = 5;
    [SerializeField] private float animationsUpdateFrequency = 1f;
    
    //Objects
    private Transform _pos;
    
    void Start()
    {
        crowdAgents = GameObject.FindGameObjectsWithTag("CrowdAgent");
        _pos = gameObject.GetComponent<Transform>();

        foreach (var agent in crowdAgents)
        {
            crowdAnimators.Add(agent.GetComponent<FoveatedAnimationTarget>());
        }

        StartCoroutine(UpdateAnimations());
    }

    private IEnumerator UpdateAnimations()
    {
        while (true)
        {
            foreach (var agent in crowdAnimators)
            {
                var distance = Vector3.Distance(_pos.position, agent.transform.position);
                if(distance > stopThreshold) agent.StopAnimation();
                else{
                    agent.RestartAnimation();
                    if(distance > foveationThreshold) agent.SetFixedFPS();
                    else agent.SetForegroundFPS();
                }
            }

            yield return new WaitForSeconds(animationsUpdateFrequency);
        }

        yield break;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        foreach (var agent in crowdAnimators)
        {
            float distance = Vector3.Distance(_pos.position, agent.transform.position);
            if(distance > stopThreshold) agent.StopAnimation();
            else{
                agent.RestartAnimation();
                if(distance > foveationThreshold) agent.SetFixedFPS();
                else agent.SetForegroundFPS();
            }
        }
    }
}
