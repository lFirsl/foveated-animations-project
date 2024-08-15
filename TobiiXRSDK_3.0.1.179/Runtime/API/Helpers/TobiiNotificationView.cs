using Tobii.XR;
using UnityEngine;
using UnityEngine.UI;

public class TobiiNotificationView : MonoBehaviour
{
    private Text _message;

    public static void Show(string message)
    {
        var go = GameObject.Find("/Tobii Notification View");
        if (go == null)
        {
            var prefab = Resources.Load("Tobii Notification View");
            go = (GameObject)Instantiate(prefab);    
        }

        var view = go.GetComponent<TobiiNotificationView>();

        
        view.SetMessage(message);
    }

    private void Awake()
    {
        var canvas = GetComponentInChildren<Canvas>();
        canvas.worldCamera = Camera.main;
        _message = transform.Find("Background/Message").GetComponent<Text>();
    }

    private void Update()
    {
        if (ControllerManager.Instance.AnyTriggerPressed()) gameObject.SetActive(false);
    }

    private void SetMessage(string message)
    {
        _message.text = message;
    }
}