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
using UnityEngine.XR;

public class Experiment : MonoBehaviour
{
    [Header("General Settings")]
    VideoPlayer vp;
    public TMP_Text instructions;
    [SerializeField] public UnityEngine.UI.Button restartButton;
    public ushort numberOfScenes = 10;
    public double stageTime = 10;
    public float errorMargin = 0.002f;
    [FormerlySerializedAs("sceneTime")] public double sceneTimeDynamic = 120;
    public double sceneTimeFullStop = 70;
    
    [Header("Tutorial Settings")]
    public bool tutorial = true;

    public VideoClip[] tutorialClips;
    public String[] tutorialClipInstructions;
    private uint tutorialStage = 0;
    
    [Header("Foveation Level Variables - Full Stop")]
    public float FullStopFoveationStart = 0.3f;
    public float FullStopFoveationStep = 0.05f;
    
    [Header("Foveation Level Variables - Dynamic")]
    public float DynamicFoveationFoveaArea = 0.05f;
    public float DynamicFoveationFactorStart = 0.1f;
    public float DynamicFoveationFactorStep = 0.2f;
    
    [Header("Video Variables")]
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
    private float[] _detectedNsd;

    private int _clickEventStage = 0;
    private bool _finished = false;
    private bool _fullStop = true;
    private Vector2[] _click = {new Vector2(0, 0),new Vector2(0,0)};
    private float clicksNsd = 0;
    
    private float _screenWidth = Screen.width;
    private float _screenHeight = Screen.height;
    private static readonly Color tRed = new Color(1f, 0.1f, 0.1f, 0.3f);
    
#if UNITY_EDITOR
    string folder = Application.streamingAssetsPath;
#else
    string folder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
#endif

