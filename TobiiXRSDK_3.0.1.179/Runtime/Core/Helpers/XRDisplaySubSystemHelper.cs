using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

public static class XRDisplaySubSystemHelper
{
    private static readonly List<XRDisplaySubsystem> DisplaySystems = new List<XRDisplaySubsystem>();
    private static XRDisplaySubsystem _activeDisplaySystem;

    public static bool TryGetFirstActive(out XRDisplaySubsystem displaySubsystem)
    {
        if (_activeDisplaySystem != null && _activeDisplaySystem.running)
        {
            displaySubsystem = _activeDisplaySystem;
            return true;
        }

        DisplaySystems.Clear();
        SubsystemManager.GetInstances(DisplaySystems);
        _activeDisplaySystem = displaySubsystem = DisplaySystems.FirstOrDefault(x => x.running);
        
        return _activeDisplaySystem != null;
    }

    /// <summary>
    /// Returns true if at least one XR display system is running.
    ///
    /// To see if it is actually rendering, use `IsRendering`
    /// </summary>
    /// <returns></returns>
    public static bool IsRunning()
    {
        return TryGetFirstActive(out var displaySubsystem);
    }

    /// <summary>
    /// Returns true if configured to run XR and display is being rendered to
    /// </summary>
    /// <returns></returns>
    public static bool IsRendering()
    {
        if (TryGetFirstActive(out var displaySubsystem))
        {
            return displaySubsystem.GetRenderPassCount() > 0;
        }
        return false;
    }

    /// <summary>
    /// Returns true if configured to run XR but display is not being rendered to yet
    /// </summary>
    /// <returns></returns>
    public static bool IsXRButNotRenderingYet()
    {
        if (TryGetFirstActive(out var displaySubsystem))
        {
            return displaySubsystem.GetRenderPassCount() == 0;
        }
        return false;
    }

    public static bool TryGetRenderParameters(Camera cam, out XRDisplaySubsystem.XRRenderParameter left, out XRDisplaySubsystem.XRRenderParameter right)
    {
        left = default;
        right = default;

        if (!TryGetFirstActive(out var ds)) return false;
        
        var leftAssigned = false;
        var renderPassCount = ds.GetRenderPassCount();
        for (var renderPassIndex = 0; renderPassIndex < renderPassCount; renderPassIndex++)
        {
            ds.GetRenderPass(renderPassIndex, out var renderPass);
            var renderParameterCount = renderPass.GetRenderParameterCount();
            for (var renderParameterIndex = 0; renderParameterIndex < renderParameterCount; renderParameterIndex++)
            {
                renderPass.GetRenderParameter(cam, renderParameterIndex, out var parameter);
                if (!leftAssigned)
                {
                    left = parameter;
                    leftAssigned = true;
                }
                else
                {
                    right = parameter;
                    return true;
                }
            }
        }

        return false;
    }
}