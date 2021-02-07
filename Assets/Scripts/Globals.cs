using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ProjectionMode { ClosestHit, Occlusion, Spray, Combo, Phong };
public static class Globals
{
    public const Valve.VR.SteamVR_Input_Sources HAND = Valve.VR.SteamVR_Input_Sources.RightHand;
    //    //public static readonly string[] PROJECTION_MODES = { "Occlusion", "Spray", "Combo", "Closest Hit", "Phong"};

    public static Vector3 ChangeHandedness(Vector3 v)
    {
        return new Vector3(-v.x, v.y, v.z);
    }

}
