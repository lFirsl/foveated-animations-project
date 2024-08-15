using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Tobii.StreamEngine;

namespace Tobii.XR
{
    public class ConnectResult
    {
        public bool Connected = false;
        public readonly List<tobii_license_validation_result_t> LicenseValidationResults = new List<tobii_license_validation_result_t>();
    }
    
    public static class ConnectionHelper
    {
        private static readonly tobii_device_url_receiver_t _deviceUrlReceiver = DeviceUrlReceiver; // Needed to prevent GC from removing callback
        private static readonly Stopwatch _stopwatch = new Stopwatch();
        
        public static ConnectResult TryConnect(IStreamEngineInterop interop, StreamEngineTracker_Description description, out StreamEngineContext context, tobii_custom_log_t customLog = null)
        {
            _stopwatch.Reset();
            _stopwatch.Start();

            var connectResult = new ConnectResult();
            context = null;
            if (CreateApiContext(interop, out var apiContext, customLog) == false) return connectResult;

            try
            {
                // Get a list of all eye trackers
                if (GetAvailableTrackers(interop, apiContext, out var connectedDevices) == false)
                {
                    DestroyApiContext(interop, apiContext);
                    return connectResult;
                }

                // Connect to the first supported eye tracker
                connectResult = GetFirstSupportedTracker(interop, apiContext, connectedDevices, description, out var deviceContext, out var hmdEyeTrackerUrl);
                if (!connectResult.Connected)
                {
                    DestroyApiContext(interop, apiContext);
                    return connectResult;
                }

                context = new StreamEngineContext(apiContext, deviceContext, hmdEyeTrackerUrl);
                _stopwatch.Stop();
                UnityEngine.Debug.Log($"Connected to eye tracker: {context.Url} and it took {_stopwatch.ElapsedMilliseconds}ms");
                return connectResult;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("Error connecting to eye tracker: " + e.ToString());
                return connectResult;
            }
        }

        public static void Disconnect(IStreamEngineInterop interop, StreamEngineContext context)
        {
            if (context == null) return;

            DestroyDeviceContext(interop, context.Device);
            DestroyApiContext(interop, context.Api);
        }

        private static ConnectResult CreateDeviceContext(IStreamEngineInterop interop, string url, Interop.tobii_field_of_use_t fieldOfUse, IntPtr apiContext, string[] licenseKeys, out IntPtr deviceContext)
        {
            var connectResult = new ConnectResult();
            
            // Connect without license
            if (licenseKeys == null || licenseKeys.Length == 0)
            {
                connectResult.Connected = interop.tobii_device_create(apiContext, url, fieldOfUse, out deviceContext) == tobii_error_t.TOBII_ERROR_NO_ERROR;
                return connectResult;
            }

            // Connect with license
            var result = interop.tobii_device_create_ex(apiContext, url, fieldOfUse, licenseKeys, connectResult.LicenseValidationResults, out deviceContext);
            if (result == tobii_error_t.TOBII_ERROR_NO_ERROR)
            {
                connectResult.Connected = true;
            }
            else
            {
                UnityEngine.Debug.LogError($"Failed to create device context for {url}. {result}");
                return connectResult;
            }

            // Print validation errors in the log
            for (var i = 0; i < licenseKeys.Length; i++)
            {
                var licenseResult = connectResult.LicenseValidationResults[i];
                if (licenseResult == tobii_license_validation_result_t.TOBII_LICENSE_VALIDATION_RESULT_OK) continue;

                UnityEngine.Debug.LogError("License " + i + " failed. Return code " + licenseResult);
            }

            return connectResult;
        }

        private static void DestroyDeviceContext(IStreamEngineInterop interop, IntPtr deviceContext)
        {
            if (deviceContext == IntPtr.Zero) return;

            var result = interop.tobii_device_destroy(deviceContext);
            if (result != tobii_error_t.TOBII_ERROR_NO_ERROR)
            {
                UnityEngine.Debug.LogError($"Failed to destroy device context. Error {result}");
            }
        }