    private bool onExampleVideo = false;
    // Start is called before the first frame update
    void Start()
    {
        vp = GetComponent<VideoPlayer>();
        if(!Directory.Exists(folder)) Directory.CreateDirectory(folder);
        _detectedStages = new uint[numberOfScenes];
        _detectedTimes = new double[numberOfScenes * 2];
        _detectedNsd = new float[numberOfScenes * 2];
        vp.loopPointReached += EndReached;
        branches = new VideoClip[3][];
        branches[0] = branch1Videos;
        branches[1] = branch2Videos;
        branches[2] = branch3Videos;
        vp.Pause();
        if (tutorial) instructions.text = tutorialClipInstructions[tutorialStage];
        
        StartCoroutine(prepareVideo(true));
        restartButton.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (_finished) return;
        if (tutorial && tutorialStage < tutorialClips.Length)
        {
            if (Input.GetKeyUp(KeyCode.Space))
            {
                Debug.Log("Going to next step of tutorial: step "+tutorialStage);
                tutorialStage++;
                if (tutorialStage >= tutorialClips.Length)
                {
                    tutorial = false;
                    instructions.text = 
                        "Welcome to the Foveated Animation User Test." +
                        "\n\nYou will be going through 10 scenes. You'll know when you move to the next scene." +
                        "\n\nPress SPACE to signal you've seen foveation happening. When you do, the video will change then pause to give you a second to locate the focus point. At this point, press SPACE again to re-start the video." +
                        "\n\nYou may start. Take a second to locate the focus point, then press SPACE.";
                }
                else instructions.text = tutorialClipInstructions[tutorialStage];
                StartCoroutine(prepareVideo());
            }
            return;
        }
        if (_clickEventStage == 3)
        {
            _screenWidth = Screen.width;
            _screenHeight = Screen.height;
            Vector2 normalizedPos1 = new Vector2(_click[0].x / _screenWidth, _click[0].y / _screenHeight);
            Vector2 normalizedPos2 = new Vector2(_click[1].x / _screenWidth, _click[1].y / _screenHeight);

            clicksNsd = Vector2.Distance(normalizedPos1, normalizedPos2);
            if (
                (!_fullStop & clicksNsd < (DynamicFoveationFoveaArea + errorMargin))
                ||
                (_fullStop & clicksNsd < (FullStopFoveationStart + errorMargin - FullStopFoveationStep * timeToFoveationStage()))
            )
            {
                // Clicked on an area with NO foveation. Ignore last click.
                Debug.Log("Clicked in control area!" + clicksNsd + " < " + (FullStopFoveationStart - FullStopFoveationStep) +". Ignoring.");
                clicksNsd = -1;
                _detectedOnce = false;
            }
            Debug.Log("Calculated nsd is " + clicksNsd);
        }

        if (_clickEventStage is 1 or 2)
        {
            if (Input.GetMouseButtonUp(0))
            {
                Debug.Log("Going once, stage " + _clickEventStage);
                _click[_clickEventStage-1] = Input.mousePosition;
                if (_clickEventStage == 1)
                {
                    instructions.text = "Please click in the area where you think you saw foveation.";
                }
                _clickEventStage++;
            }
        }
        else if (!onExampleVideo)
        {
            if ((Input.GetKeyUp(KeyCode.Space) && vp.isPlaying) || _clickEventStage == 3)
            {
                if (vp.time < stageTime)
                {
                    Debug.Log("Pressed during Control Stage. Restart!");
                    instructions.text = "THERE WAS NO FOVEATION IN EFFECT.\n Ignoring that event. Please try again. \n\n Take a second to locate the focus point, then press SPACE.";
                    _currentTime = vp.time; 
                    _currentBranch = (_currentBranch + 1) % 3;
                    _detectedOnce = false;
                    StartCoroutine(prepareVideo());
                    return;
                }
                if (_clickEventStage == 0)
                {
                    vp.Pause();
                    _clickEventStage++;
                    instructions.text = "Please click on the red agent.";
                    instructions.enabled = true;
                    return;
                }
                if (_clickEventStage == 3) _clickEventStage = 0;
                
                if(_detectedOnce && _detectedStage == timeToFoveationStage() && clicksNsd != -1)
                {
                    vp.Pause();
                    EndReached(vp);
                    return;
                }

                _detectedOnce = true;
                _detectedStage = timeToFoveationStage();
                _detectedTimes[_currentScene * 2] = vp.time;
                _detectedNsd[_currentScene * 2] = clicksNsd;
                if (clicksNsd == -1)
                {
                    _detectedOnce = false;
                    instructions.text = "THERE WAS NO FOVEATION IN EFFECT THERE.\n Ignoring that click. \n\n Take a second to locate the focus point, then press SPACE.";
                }

                else
                {
                    CaptureScreenshot((_currentScene * 2).ToString());
                    instructions.text = "Take a second to locate the focus point, then press SPACE.";
                }
                
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
                //Reset _stage
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
        if (tutorial) vp.clip = tutorialClips[tutorialStage];
        else if (playExample) vp.clip = foveationExampleVideo;
        else vp.clip = branches[_currentBranch][_currentScene];

        vp.Prepare();
        while (!vp.isPrepared)
        {
            yield return null;
        }

        if (!startAt0 && !playExample && !tutorial)
        {
            int integerDiv = System.Convert.ToInt32(Math.Floor((_currentTime - stageTime) / stageTime));
            Debug.Log("Integer Div is " + integerDiv + " with currentTime - stageTime = " + (_currentTime - stageTime));
            double newTime = System.Math.Max(integerDiv,0) * stageTime;
            Debug.Log("New Time is set to " + newTime + " at _stage " + timeToFoveationStage(newTime) + " from " + _currentTime);
            vp.time = newTime;
            vp.frame = vp.frame;
        }
        else
        {
            vp.time = 0;
            vp.frame = 0;
        }
        vp.Play();
        if (playExample)
        {
            vp.isLooping = true;
            instructions.enabled = false;
        }
        else if (tutorial)
        {
            vp.isLooping = true;
            instructions.enabled = true;
        }
        else
        {
            vp.isLooping = false;
            yield return new WaitForSeconds(0.2f);
            instructions.enabled = true;
            vp.Pause();
        }
    }

    void EndReached(UnityEngine.Video.VideoPlayer vp)
    {
        if (tutorial) return;
        if (onExampleVideo)
        {
            StartCoroutine(prepareVideo(true, true));
            return;
        }

        if ((!_fullStop && vp.time > (sceneTimeDynamic - 1)) || (_fullStop && vp.time > (sceneTimeFullStop - 1)))
        {
            Debug.Log("Going through logic for videos that ended by themselves.");
            double timeToUse = (_fullStop ? sceneTimeFullStop : sceneTimeDynamic);
            _detectedStages[_currentScene] = timeToFoveationStage(timeToUse-1) + 1;
            _detectedTimes[_currentScene * 2] = timeToUse;
            _detectedTimes[_currentScene * 2 + 1] = timeToUse;
        }
        else
        {
            _detectedStages[_currentScene] = timeToFoveationStage();
            _detectedTimes[_currentScene * 2 + 1] = vp.time;
            _detectedNsd[_currentScene * 2 + 1] = clicksNsd;
            CaptureScreenshot((_currentScene * 2 + 1).ToString());
        }
        
        Debug.Log("Finished Scene " + _currentScene + " at _stage " + _detectedStages[_currentScene] + ". Moving to next scene.");

        _currentScene++;
        _detectedOnce = false;
        clicksNsd = 0;
        if (_fullStop && _currentScene >= 5)
        {
            Debug.Log("Changing foveation with new message? At scene " + _currentScene);
            _fullStop = false;
            instructions.text = "NEXT SCENE. \n\n You'll be repeating the same scenes, but with a different foveation system. \n\n Take a second to find the focus point, then press SPACE.";
        }
        else
        {
            Debug.Log("Moving on. At scene " + _currentScene);
            instructions.text = "NEXT SCENE. \n\n Take a second to find the focus point, then press SPACE.";
        }

        
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

        _finished = true;
        instructions.text = "END OF TEST \n\n Thanks for taking part in our user test. Have a nice day!";
        instructions.enabled = true;
        resultsToCSV();
        
        restartButton.gameObject.SetActive(true);

        //write to a csv file the result as in "they detected at this fov level"
        //load next scene
    }

    public uint timeToFoveationStage(double time = 0)
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
        //Reset _stage
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
        var filePath = Path.Combine(folder, "AAA foveationUserTest.csv");
        bool fileExists = File.Exists(filePath);
        var sb = new StringBuilder();

        // If file doesn't exist, create it and add headers.
        if (!fileExists)
        {
            for (uint x = 1; x <= numberOfScenes; x++)
            {
                if (x != 1) sb.Append(",");
                sb.Append("Scene ").Append(x).Append(" _stage,");
                sb.Append("Scene ").Append(x).Append(" type,");
                sb.Append("Scene ").Append(x).Append(" time 1,");
                sb.Append("Scene ").Append(x).Append(" time 2,");
                sb.Append("Scene ").Append(x).Append("nsd 1,");
                sb.Append("Scene ").Append(x).Append("nsd 2");
            }

            sb.Append("\n");
        }

        // Add this test's _stage values
        for (uint x = 0; x < numberOfScenes; x++)
        {
            if (x != 0) sb.Append(",");
            if (x < 5)
            {
                // This is a Full Stop Stage
                sb.Append(Math.Max(0,FullStopFoveationStart+FullStopFoveationStep - FullStopFoveationStep * _detectedStages[x])+",");
                sb.Append("Full Stop,");
            }
            else
            {
                // This is a Dynamic Foveation Stage
                sb.Append((DynamicFoveationFactorStart + DynamicFoveationFactorStep * _detectedStages[x])+",");
                sb.Append("Dynamic,");
            }
            sb.Append(_detectedTimes[x*2]+",");
            sb.Append(_detectedTimes[x*2+1]+",");
            sb.Append(_detectedNsd[x*2]+",");
            sb.Append(_detectedNsd[x*2+1]);
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

    public bool isVideoPlaying()
    {
        return vp.isPlaying;
    }
    
    //ChatGPT-made helper function
    public void CaptureScreenshot(string name)
    {
        // Step 1: Create a new RenderTexture
        RenderTexture renderTexture = new RenderTexture(Screen.width, Screen.height, 32);
        
        // Step 2: Set the camera's target texture to the RenderTexture
        Camera.main.targetTexture = renderTexture;

        // Step 3: Render the camera's view to the RenderTexture
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = renderTexture;
        Camera.main.Render();

        // Step 4: Create a Texture2D to store the RenderTexture data
        Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGBA32, false);
        screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenshot.Apply();
        
        // Add Red Dot
        AddRedDot(screenshot,_click[0],20,tRed);
        AddRedDot(screenshot,_click[1],20,tRed);

        // Step 5: Save the screenshot to a PNG file
        byte[] bytes = screenshot.EncodeToPNG();
        string filePath = Path.Combine(folder, Path.Combine("Screenshots\\",name + "_"+ DateTime.Now.ToString("yy-MM-dd-hh-mm") + "_" + clicksNsd + ".png"));
        Directory.CreateDirectory(Path.Combine(folder,"Screenshots\\"));
        File.WriteAllBytes(filePath, bytes);

        // Debug Log
        Debug.Log("Screenshot saved to: " + filePath);

        // Step 6: Cleanup
        Camera.main.targetTexture = null;
        RenderTexture.active = currentRT;
        Destroy(renderTexture);
    }
    
    //ChatGPT-made helper function
    void AddRedDot(Texture2D texture, Vector2 position, int radius, Color color)
    {
        // Convert normalized (0-1) coordinates to pixel coordinates
        int centerX = (int)position.x;
        int centerY = (int)position.y;

        // Loop through pixels in the circle's area
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                // Check if the pixel is inside the circle (Pythagorean theorem)
                if (x * x + y * y <= radius * radius)
                {
                    int px = centerX + x;
                    int py = centerY + y;

                    // Ensure the pixel is within bounds of the texture
                    if (px >= 0 && px < texture.width && py >= 0 && py < texture.height)
                    {
                        // Get the current pixel color and blend it with the transparent red
                        Color currentColor = texture.GetPixel(px, py);
                        Color blendedColor = Color.Lerp(currentColor, color, color.a); // Blend based on alpha
                        texture.SetPixel(px, py, blendedColor);
                    }
                }
            }
        }

        // Apply the changes to the texture
        texture.Apply();
    }
    
}
