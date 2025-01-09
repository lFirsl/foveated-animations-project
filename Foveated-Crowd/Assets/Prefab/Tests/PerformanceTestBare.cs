using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using Unity.PerformanceTesting;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class PerformanceTestBare : MonoBehaviour
{
    
    static string[] scenes = new string[]
    {
        "Performance Wander - Base",
        "Performance Wander - Dynamic",
        "Performance Wander - Full Stop"
    };
    
    //Warmup and measure parameters
    private readonly int warmupFrames = 600;

    private readonly int measureFrames = 1000;
    // A Test behaves as an ordinary method
    [UnityTest, Performance]
    public IEnumerator PerformanceTest([ValueSource("scenes")] string scene)
    {
        SceneManager.LoadScene(scene, LoadSceneMode.Single);
        yield return new WaitForSeconds(80);
        yield return Measure.Frames()
            .WarmupCount(warmupFrames)
            .MeasurementCount(measureFrames)
            .Run();
    }
}
