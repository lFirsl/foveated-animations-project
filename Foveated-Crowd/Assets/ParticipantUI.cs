using System.Collections;
using TMPro;
using UnityEngine;

public class ParticipantUI : MonoBehaviour
{
    private TMP_Text text;
    public Experiment experiment;
    
    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TMP_Text>();
        StartCoroutine(updateTextCoroutine());
    }

    // Update is called once per frame
    private IEnumerator updateTextCoroutine()
    {
        while (true)
        {
            if (!experiment.tutorial)
            {
                text.gameObject.SetActive(false);
            }
            updateText();
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void updateText()
    {
        text.text = "Participant Number = " + experiment.participantNr;
    }
}

