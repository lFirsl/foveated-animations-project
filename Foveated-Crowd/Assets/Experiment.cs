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
    public double sceneTime = 100;
    public VideoClip foveationExampleVideo;
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
    private uint[] _detectedStages;
    private double[] _detectedTimes;

    private bool finished = false;

    private bool onExampleVideo = false;
    // Start is called before the first frame update
    void Start()
    {
        vp = GetComponent<VideoPlayer>();
        _detectedStages = new uint[numberOfScenes];
        _detectedTimes = new double[numberOfScenes * 2];
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

        if (!onExampleVideo)
        {
            if (Input.GetKeyUp(KeyCode.Space) && vp.isPlaying)
            {
            
                if(_detectedOnce && _detectedStage == timeToFoveationStage())
                {
                    vp.Pause();
                    EndReached(vp);
                    return;
                }

                _detectedOnce = true;
                _detectedStage = timeToFoveationStage();
                _detectedTimes[_currentScene * 2] = vp.time;
                instructions.text = "Take a second to locate the focus point, then press SPACE.";
            
                _currentTime = vp.time;
                _currentBranch = (_currentBranch + 1) % 3;
                StartCoroutine(prepareVideo());
            }
            else if (Input.GetKeyUp(KeyCode.Space) && !vp.isPlaying)
            {
                instructions.enabled = false;
                vp.Play();
            }
            else if (Input.GetKeyUp(KeyCode.S))
            {
                if (vp.isPlaying)
                {
                    instructions.text = "Paused.";
                    instructions.enabled = true;
                    vp.Pause();
                }
                else
                {
                    instructions.enabled = false;
                    vp.Play();
                }
            } 
            else if (Input.GetKeyUp(KeyCode.R))
            {
                //Reset stage
                restartFoveationVideo();
            }   
        }
        if (Input.GetKeyUp(KeyCode.E))
        {
            playExampleVideo();
        }
    }

    private IEnumerator prepareVideo(bool startAt0 = false, bool playExample = false)
    {
        if (!playExample) vp.clip = branches[_currentBranch][_currentScene];
        else vp.clip = foveationExampleVideo;

        vp.Prepare();
        while (!vp.isPrepared)
        {
            yield return null;
        }

        if (!startAt0 && !playExample)
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
            vp.frame = 0;
        }
        vp.Play();
        if (!playExample)
        {
            vp.isLooping = false;
            yield return new WaitForSeconds(0.2f);
            instructions.enabled = true;
            vp.Pause();
        }
        else
        {
            vp.isLooping = true;
            instructions.enabled = false;
        }
    }

    void EndReached(UnityEngine.Video.VideoPlayer vp)
    {
        if (onExampleVideo)
        {
            StartCoroutine(prepareVideo(true, true));
            return;
        }

        if (vp.time > (sceneTime - 1))
        {
            _detectedStages[_currentScene] = System.Convert.ToUInt32(sceneTime/stageTime) + 1;
            _detectedTimes[_currentScene * 2] = sceneTime;
            _detectedTimes[_currentScene * 2 + 1] = sceneTime;
        }
        else
        {
            _detectedStages[_currentScene] = timeToFoveationStage();
            _detectedTimes[_currentScene * 2 + 1] = vp.time; 
        }
        
        
        Debug.Log("Finished Scene " + _currentScene + " at stage " + _detectedStages[_currentScene] + ". Moving to next scene.");
        instructions.text = "NEXT SCENE. \n\n Take a second to find the focus point, then press SPACE.";
        _currentScene++;
        _detectedOnce = false;
        if (_currentScene == branches[_currentBranch].Length)
        {
            FinishExperiment();
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

    private void playExampleVideo()
    {
        if (!onExampleVideo)
        {
            onExampleVideo = true;
            StartCoroutine(prepareVideo(true, true));
        }
        else
        {
            onExampleVideo = false;
            restartFoveationVideo();
        }
    }

    private void restartFoveationVideo()
    {
        //Reset stage
        if(vp.isPlaying) vp.Pause();
        instructions.text = "Take a second to locate the focus point, then press SPACE.";
        instructions.enabled = true;
        _detectedStage = 0;
        _detectedOnce = false;
        StartCoroutine(prepareVideo(true));
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
        var filePath = Path.Combine(folder, "foveationUserTest.csv");
        bool fileExists = File.Exists(filePath);
        var sb = new StringBuilder();

        // If file doesn't exist, create it and add headers.
        if (!fileExists)
        {
            for (uint x = 1; x <= numberOfScenes; x++)
            {
                if (x != 1) sb.Append(",");
                sb.Append("Scene ").Append(x).Append(" stage,");
                sb.Append("Scene ").Append(x).Append(" time 1,");
                sb.Append("Scene ").Append(x).Append(" time 2");
            }

            sb.Append("\n");
        }

        // Add this test's stage values
        for (uint x = 0; x < numberOfScenes; x++)
        {
            if (x != 0) sb.Append(",");
            sb.Append(_detectedStages[x]+",");
            sb.Append(_detectedTimes[x*2]+",");
            sb.Append(_detectedTimes[x*2+1]);
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
