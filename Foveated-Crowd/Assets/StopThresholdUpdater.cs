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
        yield return new WaitForSeconds(1);
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

        if (focusPoint.useVR)
        {
            sign.text = "Fov Factor = " + Math.Round(focusPoint.foveationFactor, 3) +
                        "; Fovea Area ≈  " + Math.Round(focusPoint.foveaArea, 3) +
                        "; FPS ≈  " + focusPoint.fpsEstimate() +
                        "; Using " + leftRightEye;
        }
        else
        {
            sign.text = "Fov Factor = " + Math.Round(focusPoint.foveationFactor, 3) +
                        "; Fovea Area ≈  " + Math.Round(focusPoint.foveaArea, 3) +
                        "; FPS ≈  " + focusPoint.fpsEstimate() +
                        "; Operations ≈ " + focusPoint.operationsPerSecond();
        }
        
    }
}
