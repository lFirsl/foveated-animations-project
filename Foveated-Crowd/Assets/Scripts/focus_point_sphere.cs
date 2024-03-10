using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class FocusPointSphere : MonoBehaviour
{
    [Header("Camera Controls")]
    public bool useMouseFocus = false;
    [SerializeField] private Camera camera;
    [SerializeField] private LayerMask _rayLayerMask;
    
    [Header("Foveation FPS")]
    [Tooltip("Determines whether foveated agents stop when outside of view, or use the outOfFocus FPS value instead.")]
    public bool shouldStop = true;
    [Tooltip("Animation frame rate for targets to use when outside of focus point's area. 0 makes targets use their own framerate instead.")]
    public uint outOfFocusFPS = 5;
    public uint Stage2FoveationFPS = 15;
    public uint Stage1FoveationFPS = 30; //If set to 0, it let's the target determine it's own FPS.
    
    [Header("Foveation Thresholds")]
    [SerializeField] private float stopThreshold = 10;
    [SerializeField] private float foveationThreshold = 5;
    [SerializeField] private float foveationThreshold2 = 10;
    [SerializeField] private LayerMask layermask;
    
    [Header("Animation Variables")]
    [SerializeField] private float animationsResetTime = 0.5f;
    [SerializeField] private float animationsUpdateFrequency = 1f;
    
    //private
    private Collider[] agents = new Collider[800];
    
    //Objects
    private Transform _pos;
    private Camera _mainCamera;
    
    void Start()
    {
        _pos = gameObject.GetComponent<Transform>();
        _mainCamera = Camera.main;
        Debug.Log("Started");

        StartCoroutine(UpdateAnimations());
    }
    

    private IEnumerator UpdateAnimations()
    {
        while (true)
        {
            // Cast a ray from the mouse position into the world
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Vector3 targetPosition;

            //If we're not using rays, center around game object instead
            if (!useMouseFocus) targetPosition = _pos.position;
            // Check if the ray hits something in the world
            else if (Physics.Raycast(ray, out hit,Mathf.Infinity,_rayLayerMask))
            {
                // Get the point where the ray hits the ground
                targetPosition = hit.point;
            }
            else
            {
                targetPosition = Input.mousePosition;
            }
            yield return new WaitForFixedUpdate();
            int numOfAgents = Physics.OverlapSphereNonAlloc(targetPosition, ScreenToWorldRadius(targetPosition),agents,layermask);
            //Debug.Log("Nr of Agents overlapping:"+numOfAgents);
            for(int i = 0; i < numOfAgents; i++)
            {
                FoveatedAnimationTarget agent = agents[i].gameObject.GetComponent<FoveatedAnimationTarget>();
                DetermineAnimation(agent,targetPosition);
            }
            yield return new WaitForSeconds(animationsUpdateFrequency);
        }
    }

    private void DetermineAnimation(FoveatedAnimationTarget agent,Vector3 centre)
    {
        float distance = WorldToScreenDistance(agent.transform.position, centre);
        agent.RestartAnimation(animationsResetTime);
        if (distance > foveationThreshold2) agent.SetFixedFPS(Stage2FoveationFPS, animationsResetTime);
        else if (distance > foveationThreshold) agent.SetFixedFPS(Stage1FoveationFPS,animationsResetTime);
        else agent.SetForegroundFPS(animationsResetTime);
    }

    private float WorldToScreenDistance(Vector3 pos1, Vector3 pos2)
    {
        // Get screen dimensions
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        
        Vector3 screenPos1 = _mainCamera.WorldToScreenPoint(pos1);
        Vector3 screenPos2 = _mainCamera.WorldToScreenPoint(pos2);
        
        Vector2 normalizedPos1 = new Vector2(screenPos1.x / screenWidth, screenPos1.y / screenHeight);
        Vector2 normalizedPos2 = new Vector2(screenPos2.x / screenWidth, screenPos2.y / screenHeight);
        
        return Vector2.Distance(normalizedPos1, normalizedPos2);
    }

    private float ScreenToWorldRadius(Vector3 target)
    {
        // Calculate the distance from the camera to the center point
        float distanceToCenter = Vector3.Distance(_mainCamera.transform.position, target);

        // Calculate the world-space radius based on the distance from the camera
        float worldRadius = Mathf.Clamp(stopThreshold * distanceToCenter, 0f, 100f);
        //Debug.Log(worldRadius);
        return worldRadius;
    }
}
