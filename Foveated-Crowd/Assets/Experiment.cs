using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Video;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;

public class Experiment : MonoBehaviour
{
    VideoPlayer vp;
    public TMP_Text instructions;
    private ushort numberOfScenes = 3;
    public VideoClip[] branch1Videos;
    public VideoClip[] branch2Videos;
    public VideoClip[] branch3Videos;

    
    // Video variables
    private uint _currentScene = 0;
    private uint _currentBranch = 0; 
    VideoClip[][] branches;
    
    //Time variables
    public double sceneTime = 10;
    private double _currentTime = 0;
    
    //Detection variables
    private bool _detectedOnce = false;
    private uint _detectedStage = 0;
    private double _detectionTime = 0;
    private uint[] _detectedStages;
    
    bool started = false;
    // Start is called before the first frame update
    void Start()
    {
        vp = GetComponent<VideoPlayer>();
        _detectedStages = new uint[numberOfScenes];
        vp.loopPointReached += EndReached;
        branches = new VideoClip[3][];
        branches[0] = branch1Videos;
        branches[1] = branch2Videos;
        branches[2] = branch3Videos;
        vp.Pause();
        vp.clip = branches[_currentBranch][_currentScene];
        vp.Pause();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space) && started)
        {
            
            if(_detectedOnce && _detectedStage == timeToFoveationStage())
            {
                vp.Pause();
                EndReached(vp);
                return;
            }

            _detectedOnce = true;
            _detectedStage = timeToFoveationStage();
            
            _currentTime = vp.time;
            _currentBranch = (_currentBranch + 1) % 3;
            StartCoroutine(prepareVideo());
        }
        else if (Input.GetKeyUp(KeyCode.Space) && !started)
        {
            instructions.enabled = false;
            started = true;
            vp.Play();
        }
        if (Input.GetKeyUp(KeyCode.A))
        {
            vp.Pause();
            vp.time = System.Math.Max(vp.time - 10, 0d);
            vp.Play();
            started = true;
        }
    }

    private IEnumerator prepareVideo()
    {
        vp.clip = branches[_currentBranch][_currentScene];

        vp.Prepare();
        while (!vp.isPrepared)
        {
            yield return null;
        }
        
        int integerDiv = System.Convert.ToInt32(Math.Floor((_currentTime - sceneTime) / sceneTime));
        Debug.Log("Integer Div is " + integerDiv + " with currentTime - sceneTime = " + (_currentTime - sceneTime));
        double newTime = System.Math.Max(integerDiv,0) * sceneTime;
        Debug.Log("New Time is set to " + newTime + " at stage " + timeToFoveationStage(newTime) + " from " + _currentTime);
        vp.time = newTime;
        started = false;
        vp.frame = vp.frame;
        vp.Play();
        yield return new WaitForSeconds(0.2f);
        instructions.enabled = true;
        vp.Pause();
    }

    void EndReached(UnityEngine.Video.VideoPlayer vp)
    {
        _detectedStages[_currentScene] = timeToFoveationStage();
        Debug.Log("Finished Scene " + _currentScene + " at stage " + _detectedStages[_currentScene] + ". Moving to next scene.");
        _currentScene++;
        _detectedOnce = false;
        if (_currentScene == branches[_currentBranch].Length)
        {
            FinishExperiment();
            started = false;
        }
        else
        {
            vp.time = 0;
            StartCoroutine(prepareVideo());
        }
    }
    void FinishExperiment()
    {
        print("we are done");
        
        resultsToCSV();
        
        //write to a csv file the result as in "they detected at this fov level"
        //load next scene
    }

    private uint timeToFoveationStage(double time = 0)
    {
        if(time == 0) return System.Convert.ToUInt32(Math.Floor(vp.time / sceneTime));
        else return System.Convert.ToUInt32(Math.Floor(time / sceneTime));
    }

    private void resultsToCSV()
    {

        // The target file path e.g.
#if UNITY_EDITOR
        var folder = Application.streamingAssetsPath;

        if(!Directory.Exists(folder)) Directory.CreateDirectory(folder);
#else
        var folder = Application.persistentDataPath;
#endif
        var filePath = Path.Combine(folder, "export.csv");
        bool fileExists = File.Exists(filePath);
        var sb = new StringBuilder();

        if (!fileExists)
        {
            for (uint x = 0; x < numberOfScenes; x++)
            {
                if (x != 0) sb.Append(",");
                sb.Append("Scene ").Append(x);
            }

            sb.Append("\n");
        }

        for (uint x = 0; x < numberOfScenes; x++)
        {
            if (x != 0) sb.Append(",");
            sb.Append(_detectedStages[x]);
        }
        sb.Append("\n");
        
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
}
