
using System.Runtime.InteropServices;
using UnityEngine;
using ViveSR.anipal.Eye;

public class ReadEyeTrackingSample : MonoBehaviour, IReadEye
{
    


    private static EyeData_v2 eyeData = new EyeData_v2();
    private bool eye_callback_registered = false;
    private IReadEye.RayInfo rightRay;
    private IReadEye.RayInfo leftRay;
    // Start is called before the first frame update
    void Start()
    {
        if (!SRanipal_Eye_Framework.Instance.EnableEye)
        {
            enabled = false;
            return;
        }
    }

    public IReadEye.RayInfo getRightRay() { return rightRay; }
    public IReadEye.RayInfo getleftRay() { return leftRay; }

    // Update is called once per frame
    void Update()
    {
        if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
                        SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT) return;

        if (SRanipal_Eye_Framework.Instance.EnableEyeDataCallback == true && eye_callback_registered == false)
        {
            SRanipal_Eye_v2.WrapperRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye_v2.CallbackBasic)EyeCallback));
            eye_callback_registered = true;
        }
        else if (SRanipal_Eye_Framework.Instance.EnableEyeDataCallback == false && eye_callback_registered == true)
        {
            SRanipal_Eye_v2.WrapperUnRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye_v2.CallbackBasic)EyeCallback));
            eye_callback_registered = false;
        }

        if (eye_callback_registered) 
        {
            SRanipal_Eye_v2.GetGazeRay(GazeIndex.LEFT, out leftRay.pos, out leftRay.dir, eyeData);
            SRanipal_Eye_v2.GetGazeRay(GazeIndex.RIGHT, out rightRay.pos, out rightRay.dir, eyeData);
        }

    }
    private void Release()
    {
        if (eye_callback_registered == true)
        {
            SRanipal_Eye_v2.WrapperUnRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye_v2.CallbackBasic)EyeCallback));
            eye_callback_registered = false;
        }
    }

    private static void EyeCallback(ref EyeData_v2 eye_data)
    {
        eyeData = eye_data;
    }
}
