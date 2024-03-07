using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using Unity.PerformanceTesting;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class PerformanceTestBare : MonoBehaviour
{
    private readonly String foveatedScene = "Basic Hundreds";
    private readonly String baseCase = "Basic Hundreds - Base";
    
    //Warmup and measure parameters
    private readonly int warmupFrames = 600;

    private readonly int measureFrames = 1000;
    // A Test behaves as an ordinary method
    [UnityTest, Performance]
    public IEnumerator BaseCaseTest()
    {
        SceneManager.LoadScene(baseCase, LoadSceneMode.Single);
        yield return Measure.Frames()
            .WarmupCount(warmupFrames)
            .MeasurementCount(measureFrames)
            .Run();
    }
    
    [UnityTest, Performance]
    public IEnumerator FoveatedCase()
    {
        SceneManager.LoadScene(foveatedScene, LoadSceneMode.Single);
        yield return Measure.Frames()
            .WarmupCount(warmupFrames)
            .MeasurementCount(measureFrames)
            .Run();
    }
}
