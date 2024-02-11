using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;

public class FocusPoint : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject[] crowdAgents;
    public List<Animator> crowdAnimators;
    
    //Private
    [SerializeField] private float foveationThreshold = 5;
    
    //Objects
    private Transform pos;
    void Start()
    {
        crowdAgents = GameObject.FindGameObjectsWithTag("CrowdAgent");
        pos = gameObject.GetComponent<Transform>();

        foreach (var agent in crowdAgents)
        {
            crowdAnimators.Add(agent.GetComponent<Animator>());
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        foreach (var agent in crowdAnimators)
        {
            if (Vector3.Distance(pos.position,agent.transform.position) < foveationThreshold)
            {
                agent.enabled = true;
            }
            else
                agent.enabled = false;
        }
    }
}
