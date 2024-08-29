using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using ViveSR.anipal.Eye;


public class FocusPointSphere : MonoBehaviour
{
    [Header("Camera Controls")]
    public bool useMouseFocus = false;
    [SerializeField] private LayerMask _rayLayerMask;

    [Header("VR Controls")] 
    public bool useVR = false;
    private IReadEye _eyes;
    
    [FormerlySerializedAs("shouldStop")]
    [Header("Foveated Animation Update Frequencies")]
    [Tooltip("Determines whether foveated agents stop when outside of the Stop Threshold, or animate at the Minimum Stop Hz frequency.")]
    public bool useHaltStop = true;
    [FormerlySerializedAs("Stage1FoveationFPS")] [Tooltip("Animation frame rate for targets to use when outside of focus point's area. 0 makes targets use their own framerate instead.")]
    public uint Stage1FoveationHz = 30; //If set to 0, it let's the target determine it's own FPS.
    [FormerlySerializedAs("Stage2FoveationFPS")] 
    public uint Stage2FoveationHz = 15;
    [FormerlySerializedAs("outOfFocusFPS")]
    public uint MinimumStopHz = 5;
    
    [Header("Foveation Thresholds in Normalized Screen Distance")]
    public float foveationThreshold = 0.2f;
    public float foveationThreshold2 = 0.3f;
    public float stopThreshold = 0.4f;
    [SerializeField] private LayerMask layermask;
    
    [Header("Animation Variables")]
    [SerializeField] private float animationsResetTime = 0.5f;
    
    [Header("Debugging")] 
    [SerializeField] private bool debuggingMessages = false;
    [SerializeField] private bool displayFoveationLevels = false;
    [SerializeField] private GameObject sphere;
    [SerializeField] private Material sphereMaterial;
    
    
    //private
    private FoveatedAnimationTarget[] _agentsFov;
    private Collider[] agents = new Collider[800];
    
    // Get screen dimensions
    private float _screenWidth = Screen.width;
    private float _screenHeight = Screen.height;
    private Vector2 _centreNSD = new Vector2();
    
    //Objects
    private Transform _pos;
    private Camera _mainCamera;
    
    void Start()
    {
        _pos = gameObject.GetComponent<Transform>();
        _mainCamera = Camera.main;
        if (debuggingMessages) Debug.Log("Started");

        _agentsFov = FindObjectsOfType<FoveatedAnimationTarget>();

        _eyes = FindObjectsOfType<MonoBehaviour>(true).OfType<IReadEye>().FirstOrDefault();

    }


    private void Update()
    {
        //Buttons for changing foveation levels
        if (Input.GetKeyUp(KeyCode.Q)) foveationThreshold = System.Math.Max(foveationThreshold - 0.05f,0);
        else if (Input.GetKeyUp(KeyCode.W)) foveationThreshold = System.Math.Min(foveationThreshold + 0.05f, 1f);
        else if (Input.GetKeyUp(KeyCode.A)) foveationThreshold2 = System.Math.Max(foveationThreshold2 - 0.05f,0);
        else if (Input.GetKeyUp(KeyCode.S)) foveationThreshold2 = System.Math.Min(foveationThreshold2 + 0.05f, 1f);
        else if (Input.GetKeyUp(KeyCode.Z)) stopThreshold = System.Math.Max(stopThreshold - 0.05f,0);
        else if (Input.GetKeyUp(KeyCode.X)) stopThreshold = System.Math.Min(stopThreshold + 0.05f, 1f);
        else if (Input.GetKeyUp(KeyCode.D))
        {
            displayFoveationLevels = !displayFoveationLevels;
        }
        
        
        // Cast a ray from the mouse position into the world
        Vector3 targetPosition;

        //If we're not using rays, center around game object instead
        if (useVR && Physics.Raycast(new Ray(
                _mainCamera.transform.position,
                _mainCamera.transform.localToWorldMatrix * _eyes.getleftRay().dir), 
                out RaycastHit vrHit,Mathf.Infinity,
                _rayLayerMask))
        {
            if (debuggingMessages) Debug.Log("Got a hit with the VR! Location:" + vrHit.point);
            targetPosition = vrHit.point;
        }
        // Check if the ray hits something in the world
        else if (useMouseFocus && Physics.Raycast(_mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit,Mathf.Infinity,_rayLayerMask))
        {
            // Get the point where the ray hits the ground
            targetPosition = hit.point;
        }
        else
        {
            targetPosition = transform.position;
        }
        
        //Setup
        _screenHeight = Screen.height;
        _screenWidth = Screen.width;
        Vector3 centrePoint;
        if (useMouseFocus) centrePoint = Input.mousePosition;
        else centrePoint = _mainCamera.WorldToScreenPoint(targetPosition);
        _centreNSD = new Vector2(centrePoint.x / _screenWidth, centrePoint.y / _screenHeight);
        
        foreach(FoveatedAnimationTarget agent in _agentsFov)
        {
            DetermineAnimation(agent);
        }
    }

    private void DetermineAnimation(FoveatedAnimationTarget agent)
    {
        float distance = ScreenDistanceToCentre(agent.transform.position);
        
        //Outside our stop threshold. Skip other checks
        if (distance > stopThreshold)
        {
            if(displayFoveationLevels && agent.isSphereActive()) agent.sphereSetActive(false);
            return;
        }
        if(!displayFoveationLevels && agent.isSphereActive()) agent.sphereSetActive(false); 
        else if(displayFoveationLevels && !agent.isSphereActive()) agent.sphereSetActive(true);
        
        //Restart animations - then determine the update frequency rate.
        agent.RestartAnimation(animationsResetTime);
        if (distance > foveationThreshold2)
        {
            agent.SetFixedFPS(Stage2FoveationHz, animationsResetTime);
            if(displayFoveationLevels) agent.setSphereColour(new Color(0f, 0f, 1f, 0.3f));
        }
        else if (distance > foveationThreshold)
        {
            agent.SetFixedFPS(Stage1FoveationHz,animationsResetTime);
            if(displayFoveationLevels) agent.setSphereColour(new Color(0f, 1f, 0f, 0.3f));
        }
        else
        {
            agent.SetForegroundFPS(animationsResetTime);
            if(displayFoveationLevels) agent.setSphereColour(new Color(1f, 0f, 0f, 0.3f));
        }
    }

    private float ScreenDistanceToCentre(Vector3 agent)
    {
        Vector3 agentScreenPos = _mainCamera.WorldToScreenPoint(agent);
        
        Vector2 normalizedPos1 = new Vector2(agentScreenPos.x / _screenWidth, agentScreenPos.y / _screenHeight);
        
        return Vector2.Distance(normalizedPos1, _centreNSD);
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!EditorApplication.isPlaying) return;
        foreach(FoveatedAnimationTarget agent in _agentsFov)
        {
            if (agent.isAnimationEnabled() && !agent.lowFps)
            {
                Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            }
            else if (agent.currentFPS == Stage1FoveationHz)
            {
                Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            }
            else if (agent.currentFPS == Stage2FoveationHz)
            {
                Gizmos.color = new Color(0f, 0f, 1f, 0.3f);
            }
            else continue;
            Gizmos.DrawSphere(agent.transform.position, 1);
        }
    }
#endif
}
