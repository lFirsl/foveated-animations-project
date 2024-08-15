using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IReadEye 
{

    public struct RayInfo
    {
        public Vector3 pos;
        public Vector3 dir;
    };

    public RayInfo getRightRay();
    public RayInfo getleftRay();
}
