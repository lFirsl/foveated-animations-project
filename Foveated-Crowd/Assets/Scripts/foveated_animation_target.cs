using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;
using UnityEngine.Profiling;

public class FoveatedAnimationTarget : MonoBehaviour
{
    //Components
    private Animator _anim;
    private NavMeshAgent _agent;
    private FocusPointSphere focus;
    
    //Public variables
    public int lowFpsFrames = 30;
    
    //Private variables
    private float _waitTime;
    [SerializeField] private uint currentFPS;
    
    //Animator specific variables
    [NonSerialized] public bool lowFps = false;
    private float timeToStop;
    
    [Header("Debugging")]
    [SerializeField] private GameObject sphere;
    private Material sphereMaterial;
    
    // Start is called before the first frame update
    void Start()
    {
        _anim = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();
        _waitTime = 1f / lowFpsFrames;
        sphereMaterial = sphere.GetComponent<MeshRenderer>().material;
        StartCoroutine(LowFramerate());

        focus = FindObjectOfType<FocusPointSphere>();
        if (focus != null && focus.enabled)
        {
            SetFixedFPS(5,5f); //Get everyone to move a bit into a position, then freeze them
        }
        else this.enabled = false;
    }
    
    private IEnumerator LowFramerate()
    {
        var lastTime = Time.time;

        while (true)
        {
            if (timeToStop !=0 && timeToStop < Time.time)
            {
                timeToStop = 0;
                StopAnimation();
            }

            if (!lowFps && !isAnimationEnabled())
            {
                yield return new WaitForSeconds(_waitTime);
                continue;
            }
            //Profiler.BeginSample("AFC Low FPS Animation");
            if (lowFps) _anim.playableGraph.Evaluate(Time.time - lastTime);
            lastTime = Time.time;
            //Profiler.EndSample();
            yield return new WaitForSeconds(_waitTime);
            yield return new WaitForFixedUpdate();
        }
    }

    public void SetForegroundFPS(float timer = 0)
    {
        //If lowFPS is true, then this was already applied. Don't do it again to avoid overhead. Just update the timer.
        if(timer != 0) TimedStop(timer);
        if (!lowFps) return;
        Profiler.BeginSample("AFC SetForegroundFPS");
        
        lowFps = false;
        _anim.playableGraph.Stop();
        _anim.playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        _anim.playableGraph.Play();

        Profiler.EndSample();
    }

    public void SetFixedFPS(uint fps = 0,float timer = 0)
    {
        if(timer != 0) TimedStop(timer);
        //If we're attempting to update the agent to the same framerate, we'd be doing needless work.
        //Instead, just update the timer and move on.
        if (timeToStop != 0 && timeToStop < Time.time && fps == currentFPS) return;
        Profiler.BeginSample("AFC SetFixedFPS");
        currentFPS = fps;

        if (fps == 0) _waitTime = 1f / lowFpsFrames;
        else _waitTime = 1f / currentFPS;
        
        lowFps = true;
        _anim.playableGraph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
        Profiler.EndSample();
    }

    public void StopAnimation()
    {
        _anim.enabled = false;
        currentFPS = 0;
    }

    public void RestartAnimation(float timer = 0)
    {
        Profiler.BeginSample("AFC RestartAnimation");
        if(timer > 0) TimedStop(timer);
        if (!_anim.enabled)
        {
            _anim.enabled = true;
        }
        Profiler.EndSample();
    }

    //Used when the animation is set for an automatic stop.
    private void TimedStop(float timer)
    {
        timeToStop = Time.time + timer;
    }

    public bool isAnimationEnabled()
    {
        return _anim.enabled;
    }

    public bool isSphereActive()
    {
        return sphere.activeSelf;
    }
    public void sphereSetActive(bool active)
    {
        sphere.SetActive(active);
    }
    public void setSphereColour(Color colour)
    {
        sphereMaterial.SetColor("_Color", colour);
    }
}
