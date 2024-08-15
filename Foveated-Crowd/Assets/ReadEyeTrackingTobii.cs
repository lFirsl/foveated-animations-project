
using System.Runtime.InteropServices;
using Tobii.XR;
using Tobii.XR.GazeModifier;
using UnityEngine;

public class ReadEyeTrackingTobii : MonoBehaviour, IReadEye
{
    IReadEye.RayInfo singleRay;
    public IReadEye.RayInfo getRightRay() { return singleRay; }
    public IReadEye.RayInfo getleftRay() { return singleRay; }

    void Start()
    {

    }

    void Update()
    {
        var provider = TobiiXR.Internal.Provider;
        var eyeTrackingData = new TobiiXR_EyeTrackingData();
        provider.GetEyeTrackingDataLocal(eyeTrackingData);

       // var localToWorldMatrix = provider.LocalToWorldMatrix;
       // var worldForward = localToWorldMatrix.MultiplyVector(Vector3.forward);
       // EyeTrackingDataHelper.TransformGazeData(eyeTrackingData, localToWorldMatrix);

        var gazeModifierFilter = TobiiXR.Internal.Filter as GazeModifierFilter;
        if (gazeModifierFilter != null) gazeModifierFilter.FilterAccuracyOnly(eyeTrackingData, Vector3.forward);

       
        var gazeRay = eyeTrackingData.GazeRay;
        if (gazeRay.IsValid) { 
            singleRay.pos = gazeRay.Origin;
            singleRay.dir = gazeRay.Direction;
        }
    }
}
