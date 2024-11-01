using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Stepladder : MonoBehaviour
{
    [SerializeField] private float factorTestThreshold = 10f;
    [SerializeField] private float tempFactor = 0.5f;
    [SerializeField] private float constatFoveaArea = 0.1f;
    [SerializeField] private float factorIncrementStep = 0.25f;
    // Start is called before the first frame update
    
    private IEnumerator stepladderProcedure()
    {
        yield return new WaitForSeconds(1);
        FocusPointSphere focus = FindObjectOfType<FocusPointSphere>();
        focus.stopThreshold = 3f;
        focus.foveaArea = 2f;
        focus.foveationFactor = tempFactor;
        
        yield return new WaitForSeconds(9);
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

