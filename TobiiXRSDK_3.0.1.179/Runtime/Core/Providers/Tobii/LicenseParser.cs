using System;
using System.Linq;
using Tobii.StreamEngine;
using UnityEngine;

namespace Tobii.XR
{
    public enum FeatureGroup
    {
        Consumer,
        Config,
        Professional,
    }

    public class LicenseParser
    {
        private readonly LicenseJson _json;
        
        public string Licensee { get; private set; }
        public DateTime? ValidTo { get; private set; }
        public bool EyeImages { get; private set; }

        public bool LicenseIsParsed { get; private set; }

        public FeatureGroup FeatureGroup { get; private set; }

        public LicenseParser(string license)
        {
            var t = license.Substring(0, license.LastIndexOf("}", StringComparison.Ordinal) + 1);

            try
            {
                _json = JsonUtility.FromJson<LicenseJson>(t);
                Licensee = _json.licenseKey.header.licensee;

                if (_json.licenseKey.conditions.dateValid != null)
                {
                    var validTo = _json.licenseKey.conditions.dateValid.to;
                    if (!string.IsNullOrEmpty(validTo))
                    {
                        ValidTo = DateTime.Parse(validTo);
                    }
                }

                switch (_json.licenseKey.enables.featureGroup)
                {
                    case "consumer":
                        FeatureGroup = FeatureGroup.Consumer;
                        break;
                    case "config":
                        FeatureGroup = FeatureGroup.Config;
                        break;
                    case "professional":
                        FeatureGroup = FeatureGroup.Professional;
                        break;
                }

                // Check features
                if (_json.licenseKey.enables.features != null)
                {
                    var hasLimitedImageStream = false;
                    var hasDiagnosticsImageStream = false;
                    foreach (var feature in _json.licenseKey.enables.features)
                    {
                        switch (feature)
                        {
                            case "wearableLimitedImage":
                                hasLimitedImageStream = true;
                                break;
                            case "wearableDiagnosticsImage":
                                hasDiagnosticsImageStream = true;
                                break;
                        }
                    }

                    EyeImages = hasLimitedImageStream && hasDiagnosticsImageStream;
                }

                LicenseIsParsed = true;
            }
            catch (Exception exception)
            {
                Debug.Log("Unable to parse license: " + exception);
                LicenseIsParsed = false;
            }
        }

        public string FriendlyValidationError(tobii_license_validation_result_t validationResult, tobii_device_info_t deviceInfo)
        {
            var msg = "";
            switch (validationResult)
            {
                case tobii_license_validation_result_t.TOBII_LICENSE_VALIDATION_RESULT_OK:
                    break;
                case tobii_license_validation_result_t.TOBII_LICENSE_VALIDATION_RESULT_TAMPERED:
                    msg = "The license file has been modified since it was created. \n\nTry replacing it with a new copy of the license you received from Tobii.";
                    break;
                case tobii_license_validation_result_t.TOBII_LICENSE_VALIDATION_RESULT_INVALID_APPLICATION_SIGNATURE:
                    msg = "The public key of the signature of this app does not match the expected public key in the license file. \n\nThe application has to be signed with the key provided to Tobii when requesting the license.";
                    break;
                case tobii_license_validation_result_t.TOBII_LICENSE_VALIDATION_RESULT_NONSIGNED_APPLICATION:
                    msg = "The license expects the application to be signed but it is not. Make sure the application is signed with the key you provided to Tobii when requesting the license.";
                    break;
                case tobii_license_validation_result_t.TOBII_LICENSE_VALIDATION_RESULT_EXPIRED:
                    msg = $"The license was only valid until '{_json.licenseKey.conditions.dateValid.to}' and has now expired. \n\nVerify that the system clock on your device is correctly set.";
                    break;
                case tobii_license_validation_result_t.TOBII_LICENSE_VALIDATION_RESULT_PREMATURE:
                    msg = $"The license will not become valid until '{_json.licenseKey.conditions.dateValid.from}'. \n\nVerify that the system clock on your device is correctly set.";
                    break;
                case tobii_license_validation_result_t.TOBII_LICENSE_VALIDATION_RESULT_INVALID_PROCESS_NAME:
                    var processName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
                    var processNames = string.Join(",", _json.licenseKey.conditions.process.names);
                    msg = $"The name of this application ('{processName}') is not in the list of permitted application names of the license ({processNames}). This check is case sensitive.";
                    break;
                case tobii_license_validation_result_t.TOBII_LICENSE_VALIDATION_RESULT_INVALID_SERIAL_NUMBER:
                    var serialNumbers = string.Join(",", _json.licenseKey.conditions.serialNumbers);
                    msg = $"The serial number of the connected eye tracker ('{deviceInfo.serial_number}') is not in the list of permitted serial numbers in the license ({serialNumbers}). \n\nTo use this app with other eye trackers, request a new license from Tobii.";
                    break;
                case tobii_license_validation_result_t.TOBII_LICENSE_VALIDATION_RESULT_INVALID_MODEL:
                    var models = string.Join(",", _json.licenseKey.conditions.models);
                    msg = $"The model of the connected eye tracker ('{deviceInfo.model}') is not in the list of permitted models in the license ({models}).";
                    break;
                case tobii_license_validation_result_t.TOBII_LICENSE_VALIDATION_RESULT_INVALID_PLATFORM_TYPE:
                    var platformTypes = string.Join(",", _json.licenseKey.conditions.platformTypes);
                    msg = $"The platform type of the connected eye tracker is not in the list of permitted platform types in the license ({platformTypes}).";
                    break;
                case tobii_license_validation_result_t.TOBII_LICENSE_VALIDATION_RESULT_REVOKED:
                    msg = "The license has been revoked and can no longer be used. Request a new license from Tobii.";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(validationResult), validationResult, null);
            }

            return msg;
        }


        #region License JSON

#pragma warning disable 0649 //  Field is never assigned to, and will always have its default value null

        [Serializable]
        private class LicenseJson
        {
            public LicenseKey licenseKey;
        }

        [Serializable]
        private class Header
        {
            public string id;
            public string licensee;
            public string version;
            public string created;
        }

        [Serializable]
        private class DateValid
        {
            public string from;
            public string to;
        }

        [Serializable]
        private class Process
        {
            public string[] names;
            public string[] signatures;
        }

        [Serializable]
        private class Conditions
        {
            public string[] models;
            public string[] platformTypes;
            public string[] serialNumbers;
            public DateValid dateValid;
            public Process process;
        }

        [Serializable]
        private class Enables
        {
            public string featureGroup;
            public string[] features;
        }

        [Serializable]
        private class LicenseKey
        {
            public Header header;
            public Conditions conditions;
            public Enables enables;
        }

#pragma warning restore

        #endregion
    }
}