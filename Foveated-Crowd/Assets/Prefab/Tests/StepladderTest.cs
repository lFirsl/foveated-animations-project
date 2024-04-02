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
        "Basic Hundreds",
        "Basic Hundreds - Walk Around",
        "Basic Hundreds - Opposite directions",
        "Basic Cross"
    };
    
    [UnityTest]
    public IEnumerator FoveatedBasicCase([ValueSource("scenes")] string scene)
    {
        const float stopTestThreshold = 0.15f;
        SceneManager.LoadScene(scene, LoadSceneMode.Single);

        yield return new WaitForSeconds(3);
        FocusPointSphere focus = FindObjectOfType<FocusPointSphere>();
        //Add initial values
        focus.stopThreshold = 1f;
        focus.foveationThreshold = 0.6f;
        focus.foveationThreshold2 = 0.8f;
        
        Debug.Log(focus);
        yield return new WaitForSeconds(10);
        while(focus.stopThreshold > stopTestThreshold)
        {
            focus.stopThreshold -= 0.05f;
            if(focus.foveationThreshold > stopTestThreshold - 0.05) focus.foveationThreshold -= 0.05f;
            if(focus.foveationThreshold2 > stopTestThreshold) focus.foveationThreshold2 -= 0.05f;
            Debug.Log("Increasing Foveation. Stop Threshold is at"+focus.stopThreshold);
            //If on the final step, give viewer more time to see if they can notice the foveation  in the last second.
            if(focus.stopThreshold < stopTestThreshold) yield return new WaitForSeconds(10);
            else yield return new WaitForSeconds(5);
        }

        Assert.IsTrue(true);
    }
}
