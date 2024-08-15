using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

namespace Tobii.XR
{
    public class XRInputSystemControllerAdapter : IControllerAdapter
    {
        public Vector3 Velocity =>
            _controller.TryGetFeatureValue(CommonUsages.deviceVelocity, out var deviceVelocity)
                ? deviceVelocity
                : Vector3.zero;

        public Vector3 AngularVelocity =>
            _controller.TryGetFeatureValue(CommonUsages.deviceAngularVelocity, out var deviceAngularVelocity)
                ? deviceAngularVelocity
                : Vector3.zero;

        public Vector3 Position =>
            _controller.TryGetFeatureValue(CommonUsages.devicePosition, out var devicePosition)
                ? devicePosition
                : Vector3.zero;

        public Quaternion Rotation =>
            _controller.TryGetFeatureValue(CommonUsages.deviceRotation, out var deviceRotation)
                ? deviceRotation
                : Quaternion.identity;

        public bool GetButtonPress(ControllerButton button)
        {
            // return _controllerStates.Find(x => x.Button == button).ButtonStateThisFrame;
            return _buttonMap[button].ButtonStateThisFrame;
        }

        public bool GetButtonPressDown(ControllerButton button)
        {
            return _buttonMap[button].ButtonDownThisFrame;
        }

        public bool GetButtonPressUp(ControllerButton button)
        {
            return _buttonMap[button].ButtonUpThisFrame;
        }

        public bool GetButtonTouch(ControllerButton button)
        {
            if (button == ControllerButton.Touchpad) return _buttonMap[ControllerButton.TouchpadTouch].ButtonStateThisFrame;

            // Not supported.
            return false;
        }

        public bool GetButtonTouchDown(ControllerButton button)
        {
            if (button == ControllerButton.Touchpad) return _buttonMap[ControllerButton.TouchpadTouch].ButtonDownThisFrame;

            // Not supported.
            return false;
        }

        public bool GetButtonTouchUp(ControllerButton button)
        {
            if (button == ControllerButton.Touchpad) return _buttonMap[ControllerButton.TouchpadTouch].ButtonUpThisFrame;

            // Not supported.
            return false;
        }

        public void TriggerHapticPulse(float hapticStrength)
        {
#if UNITY_ANDROID
            var str = Mathf.Clamp01(hapticStrength * 10);
            _controller.SendHapticImpulse(0, str, 0.05f);
#else
            _controller.SendHapticImpulse(0, Mathf.Clamp01(hapticStrength), 0.05f);
#endif
        }

        public Vector2 GetTouchpadAxis()
        {
            return _controller.TryGetFeatureValue(_touchAxisFeatureUsage, out var axis) ? axis : Vector2.zero;
        }

        public void Update()
        {
            GetDevice();
            if (!_capabilitiesInitialized) InitCaps();
            SetButtonStates();
        }

        private void InitCaps()
        {
            if (!_controller.isValid) return;

            var featureUsages = new List<InputFeatureUsage>();
            _controller.TryGetFeatureUsages(featureUsages);
            var featureUsageNames = featureUsages.Select(x => x.name).ToList();

            // If touchpad is considered secondary, remap touch from primary to secondary (MR controllers behaves like this in OpenXR)
            if (featureUsageNames.All(x => x != CommonUsages.primary2DAxisTouch.name))
            {
                if (featureUsageNames.Any(x => x == CommonUsages.secondary2DAxisTouch.name))
                {
                    _buttonMap[ControllerButton.Touchpad] = new ControllerState(CommonUsages.secondary2DAxisClick);
                    _buttonMap[ControllerButton.TouchpadTouch] = new ControllerState(CommonUsages.secondary2DAxisTouch);
                    _touchAxisFeatureUsage = CommonUsages.secondary2DAxis;
                }
            }

            _capabilitiesInitialized = true;
        }

        private InputDevice _controller;
        private bool _capabilitiesInitialized;
        private InputFeatureUsage<Vector2> _touchAxisFeatureUsage = CommonUsages.primary2DAxis;
        private readonly List<InputDevice> _inputDevices = new List<InputDevice>(2);

        private class ControllerState
        {
            public ControllerState(InputFeatureUsage<bool> featureUsage)
            {
                FeatureUsage = featureUsage;
                ButtonDownThisFrame = false;
                ButtonUpThisFrame = false;
                ButtonStateThisFrame = false;
            }

            public readonly InputFeatureUsage<bool> FeatureUsage;
            public bool ButtonDownThisFrame;
            public bool ButtonUpThisFrame;
            public bool ButtonStateThisFrame;
        }

        private readonly Dictionary<ControllerButton, ControllerState> _buttonMap =
            new Dictionary<ControllerButton, ControllerState>()
            {
                { ControllerButton.Trigger, new ControllerState(CommonUsages.triggerButton) },
                { ControllerButton.Menu, new ControllerState(CommonUsages.menuButton) },
                { ControllerButton.Touchpad, new ControllerState(CommonUsages.primary2DAxisClick) },
                { ControllerButton.TouchpadTouch, new ControllerState(CommonUsages.primary2DAxisTouch) }
            };

        private void GetDevice()
        {
            if (_controller.isValid) return;

            _capabilitiesInitialized = false;
            _inputDevices.Clear();
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Controller, _inputDevices);
            _controller = _inputDevices.Find(x => (x.characteristics.HasFlag(InputDeviceCharacteristics.Right)));
            if (!_controller.isValid)
            {
                _controller = _inputDevices.Find(x => (x.characteristics.HasFlag(InputDeviceCharacteristics.Left)));
            }
        }

        private void SetButtonStates()
        {
            foreach (var state in _buttonMap.Values)
            {
                if (!_controller.TryGetFeatureValue(state.FeatureUsage, out var buttonPressed)) continue;
                state.ButtonDownThisFrame = !state.ButtonStateThisFrame && buttonPressed;
                state.ButtonUpThisFrame = state.ButtonStateThisFrame && !buttonPressed;
                state.ButtonStateThisFrame = buttonPressed;
            }
        }
    }
}