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
    private float[] _detectedNsd;

    private int _stage = 0;
    private bool _finished = false;
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
        StartCoroutine(prepareVideo(true));
        
        restartButton.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (_finished) return;

        if (_stage == 3)
        {
            _screenWidth = Screen.width;
            _screenHeight = Screen.height;
            Vector2 normalizedPos1 = new Vector2(_click[0].x / _screenWidth, _click[0].y / _screenHeight);
            Vector2 normalizedPos2 = new Vector2(_click[1].x / _screenWidth, _click[1].y / _screenHeight);

            clicksNsd = Vector2.Distance(normalizedPos1, normalizedPos2);
            Debug.Log("Calculated nsd is " + clicksNsd);
        }

        if (_stage is 1 or 2)
        {
            if (Input.GetMouseButtonUp(0))
            {
                Debug.Log("Going once, stage " + _stage);
                _click[_stage-1] = Input.mousePosition;
                if (_stage == 1)
                {
                    instructions.text = "Please click in the area where you think you saw foveation.";
                }
                _stage++;
            }
        }
        else if (!onExampleVideo)
        {
            if ((Input.GetKeyUp(KeyCode.Space) && vp.isPlaying) || _stage == 3)
            {
                if (_stage == 0)
                {
                    vp.Pause();
                    _stage++;
                    instructions.text = "Please click on the red agent.";
                    instructions.enabled = true;
                    return;
                }
                if (_stage == 3) _stage = 0;
                
                if(_detectedOnce && _detectedStage == timeToFoveationStage())
                {
                    vp.Pause();
                    EndReached(vp);
                    return;
                }

                _detectedOnce = true;
                _detectedStage = timeToFoveationStage();
                _detectedTimes[_currentScene * 2] = vp.time;
                _detectedNsd[_currentScene * 2] = clicksNsd;
                CaptureScreenshot((_currentScene * 2).ToString());
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
            _detectedNsd[_currentScene * 2 + 1] = clicksNsd;
            CaptureScreenshot((_currentScene * 2 + 1).ToString());
        }
        
        
        Debug.Log("Finished Scene " + _currentScene + " at _stage " + _detectedStages[_currentScene] + ". Moving to next scene.");
        instructions.text = "NEXT SCENE. \n\n Take a second to find the focus point, then press SPACE.";
        _currentScene++;
        _detectedOnce = false;
        clicksNsd = 0;
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
        var filePath = Path.Combine(folder, "foveationUserTest.csv");
        bool fileExists = File.Exists(filePath);
        var sb = new StringBuilder();

        // If file doesn't exist, create it and add headers.
        if (!fileExists)
        {
            for (uint x = 1; x <= numberOfScenes; x++)
            {
                if (x != 1) sb.Append(",");
                sb.Append("Scene ").Append(x).Append(" _stage,");
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
            sb.Append(_detectedStages[x]+",");
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
        AddRedDot(screenshot,_click[1],20,tRed);

        // Step 5: Save the screenshot to a PNG file
        byte[] bytes = screenshot.EncodeToPNG();
        string filePath = Path.Combine(Application.streamingAssetsPath, name + ".png");
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
        int centerX = (int)_click[1].x;
        int centerY = (int)_click[1].y;

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
