using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;


public class StopThresholdUpdater : MonoBehaviour
{
    public TMP_Text sign;

    public FocusPointSphere focusPoint;
    // Start is called before the first frame update
    private void Start()
    {
        updateText();
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        updateText();
    }

    private void updateText()
    {
        sign.text = "Stop = " + Math.Round(focusPoint.stopThreshold,2) + 
                    "; Fov1 = " + Math.Round(focusPoint.foveationThreshold,2) +
                    "; Fov2 = " + Math.Round(focusPoint.foveationThreshold2, 2);
    }
}
