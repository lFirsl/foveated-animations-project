using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StepladderPerformance : MonoBehaviour
{

    [Header("Stepladder Settings")]
    [SerializeField] bool dynamicFoveation = true;
    [SerializeField] private float initialValue = 0.5f;
    [SerializeField] private float factorStep = 0.2f;
    [SerializeField] private int numberOfSteps = 10;
    [SerializeField] private int noFoveationTime = 10;
    [SerializeField] private int stageTime = 10;
    
    [Header("Dynamic Foveation Settings")]
    [SerializeField] private float constatFoveaArea = 0.08f;
    
#if UNITY_EDITOR
    string folder = Application.streamingAssetsPath;
#else
    string folder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
#endif
    
    // Start is called before the first frame update
    
    private IEnumerator stepladderPerformanceProcedure()
    {
        yield return new WaitForSeconds(1);
        FocusPointSphere focus = FindObjectOfType<FocusPointSphere>();
        focus.foveaArea = 3f;
        focus.foveationFactor = 0.0001f;

        yield return new WaitForSeconds(Math.Max(1, noFoveationTime-1));
        
        int[] Frames = new int[numberOfSteps * stageTime];
        Debug.Log("Frames Length is " + Frames.Length);
        int f = 0;
        int[] Operations = new int[numberOfSteps * stageTime];
        Debug.Log("Operations Length is " + Operations.Length);
        int o = 0;
        

        int currentStep = 0;
        if (!dynamicFoveation)
        {
            focus.foveationFactor = float.PositiveInfinity;
            focus.foveaArea = initialValue;
        }
        else
        {
            focus.foveaArea = constatFoveaArea;
            focus.foveationFactor = initialValue;
        }
        
        while(currentStep++ < numberOfSteps)
        {
            if(dynamicFoveation) Debug.Log("Increasing Foveation. Foveation Factor is at " + focus.foveationFactor);
            else Debug.Log("Increasing Foveation. Fovea Area is at " + focus.foveaArea);
            int currentSecond = 0;
            while (currentSecond++ < stageTime)
            {
                Debug.Log(f);
                yield return new WaitForSeconds(1);
                Frames[f++] = focus.fpsEstimate();
                Operations[o++] = focus.operationsPerSecond();
            }
            //If on the final step, give viewer more time to see if they can notice the foveation  in the last second.
            //if(focus.stopThreshold < factorTestThreshold) yield return new WaitForSeconds(10);
            yield return new WaitForSeconds(10);

            if(dynamicFoveation) focus.foveationFactor += factorStep;
            else focus.foveaArea -= factorStep;
        }
        resultsToCSV(Frames, Operations);
        yield break; 
        
    }
    
    private void resultsToCSV(int[] Frames,int[] Operations)
    {

        // The target file path e.g.
        var filePath = Path.Combine(folder, "Performance Test Results.csv");
        bool fileExists = File.Exists(filePath);
        var sb = new StringBuilder();
        if(fileExists) File.Delete(filePath);

        
        // Add this test's _stage values
        for (uint x = 0; x < Frames.Length; x++)
        {
            if (x != 0) sb.Append(",");
            sb.AppendFormat(Frames[x].ToString());
        }
        sb.Append("\n");
        for (uint x = 0; x < Operations.Length; x++)
        {
            if (x != 0) sb.Append(",");
            sb.AppendFormat(Operations[x].ToString());
        }
        
        using(var writer = new StreamWriter(filePath, true))
        {
            Debug.Log(sb.ToString());
            writer.Write(sb.ToString());
        }

        // Or just

        Debug.Log($"CSV file written to \"{filePath}\"");

#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }
    void Start()
    {
        StartCoroutine(stepladderPerformanceProcedure());
    }
}

