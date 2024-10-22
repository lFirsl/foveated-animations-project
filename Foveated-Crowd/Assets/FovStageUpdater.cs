using System.Collections;
using UnityEngine;
using TMPro;


public class FovStageUpdater : MonoBehaviour
{
    public TMP_Text sign;

    public Experiment videoPlayer;
    // Start is called before the first frame update
    private void Start()
    {
        StartCoroutine(updateTextCoroutine());
    }

    private IEnumerator updateTextCoroutine()
    {
        yield return new WaitForSeconds(1f);
        while (true)
        {
            if (videoPlayer.isVideoPlaying())
            {
                updateText();
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void updateText()
    {
        sign.text = "Fov Stage = " + videoPlayer.timeToFoveationStage().ToString();
    }
}
