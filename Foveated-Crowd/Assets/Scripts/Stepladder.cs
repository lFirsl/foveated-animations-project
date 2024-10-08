using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Stepladder : MonoBehaviour
{
    [SerializeField] private float stopTestThreshold = 0.15f;
    [SerializeField] private float tempStop = 0.6f;
    [SerializeField] private float tempThresh2 = 0.5f;
    [SerializeField] private float tempThresh = 0.4f;
    // Start is called before the first frame update
    
    private IEnumerator stepladderProcedure()
    {
        yield break;
        /*
         // Temporarily commented out
        yield return new WaitForSeconds(2);
        FocusPointSphere focus = FindObjectOfType<FocusPointSphere>();
        focus.stopThreshold = 1f;
        focus.foveationThreshold = 1f;
        focus.foveationThreshold2 = 1f;

        Debug.Log(focus);
        yield return new WaitForSeconds(10);
        while(focus.stopThreshold > stopTestThreshold)
        {

            focus.stopThreshold = tempStop;
            focus.foveationThreshold = tempThresh;
            focus.foveationThreshold2 = tempThresh2;
            Debug.Log("Increasing Foveation. Stop Threshold is at "+tempStop);
            //If on the final step, give viewer more time to see if they can notice the foveation  in the last second.
            //if(focus.stopThreshold < stopTestThreshold) yield return new WaitForSeconds(10);
            yield return new WaitForSeconds(10);

            tempStop -= 0.05f;
            if(tempThresh > stopTestThreshold - 0.05) tempThresh -= 0.05f;
            if(tempThresh2 > stopTestThreshold) tempThresh2 -= 0.05f;
        }
        */
    }
    void Start()
    {
        StartCoroutine(stepladderProcedure());
    }
}

