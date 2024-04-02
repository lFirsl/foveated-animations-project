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
        sign.text = "Current Stop Threshold = " + Math.Round(focusPoint.stopThreshold,2);
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        sign.text = "Current Stop Threshold = " + Math.Round(focusPoint.stopThreshold,2);
    }
}
