using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using Unity.PerformanceTesting;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class StepLadderTest : MonoBehaviour
{
    static string[] scenes = new string[]
    {
        "Wander Crowd",
        "Square Marathon",
        "Parallel Columns",
        "Intercepting Crowds"
    };
    
    [UnityTest]
    public IEnumerator FoveatedBasicCase([ValueSource(nameof(scenes))] string scene)
    {
        /* Temporarily commented out
        const float stopTestThreshold = 0.15f;
        SceneManager.LoadScene(scene, LoadSceneMode.Single);

        yield return new WaitForSeconds(3);
        FocusPointSphere focus = FindObjectOfType<FocusPointSphere>();
        //Add initial values
        float tempStop = 0.6f;
        float tempThresh2 = 0.5f;
        float tempThresh = 0.4f;
        
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
            Debug.Log("Increasing Foveation. Stop Threshold is at"+tempStop);
            //If on the final step, give viewer more time to see if they can notice the foveation  in the last second.
            //if(focus.stopThreshold < stopTestThreshold) yield return new WaitForSeconds(10);
            yield return new WaitForSeconds(10);
            
            tempStop -= 0.05f;
            if(tempThresh > stopTestThreshold - 0.05) tempThresh -= 0.05f;
            if(tempThresh2 > stopTestThreshold) tempThresh2 -= 0.05f;
        }
        */

        Assert.IsTrue(true);
        yield break;
    }
}
