using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


public class FocusPointSphere : MonoBehaviour
{
    [Header("Camera Controls")]
    public bool useMouseFocus = false;
    [SerializeField] private LayerMask _rayLayerMask;

    [Header("VR Controls")] 
    public bool useVR = false;
    [SerializeField] private ReadEyeTrackingSample _eyes;
    
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
    [SerializeField] private float animationsUpdateFrequency = 1f;
    
    //private
    private Collider[] agents = new Collider[800];
    private int segments = 36;
    
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
            RaycastHit hit,vrHit;
            Vector3 targetPosition;

            //If we're not using rays, center around game object instead
            if (useVR && Physics.Raycast(new Ray(
                    _mainCamera.transform.position,
                    _mainCamera.transform.localToWorldMatrix * _eyes.getleftRay().dir), 
                    out vrHit,Mathf.Infinity,
                    _rayLayerMask))
            {
                Debug.Log("Got a hit with the VR! Location:" + vrHit.point);
                targetPosition = vrHit.point;
            }
            // Check if the ray hits something in the world
            else if (useMouseFocus && Physics.Raycast(GetComponent<Camera>().ScreenPointToRay(Input.mousePosition), out hit,Mathf.Infinity,_rayLayerMask))
            {
                // Get the point where the ray hits the ground
                targetPosition = hit.point;
            }
            else
            {
                targetPosition = transform.position;
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
        if (distance > foveationThreshold2) agent.SetFixedFPS(Stage2FoveationHz, animationsResetTime);
        else if (distance > foveationThreshold) agent.SetFixedFPS(Stage1FoveationHz,animationsResetTime);
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

    private float ScreenToWorldRadius(Vector3 target,float threshold = -1.0f)
    {
        // Calculate the distance from the camera to the center point
        float distanceToCenter = Vector3.Distance(_mainCamera.transform.position, target);

        if (threshold == -1.0f) threshold = stopThreshold;
        // Calculate the world-space radius based on the distance from the camera
        float worldRadius = Mathf.Clamp(threshold * distanceToCenter, 0f, 100f);
        //Debug.Log(worldRadius);
        return worldRadius;
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);;
        Gizmos.DrawSphere(transform.position, ScreenToWorldRadius(transform.position));
        
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);;
        Gizmos.DrawSphere(transform.position, ScreenToWorldRadius(transform.position,foveationThreshold));
        
        Gizmos.color = new Color(0f, 0f, 1f, 0.3f);;
        Gizmos.DrawSphere(transform.position, ScreenToWorldRadius(transform.position,foveationThreshold2));
    }
}
