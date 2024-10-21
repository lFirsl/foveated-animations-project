using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Stepladder : MonoBehaviour
{
    [SerializeField] private float factorTestThreshold = 1f;
    [SerializeField] private float tempFactor = 0.1f;
    [SerializeField] private float constatFoveaArea = 0.1f;
    // Start is called before the first frame update
    
    private IEnumerator stepladderProcedure()
    {
         // Temporarily commented out
        yield return new WaitForSeconds(20);
        FocusPointSphere focus = FindObjectOfType<FocusPointSphere>();
        focus.stopThreshold = 3f;
        focus.foveaArea = 2f;
        focus.foveationFactor = tempFactor;

        Debug.Log(focus);
        yield return new WaitForSeconds(10);
        focus.foveaArea = constatFoveaArea;
        focus.foveationFactor = tempFactor;
        while(focus.foveationFactor < factorTestThreshold)
        {
            Debug.Log("Increasing Foveation. Foveation Factor is at " + focus.foveationFactor);
            //If on the final step, give viewer more time to see if they can notice the foveation  in the last second.
            //if(focus.stopThreshold < factorTestThreshold) yield return new WaitForSeconds(10);
            yield return new WaitForSeconds(10);

            focus.foveationFactor += 0.1f;
        }
        yield break;
    }
    void Start()
    {
        StartCoroutine(stepladderProcedure());
    }
}

