using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;

public class FoveatedAnimationTarget : MonoBehaviour
{
    //Components
    private Animator _anim;
    private NavMeshAgent _agent;
    private FocusPointSphere focus;
    
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

        focus = FindObjectOfType<FocusPointSphere>();
        if (focus != null && focus.enabled)
        {
            if (focus.shouldStop)
            {
                SetFixedFPS(focus.outOfFocusFPS,2 + Random.Range(-frameVariation,frameVariation)); //Get everyone to move a bit into a position, then freeze them
            }
            else
            {
                SetFixedFPS(focus.outOfFocusFPS);
            }
        }
        else this.enabled = false;
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
                timeToStop = 0;
                if(focus.shouldStop) StopAnimation();
                else SetFixedFPS(focus.outOfFocusFPS);
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
            while(!_lowFps) yield return new WaitForSeconds(_waitTime + Random.Range(-frameVariation,frameVariation));
            yield return new WaitForFixedUpdate();
        }
    }

    public void SetForegroundFPS(float timer = 0)
    {
        //If lowFPS is true, then this was already applied. Don't do it again to avoid overhead. Just update the timer.
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
        /*//If we're currently rendering foreground on a timer, let that run out first.
        if (_lowFps == false && Time.time > timeToStop) return;
        
        //If we're currently rendering with a higher FPS, let that run out first
        if ((1f / (fps + frameVariation)) > _waitTime && Time.time > timeToStop) return;*/
        if(timer != 0) TimedStop(timer);
        
        float fpsToUse = fps + Random.Range(-frameVariation, frameVariation); // Add some randomization to avoid popping.

        if (fps == 0) _waitTime = 1f / lowFpsFrames;
        else _waitTime = 1f / fpsToUse;
        
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
