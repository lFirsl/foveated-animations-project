using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;

public class FoveatedAnimationTarget : MonoBehaviour
{
    //Components
    private Animator anim;
    private NavMeshAgent agent;
    
    //Public variables
    public int lowFpsFrames = 30;
    
    //Private variables
    private float waitTime;
    
    //Animator specific variables
    private bool playing = true;
    private bool lowFps = false;
    
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        waitTime = 1f / lowFpsFrames;
        StartCoroutine(LowFramerate());
    }

    // Update is called once per frame
    void Update()
    {

        /*if (Input.GetKeyDown(KeyCode.Space))
        {
            if (playing)
            {
                anim.StopPlayback();
                playing = false;
            }
            else
            {
                anim.StartPlayback();
                playing = true;
            }
        }*/
        if (Input.GetKeyDown(KeyCode.F))
        {
            flipFPS();
        }
    }
    
    private IEnumerator LowFramerate()
    {
        var lastTime = Time.time;

        while (true)
        {
            if (lowFps)
            {
                anim.playableGraph.Evaluate(Time.time - lastTime);
            }
            lastTime = Time.time;
            yield return new WaitForSeconds(waitTime);
        }
    }
    
    private void flipFPS()
    {
        if (lowFps)
        {
            setForegroundFPS();
        }
        else
        {
            setFixedFPS();
        }
    }

    public void setForegroundFPS()
    {
        //If lowFPS is true, then this was already applied. Don't do it again to avoid overhead.
        if (!lowFps) return;
        
        lowFps = false;
        anim.playableGraph.Stop();
        anim.playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        anim.playableGraph.Play();
    }

    public void setFixedFPS(uint FPS = 0)
    {
        //If lowFPS is false, then this was already applies. Don't do it again to avoid overhead.
        if (lowFps) return;

        if (FPS == 0) waitTime = 1f / lowFpsFrames;
        else waitTime = 1f / FPS;
        
        lowFps = true;
        anim.playableGraph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
    }

    public void stopAnimation()
    {
        anim.enabled = false;
    }

    public void restartAnimation()
    {
        anim.enabled = true;
    }
}
