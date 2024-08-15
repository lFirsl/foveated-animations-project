// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using System.Linq;
using UnityEngine;
using UnityEngine.XR;

namespace Tobii.XR
{
    /// <summary>
    /// The goal with this class is to find a historical camera pose that can be used to convert non predicted eye
    /// tracking data from local space to world space with a minimum of temporal fusion errors.
    /// 
    /// At the time of writing this, when reading the XR pose for the head, it's predicted to display time and there is
    /// no way to get the current pose for the head. This class records a history of earlier poses so that you can
    /// find a historic pose that was predicted to a time that aligns with the time your eye tracking data was captured.
    /// This is not ideal, since we will depend on head pose prediction accuracy, but we believe it's good enough for
    /// all use cases that doesn't require millisecond precision on pose fusion. 
    /// </summary>
    public class CameraPoseHistory
    {
        private struct CameraPoseSample
        {
            public long TimestampUs;
            public Matrix4x4 Matrix;

            public static readonly CameraPoseSample AncientPose = new CameraPoseSample {TimestampUs = long.MinValue, Matrix = Matrix4x4.identity};
            public static readonly CameraPoseSample FuturePose = new CameraPoseSample {TimestampUs = long.MaxValue, Matrix = Matrix4x4.identity};
        }

        private const int SecsToUs = 1000000;
        private const int EstimatedEyeTrackerLatencyUs = 12000;
        private const float EstimatedEyeTrackerLatencySecs = EstimatedEyeTrackerLatencyUs / 1000000f;
        private readonly CameraPoseSample[] _history;
        private Transform _cameraTransform;
        private int _writeIndex = 0;
        private readonly long _headPosePredictionUs;

        /// <summary>
        /// Configures timings for recording camera poses.
        /// </summary>
        /// <param name="eyeTrackerLatencySecs">The average system latency for the eye tracking signal. This is only used when requesting camera pose without timestamp.</param>
        /// <param name="overrideHeadPosePredictionSecs">If this value is set, this value is used for head pose prediction time instead of calculating it.</param>
        /// <param name="scanoutTime">Scanout time is assumed to add one extra frame of prediction time. Setting this value overrides that estimation.</param>
        public CameraPoseHistory(float eyeTrackerLatencySecs = EstimatedEyeTrackerLatencySecs, float? overrideHeadPosePredictionSecs = null, float? scanoutTime = null)
        {
            var frameDurationSecs = 1 / (XRDevice.refreshRate > 1 ? XRDevice.refreshRate : 90);
            var headPosePredictionSecs = overrideHeadPosePredictionSecs.GetValueOrDefault(2 * frameDurationSecs + scanoutTime.GetValueOrDefault(frameDurationSecs));

            var eyeMotionToPhotonLatencySec = eyeTrackerLatencySecs + headPosePredictionSecs;
            var frameOffsetBetweenHeadPoseAndEyePose = Mathf.CeilToInt(eyeMotionToPhotonLatencySec / frameDurationSecs);
            _history = Enumerable.Repeat(CameraPoseSample.AncientPose, frameOffsetBetweenHeadPoseAndEyePose + 1).ToArray();
            _cameraTransform = CameraHelper.GetCameraTransform();
            _headPosePredictionUs = (long) (headPosePredictionSecs * SecsToUs);
        }

        /// <summary>
        /// Records the camera pose for the current frame.
        /// <param name="systemTimestampUs">A monotonic timestamp at the time of calling this method.</param>
        /// </summary>
        public void Tick(long systemTimestampUs)
        {
            // Calculate, in system time, when the frame started
            var timeSinceStartOfFrameUs = (long) (SecsToUs * (Time.realtimeSinceStartup - Time.unscaledTime));
            var timestampStartOfFrameUs = systemTimestampUs - timeSinceStartOfFrameUs;

            // Sample predicted camera pose and set timestamp as the predicted time 
            _history[_writeIndex].TimestampUs = timestampStartOfFrameUs + _headPosePredictionUs;
            _history[_writeIndex].Matrix = GetCameraLocalToWorldMatrix();
            _writeIndex = (_writeIndex + 1) % _history.Length;
        }

        /// <summary>
        /// Get the camera pose from the frame that matches the estimated eye tracker timestamp. If you want better
        /// precision you should use the overload that accepts an eye tracker timestamp. 
        /// </summary>
        /// <returns></returns>
        public Matrix4x4 GetLocalToWorldMatrix()
        {
            var historicPoseMatchingEyeTracking = _history[_writeIndex]; // The size of the buffer is set up so that the next value to be overwritten matches the eye tracking latency
            return historicPoseMatchingEyeTracking.TimestampUs == 0 ? GetCameraLocalToWorldMatrix() : historicPoseMatchingEyeTracking.Matrix;
        }

        public bool TryGetLocalToWorldMatrixFor(long timestampUs, out Matrix4x4 matrix)
        {
            var beforeSet = false;
            var afterSet = false;
            var before = CameraPoseSample.AncientPose;
            var after = CameraPoseSample.FuturePose;
            foreach (var sample in _history)
            {
                if (sample.TimestampUs < timestampUs && sample.TimestampUs > before.TimestampUs)
                {
                    before = sample;
                    beforeSet = true;
                }

                if (sample.TimestampUs >= timestampUs && sample.TimestampUs <= after.TimestampUs)
                {
                    after = sample;
                    afterSet = true;
                }
            }

            if (!beforeSet && !afterSet)
            {
                matrix = Matrix4x4.identity;
                return false;
            }

            if (beforeSet && afterSet)
            {
                var t = (float) (timestampUs - before.TimestampUs) / (float) (after.TimestampUs - before.TimestampUs);
                var translation = Vector3.Lerp(before.Matrix.MultiplyPoint3x4(Vector3.zero), after.Matrix.MultiplyPoint3x4(Vector3.zero), t);
                var rotation = Quaternion.Lerp(before.Matrix.rotation, after.Matrix.rotation, t);
                matrix = Matrix4x4.TRS(translation, rotation, Vector3.one);
            }
            else if (beforeSet) matrix = before.Matrix;
            else matrix = after.Matrix;

            return true;
        }

        private Matrix4x4 GetCameraLocalToWorldMatrix()
        {
            if (_cameraTransform != null && _cameraTransform.gameObject.activeInHierarchy)
            {
                return _cameraTransform.localToWorldMatrix;
            }

            UnityEngine.Debug.Log("Camera transform invalid. Trying to retrieve a new.");
            _cameraTransform = CameraHelper.GetCameraTransform();
            return _cameraTransform != null ? _cameraTransform.localToWorldMatrix : Matrix4x4.identity;
        }
    }
}