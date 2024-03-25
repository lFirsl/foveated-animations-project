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
        "Basic Hundreds - Opposite directions"
    };
    
    [UnityTest]
    public IEnumerator FoveatedBasicCase([ValueSource("scenes")] string scene)
    {
        SceneManager.LoadScene(scene, LoadSceneMode.Single);

        yield return new WaitForSeconds(3);
        FocusPointSphere focus = FindObjectOfType<FocusPointSphere>();
        //Add initial values
        focus.stopThreshold = 1f;
        focus.foveationThreshold = 0.5f;
        focus.foveationThreshold2 = 0.8f;
        
        Debug.Log(focus);
        for (int x = 0; x < 20; x++)
        {
            yield return new WaitForSeconds(5);
            focus.stopThreshold -= 0.05f;
            focus.foveationThreshold -= 0.05f;
            focus.foveationThreshold2 -= 0.05f;
            Debug.Log("Increasing Foveation. Stop Threshold is at"+focus.stopThreshold);
        }

        Assert.IsTrue(true);
    }
}
