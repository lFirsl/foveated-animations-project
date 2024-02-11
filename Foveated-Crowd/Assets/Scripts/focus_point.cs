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
    
    //Objects
    private Transform pos;
    void Start()
    {
        crowdAgents = GameObject.FindGameObjectsWithTag("CrowdAgent");
        pos = gameObject.GetComponent<Transform>();

        foreach (var agent in crowdAgents)
        {
            crowdAnimators.Add(agent.GetComponent<FoveatedAnimationTarget>());
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        foreach (var agent in crowdAnimators)
        {
            float distance = Vector3.Distance(pos.position, agent.transform.position);
            if(distance > stopThreshold) agent.stopAnimation();
            else{
                agent.restartAnimation();
                if(distance > foveationThreshold) agent.setFixedFPS();
                else agent.setForegroundFPS();
            }
        }
    }
}
