using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using Unity.PerformanceTesting;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class PerformanceTest : MonoBehaviour
{
    private readonly String testScene = "City Hundreds";
    // A Test behaves as an ordinary method
    private GameObject protagonist;
    [UnityTest, Performance]
    public IEnumerator BaseCaseTest()
    {
        protagonist = GameObject.FindGameObjectWithTag("Protagonist");
        Debug.Log("Game object lookout returned:", protagonist);
        FocusPointSphere focus = protagonist.GetComponent<FocusPointSphere>();
        focus.enabled = false;
        yield return Measure.Frames()
            .WarmupCount(240)
            .MeasurementCount(1000)
            .Run();
    }
    
    [UnityTest, Performance]
    public IEnumerator FoveatedCase()
    {
        GameObject protagonist = GameObject.FindGameObjectWithTag("Protagonist");
        Debug.Log("Game object lookout returned:", protagonist);
        protagonist.SetActive(true);
        yield return Measure.Frames()
            .WarmupCount(240)
            .MeasurementCount(1000)
            .Run();
    }

    [SetUp]
    public void SetUp()
    {
        SceneManager.LoadScene(testScene, LoadSceneMode.Single);
    }
}
