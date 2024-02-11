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
    
    //Private variables
    private float _waitTime;
    
    //Animator specific variables
    private bool _lowFps = false;
    
    // Start is called before the first frame update
    void Start()
    {
        _anim = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();
        _waitTime = 1f / lowFpsFrames;
        StartCoroutine(LowFramerate());
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
            yield return new WaitForSeconds(_waitTime);
        }
    }

    public void SetForegroundFPS()
    {
        //If lowFPS is true, then this was already applied. Don't do it again to avoid overhead.
        if (!_lowFps) return;
        
        _lowFps = false;
        _anim.playableGraph.Stop();
        _anim.playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        _anim.playableGraph.Play();
    }

    public void SetFixedFPS(uint fps = 0)
    {
        //If lowFPS is false, then this was already applied. Don't do it again to avoid overhead.
        if (_lowFps) return;

        if (fps == 0) _waitTime = 1f / lowFpsFrames;
        else _waitTime = 1f / fps;
        
        _lowFps = true;
        _anim.playableGraph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
    }

    public void StopAnimation()
    {
        _anim.enabled = false;
    }

    public void RestartAnimation()
    {
        _anim.enabled = true;
    }
}
