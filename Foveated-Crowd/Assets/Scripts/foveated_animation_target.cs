using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;

public class FoveatedAnimationTarget : MonoBehaviour
{
    //Components
    private Animator _anim;
    private NavMeshAgent _agent;
    
    //Public variables
    public int lowFpsFrames = 30;
    public float frameVariation = 0.10f;
    
    //Private variables
    private float _waitTime;
    
    //Animator specific variables
    private bool _lowFps = false;
    private float timeToStop;
    
    // Start is called before the first frame update
    void Start()
    {
        _anim = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();
        _waitTime = 1f / lowFpsFrames;
        StartCoroutine(LowFramerate());

        FocusPointSphere focus = GameObject.FindObjectOfType<FocusPointSphere>();
        if(focus != null || !focus.enabled) StopAnimation();
    }
    
    private IEnumerator AnimationStopTimer(float timer)
    {
        timeToStop = Time.time + timer;
        var currentTimer = timer;

        while (true)
        {
            yield return new WaitForSeconds(currentTimer);
            if (timeToStop > Time.time)
            {
                currentTimer = timeToStop - Time.time;
            }
            else
            {
                StopAnimation();
                timeToStop = 0;
                yield break;
            }
        }
    }
    
    private IEnumerator LowFramerate()
    {
        var lastTime = Time.time;

        while (true)
        {
            if (_lowFps)
            {
                _anim.playableGraph.Evaluate(Time.time - lastTime);
            }
            lastTime = Time.time;
            yield return new WaitForSeconds(_waitTime + Random.Range(-frameVariation,frameVariation));
            yield return new WaitUntil(() => _lowFps);
        }
    }

    public void SetForegroundFPS(float timer = 0)
    {
        //If lowFPS is true, then this was already applied. Don't do it again to avoid overhead.
        if(timer != 0) TimedStop(timer);
        if (!_lowFps) return;
        
        _lowFps = false;
        _anim.playableGraph.Stop();
        _anim.playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        _anim.playableGraph.Play();

        _agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
    }

    public void SetFixedFPS(uint fps = 0,float timer = 0)
    {
        //If lowFPS is false, then this was already applied. Don't do it again to avoid overhead.
        if(timer != 0) TimedStop(timer);
        if (_lowFps) return;

        if (fps == 0) _waitTime = 1f / lowFpsFrames;
        else _waitTime = 1f / fps;
        
        _lowFps = true;
        _anim.playableGraph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
    }

    public void StopAnimation()
    {
        _anim.enabled = false;
        _agent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
    }

    public void RestartAnimation(float timer = 0)
    {
        if(timer > 0) TimedStop(timer);
        _anim.enabled = true;
        _agent.obstacleAvoidanceType = ObstacleAvoidanceType.MedQualityObstacleAvoidance;
    }

    //Used when the animation is set for an automatic stop.
    private void TimedStop(float timer)
    {
        if (timeToStop > 0) timeToStop = Time.time + timer;
        else StartCoroutine(AnimationStopTimer(timer));
    }
}
