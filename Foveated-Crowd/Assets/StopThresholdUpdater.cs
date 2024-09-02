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
        StartCoroutine(updateTextCoroutine());
    }

    private IEnumerator updateTextCoroutine()
    {
        while (true)
        {
            updateText();
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void updateText()
    {
        String leftRightEye;
        if (focusPoint.UsingRighteye())
        {
            leftRightEye = "Right Eye";
        }
        else
        {
            leftRightEye = "Left Eye";
        }
        sign.text = "Fov1 =  " + Math.Round(focusPoint.foveationThreshold, 3) +
                    "; Fov2 = " + Math.Round(focusPoint.foveationThreshold2, 3) +
                    "; Stop = " + Math.Round(focusPoint.stopThreshold, 3) +
                    "; FPS ≈  " + (int)(1f / Time.deltaTime) +
                    "; Using " + leftRightEye;
    }
}
