using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class CameraSphere : MonoBehaviour
{
    [Header("Foveation FPS")]
    [Tooltip("Determines whether foveated agents stop when outside of view, or use the outOfFocus FPS value instead.")]
    public bool shouldStop = true;
    [Tooltip("Animation frame rate for targets to use when outside of focus point's area. 0 makes targets use their own framerate instead.")]
    public uint outOfFocusFPS = 5;
    [FormerlySerializedAs("Stage2FoveationFPS")] public uint stage2FoveationFPS = 15;
    [FormerlySerializedAs("Stage1FoveationFPS")] public uint stage1FoveationFPS = 30; //If set to 0, it let's the target determine it's own FPS.
    [FormerlySerializedAs("_rayLayerMask")] [SerializeField] private LayerMask rayLayerMask;
    
    [Header("Foveation Thresholds")]
    public float stopThreshold = 0.4f;
    public float foveationThreshold = 0.2f;
    public float foveationThreshold2 = 0.3f;
    [SerializeField] private LayerMask layermask;
    
    [Header("Animation Variables")]
    [SerializeField] private float animationsResetTime = 0.5f;
    [SerializeField] private float animationsUpdateFrequency = 1f;
    
    //private
    private Collider[] _agents = new Collider[800];
    private static readonly Vector2 cameraFront = new Vector2(0.5f, 0);
    
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
            Ray ray = _mainCamera.ScreenPointToRay(new Vector3(Screen.width/2f,0));
            RaycastHit hit;
            Vector3 targetPosition = _pos.position;
            
            if(Physics.Raycast(ray, out hit,Mathf.Infinity,rayLayerMask))
            {
                // Get the point where the ray hits the ground
                targetPosition = hit.point;
            }
            
            yield return new WaitForFixedUpdate();
            int numOfAgents = Physics.OverlapSphereNonAlloc(targetPosition, ScreenToWorldRadius(targetPosition),_agents,layermask);
            //Debug.Log("Nr of Agents overlapping:"+numOfAgents);
            for(int i = 0; i < numOfAgents; i++)
            {
                FoveatedAnimationTarget agent = _agents[i].gameObject.GetComponent<FoveatedAnimationTarget>();
                DetermineAnimationCamera(agent,cameraFront);
            }
            yield return new WaitForSeconds(animationsUpdateFrequency);
        }
    }

    private void DetermineAnimationCamera(FoveatedAnimationTarget agent,Vector2 centre)
    {
        float distance = WorldToScreenDistance(agent.transform.position, centre);
        agent.RestartAnimation(animationsResetTime);
        if (distance > foveationThreshold2) agent.SetFixedFPS(stage2FoveationFPS, animationsResetTime);
        else if (distance > foveationThreshold) agent.SetFixedFPS(stage1FoveationFPS,animationsResetTime);
        else agent.SetForegroundFPS(animationsResetTime);
    }

    private float WorldToScreenDistance(Vector3 pos1, Vector2 centre)
    {
        // Get screen dimensions
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        
        Vector3 screenPos1 = _mainCamera.WorldToScreenPoint(pos1);
        
        Vector2 normalizedPos1 = new Vector2(screenPos1.x / screenWidth, screenPos1.y / screenHeight);
        
        return Vector2.Distance(normalizedPos1, centre);
    }
    
    private float ScreenToWorldRadius(Vector3 target)
    {
        // Calculate the distance from the camera to the center point
        float distanceToCenter = Vector3.Distance(_mainCamera.transform.position, target);
        Debug.Log("Distance is: "+distanceToCenter);

        // Calculate the world-space radius based on the distance from the camera
        float worldRadius = Mathf.Clamp(stopThreshold * distanceToCenter, 0f, 100f);
        //Debug.Log(worldRadius);
        return worldRadius;
    }
}
