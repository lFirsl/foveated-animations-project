using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Video;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class Experiment : MonoBehaviour
{
    VideoPlayer vp;
    public TMP_Text instructions;
    [SerializeField] public UnityEngine.UI.Button restartButton;
    public ushort numberOfScenes = 10;
    [FormerlySerializedAs("sceneTime")] public double stageTime = 10;
    public VideoClip[] branch1Videos;
    public VideoClip[] branch2Videos;
    public VideoClip[] branch3Videos;

    
    // Video variables
    private uint _currentScene = 0;
    private uint _currentBranch = 0; 
    VideoClip[][] branches;
    
    //Time variables
    private double _currentTime = 0;
    
    //Detection variables
    private bool _detectedOnce = false;
    private uint _detectedStage = 0;
    private double _detectionTime = 0;
    private uint[] _detectedStages;
    
    bool started = false;

    private bool finished = false;
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
        StartCoroutine(prepareVideo(true));
        
        restartButton.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (finished) return;
        
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
            instructions.text = "Take a second to locate the focus point, then press SPACE.";
            
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

    private IEnumerator prepareVideo(bool startAt0 = false)
    {
        vp.clip = branches[_currentBranch][_currentScene];

        vp.Prepare();
        while (!vp.isPrepared)
        {
            yield return null;
        }

        if (!startAt0)
        {
            int integerDiv = System.Convert.ToInt32(Math.Floor((_currentTime - stageTime) / stageTime));
            Debug.Log("Integer Div is " + integerDiv + " with currentTime - stageTime = " + (_currentTime - stageTime));
            double newTime = System.Math.Max(integerDiv,0) * stageTime;
            Debug.Log("New Time is set to " + newTime + " at stage " + timeToFoveationStage(newTime) + " from " + _currentTime);
            vp.time = newTime;
            vp.frame = vp.frame;
        }
        else
        {
            vp.time = 0;
            vp.frame = vp.frame;
        }
        started = false;
        vp.Play();
        yield return new WaitForSeconds(0.2f);
        instructions.enabled = true;
        vp.Pause();
    }

    void EndReached(UnityEngine.Video.VideoPlayer vp)
    {
        _detectedStages[_currentScene] = timeToFoveationStage();
        Debug.Log("Finished Scene " + _currentScene + " at stage " + _detectedStages[_currentScene] + ". Moving to next scene.");
        instructions.text = "NEXT SCENE. \n\n Take a second to find the focus point, then press SPACE.";
        _currentScene++;
        _detectedOnce = false;
        if (_currentScene == branches[_currentBranch].Length)
        {
            FinishExperiment();
            started = false;
        }
        else
        {
            StartCoroutine(prepareVideo(true));
        }
    }
    void FinishExperiment()
    {
        print("we are done");

        finished = true;
        instructions.text = "END OF TEST \n\n Thanks for taking part in our user test. Have a nice day!";
        instructions.enabled = true;
        resultsToCSV();
        
        restartButton.gameObject.SetActive(true);

        //write to a csv file the result as in "they detected at this fov level"
        //load next scene
    }

    private uint timeToFoveationStage(double time = 0)
    {
        if(time == 0) return System.Convert.ToUInt32(Math.Floor(vp.time / stageTime));
        else return System.Convert.ToUInt32(Math.Floor(time / stageTime));
    }

    public void restartScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

    private void resultsToCSV()
    {

        // The target file path e.g.
#if UNITY_EDITOR
        var folder = Application.streamingAssetsPath;

        if(!Directory.Exists(folder)) Directory.CreateDirectory(folder);
#else
        var folder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
#endif
        var filePath = Path.Combine(folder, "export.csv");
        bool fileExists = File.Exists(filePath);
        var sb = new StringBuilder();

        // If file doesn't exist, create it and add headers.
        if (!fileExists)
        {
            for (uint x = 1; x <= numberOfScenes; x++)
            {
                if (x != 1) sb.Append(",");
                sb.Append("Scene ").Append(x);
            }

            sb.Append("\n");
        }

        // Add this test's stage values
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