        private static bool CreateApiContext(IStreamEngineInterop interop, out IntPtr apiContext, tobii_custom_log_t customLog = null)
        {
            var result = interop.tobii_api_create(out apiContext, customLog);
            if (result == tobii_error_t.TOBII_ERROR_NO_ERROR) return true;

            UnityEngine.Debug.LogError("Failed to create api context. " + result);
            apiContext = IntPtr.Zero;
            return false;
        }

        private static void DestroyApiContext(IStreamEngineInterop interop, IntPtr apiContext)
        {
            if (apiContext == IntPtr.Zero) return;

            var result = interop.tobii_api_destroy(apiContext);
            if (result != tobii_error_t.TOBII_ERROR_NO_ERROR)
            {
                UnityEngine.Debug.LogError($"Failed to destroy api context. Error {result}");
            }
        }

        [AOT.MonoPInvokeCallback(typeof(tobii_device_url_receiver_t))]
        private static void DeviceUrlReceiver(string url, IntPtr userData)
        {
            var gch = GCHandle.FromIntPtr(userData);
            var urls = (List<string>)gch.Target;
            urls.Add(url);
        }

        private static bool GetAvailableTrackers(IStreamEngineInterop interop, IntPtr apiContext, out List<string> connectedDevices)
        {
            connectedDevices = new List<string>();
            var gch = GCHandle.Alloc(connectedDevices);
            var result = interop.tobii_enumerate_local_device_urls_internal(apiContext, _deviceUrlReceiver, GCHandle.ToIntPtr(gch));
            if (result != tobii_error_t.TOBII_ERROR_NO_ERROR)
            {
                UnityEngine.Debug.LogError("Failed to enumerate connected devices. " + result);
                return false;
            }

            if (connectedDevices.Count >= 1) return true;

            UnityEngine.Debug.LogWarning("No connected eye trackers found.");
            return false;
        }

        private static ConnectResult GetFirstSupportedTracker(IStreamEngineInterop interop, IntPtr apiContext, IList<string> connectedDevices, StreamEngineTracker_Description description, out IntPtr deviceContext, out string deviceUrl)
        {
            deviceContext = IntPtr.Zero;
            deviceUrl = "";
            var connectResult = new ConnectResult();
            
            for (var i = 0; i < connectedDevices.Count; i++)
            {
                var connectedDeviceUrl = connectedDevices[i];
                connectResult = CreateDeviceContext(interop, connectedDeviceUrl, Interop.tobii_field_of_use_t.TOBII_FIELD_OF_USE_STORE_OR_TRANSFER_FALSE, apiContext, description.License, out deviceContext);
                if (!connectResult.Connected) continue;

                var result = interop.tobii_get_device_info(deviceContext, out var deviceInfo);
                if (result != tobii_error_t.TOBII_ERROR_NO_ERROR)
                {
                    DestroyDeviceContext(interop, deviceContext);
                    UnityEngine.Debug.LogWarning("Failed to get device info. " + result);
                    continue;
                }

                var integrationType = deviceInfo.integration_type.ToLowerInvariant();
                if (integrationType != description.SupportedIntegrationType)
                {
                    DestroyDeviceContext(interop, deviceContext);
                    continue;
                }

                deviceUrl = connectedDeviceUrl;
                return connectResult;
            }

            UnityEngine.Debug.LogWarning($"Failed to find Tobii eye trackers of integration type {description.SupportedIntegrationType}");
            return connectResult;
        }

        public static bool TryReconnect(IStreamEngineInterop interop, IntPtr deviceContext)
        {
            var result = interop.tobii_device_reconnect(deviceContext);
            if (result != tobii_error_t.TOBII_ERROR_NO_ERROR) return false;

            UnityEngine.Debug.Log("Reconnected.");
            return true;
        }
    }

    
}
