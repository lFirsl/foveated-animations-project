using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Serialization;
using UnityEngine.XR;
using ViveSR.anipal.Eye;


public class FocusPointSphere : MonoBehaviour
{
    [Header("Camera Controls")]
    public bool useMouseFocus = false;
    [SerializeField] private LayerMask _rayLayerMask;

    [Header("VR Controls")] 
    public bool useVR = false;
    private IReadEye _eyes;
    
    [Header("Foveation Thresholds in Normalized Screen Distance")]
    public float foveationFactor = 5f;
    public float foveaArea = 0.5f;
    [SerializeField] private LayerMask layermask;
    
    [Header("Animation Variables")]
    [SerializeField] private float animationsResetTime = 0.5f;
    [SerializeField] private uint cappedHz = 120;
    
    [Header("Debugging")] 
    [SerializeField] private bool debuggingMessages = false;
    [SerializeField] private bool displayFoveationLevels = false;
    [SerializeField] private bool displayHz = false;
    [SerializeField] private GameObject sphere;
    [SerializeField] private Material sphereMaterial;
    
    
    //private
    private FoveatedAnimationTarget[] _agentsFov;
    private Collider[] agents = new Collider[800];
    private static readonly Color tGrey = new Color(0.1f, 0.1f, 0.1f, 0.3f);
    private static readonly Color tGreen = new Color(0f, 1f, 0f, 0.3f);
    private static readonly Color tRed = new Color(1f, 0.1f, 0.1f, 0.3f);
    private static readonly Color tBlue = new Color(0f, 0f, 1f, 0.3f);
    
    // Get screen dimensions
    private float _screenHeight = Screen.height;
    private Vector2 _centreNSD = new Vector2();
    private Vector3 _centrePoint;
    private int _pixelThreshold = 0;
    
    //Objects
    private Transform _pos;
    private Camera _mainCamera;
    
    //etc
    private readonly float foveationStep = 0.025f;
    private readonly float foveationFactorStep = 0.2f;
    private bool useRightEye = false;
    
    void Start()
    {
        _pos = gameObject.GetComponent<Transform>();
        _mainCamera = Camera.main;
        if (debuggingMessages) Debug.Log("Started");

        _agentsFov = FindObjectsOfType<FoveatedAnimationTarget>();
        if (debuggingMessages) Debug.Log("Got " + _agentsFov.Length + " Foveated Animation Targets");

        _eyes = FindObjectsOfType<MonoBehaviour>(true).OfType<IReadEye>().FirstOrDefault();

    }


    private void Update()
    {
        //Buttons for changing foveation levels
        if (Input.GetKeyUp(KeyCode.Q)) foveationFactor = System.Math.Max(foveationFactor - foveationFactorStep, 0f);
        else if (Input.GetKeyUp(KeyCode.W)) foveationFactor = System.Math.Min(foveationFactor + foveationFactorStep, 3f);
        else if (Input.GetKeyUp(KeyCode.A)) foveaArea = System.Math.Max(foveaArea - foveationStep, 0f);
        else if (Input.GetKeyUp(KeyCode.S)) foveaArea = System.Math.Min(foveaArea + foveationStep, 1f);

        
        else if (Input.GetKeyUp(KeyCode.D))
        {
            displayFoveationLevels = !displayFoveationLevels;
        }
        else if (Input.GetKeyUp(KeyCode.F))
        {
            displayHz = !displayHz;
        }
        else if (Input.GetKeyUp(KeyCode.E))
        {
            useRightEye = !useRightEye;
        }
        
        
        // Cast a ray from the mouse position into the world
        Vector3 targetPosition;

        //If we're not using rays, center around game object instead
        if (useVR && Physics.Raycast(new Ray(
                _mainCamera.transform.position,
                _mainCamera.transform.localToWorldMatrix * (useRightEye ? _eyes.getRightRay().dir :_eyes.getleftRay().dir)), 
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
   
        if (useMouseFocus) _centrePoint = Input.mousePosition;
        else _centrePoint = _mainCamera.WorldToScreenPoint(targetPosition);
        _centreNSD = new Vector2(_centrePoint.x / _screenHeight, _centrePoint.y / _screenHeight);
        
        Profiler.BeginSample("AFC All Agents Foveation Calculation");
        foreach(FoveatedAnimationTarget agent in _agentsFov)
        {
            DetermineAnimation(agent);
        }
        Profiler.EndSample();
    }

    private void DetermineAnimation(FoveatedAnimationTarget agent)
    {
        Profiler.BeginSample("AFC Individual Agent Foveation Level Calculation");
        float distance = ScreenDistanceToCentre(agent.transform.position);
        try
        {
            if (!displayFoveationLevels && agent.isSphereActive()) agent.sphereSetActive(false);
            else if (displayFoveationLevels && !agent.isSphereActive()) agent.sphereSetActive(true);

            //No point calculating anything if it's in the foreground. Make the check.
            if (distance < foveaArea)
            {
                agent.RestartAnimation(animationsResetTime);
                agent.SetForegroundFPS(animationsResetTime);
                if (displayFoveationLevels) agent.setSphereColour(tGreen);
                return;
            }

            //Calculation for Dynamic HZ
            uint dynamicHz =
                (uint)Mathf.FloorToInt(
                    cappedHz 
                    / 
                    Mathf.Max(
                        1, 
                        foveationFactor * 
                        (
                            (distance * 10) * (distance * 10)
                        )
                        - foveaArea * 10
                    )
                );

            //If the dynamic frequency is under 5 FPS, there is no point keeping animations up anymore.
            if (dynamicHz < 5)
            {
                if (displayFoveationLevels && agent.isSphereActive()) agent.sphereSetActive(false);
                return;
            }
            //Restart animations - then set update frequency to the dynamicHZ.
            agent.RestartAnimation(animationsResetTime);
            agent.SetFixedFPS(dynamicHz, animationsResetTime);
            // Map value from 0 to 120 to a color range (red to blue)
            if (displayFoveationLevels) agent.setSphereColour(Color.Lerp(tGrey, tRed, (dynamicHz / (float)cappedHz)));
            if (displayHz)
            {
                //TODO: Add floating text
            }
            return;
        }
        finally
        {
            Profiler.EndSample();
        }
    }

    private float ScreenDistanceToCentre(Vector3 agent)
    {
        Vector3 agentScreenPos = _mainCamera.WorldToScreenPoint(agent);
        
        Vector2 normalizedPos1 = new Vector2(agentScreenPos.x / _screenHeight, agentScreenPos.y / _screenHeight);
        
        return Vector2.Distance(normalizedPos1, _centreNSD);
    }

    public bool UsingRighteye()
    {
        return useRightEye;
    }
}
