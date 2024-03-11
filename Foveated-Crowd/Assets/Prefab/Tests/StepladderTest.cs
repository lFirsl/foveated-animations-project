using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using Unity.PerformanceTesting;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class StepLadderTest : MonoBehaviour
{
    private readonly String foveatedScene = "Basic Hundreds";
    
    [UnityTest]
    public IEnumerator FoveatedCase()
    {
        SceneManager.LoadScene(foveatedScene, LoadSceneMode.Single);

        yield return new WaitForSeconds(3);
        FocusPointSphere focus = FindObjectOfType<FocusPointSphere>();
        Debug.Log(focus);
        for (int x = 0; x < 10; x++)
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
