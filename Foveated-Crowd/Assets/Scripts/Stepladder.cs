using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Stepladder : MonoBehaviour
{
    [SerializeField] private float factorTestThreshold = 10f;
    [SerializeField] private float tempFactor = 0.1f;
    [SerializeField] private float constatFoveaArea = 0.1f;
    [SerializeField] private float factorIncrementStep = 0.25f;
    [SerializeField] private int noFoveationTime = 20;
    // Start is called before the first frame update
    
    private IEnumerator stepladderProcedure()
    {
        yield return new WaitForSeconds(1);
        FocusPointSphere focus = FindObjectOfType<FocusPointSphere>();
        focus.stopThreshold = 3f;
        focus.foveaArea = 3f;
        focus.foveationFactor = 0.0001f;
        
        yield return new WaitForSeconds(Math.Max(1, noFoveationTime-1));
        focus.foveaArea = constatFoveaArea;
        focus.foveationFactor = tempFactor;
        while(focus.foveationFactor < factorTestThreshold)
        {
            Debug.Log("Increasing Foveation. Foveation Factor is at " + focus.foveationFactor);
            //If on the final step, give viewer more time to see if they can notice the foveation  in the last second.
            //if(focus.stopThreshold < factorTestThreshold) yield return new WaitForSeconds(10);
            yield return new WaitForSeconds(10);

            focus.foveationFactor += factorIncrementStep;
        }
        yield break;
    }
    void Start()
    {
        StartCoroutine(stepladderProcedure());
    }
}

