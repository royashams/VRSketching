using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ProjectionMode { ClosestHit, Occlusion, Spray, Combo, Phong };
public static class Globals
{
    public const Valve.VR.SteamVR_Input_Sources HAND = Valve.VR.SteamVR_Input_Sources.RightHand;
    //    //public static readonly string[] PROJECTION_MODES = { "Occlusion", "Spray", "Combo", "Closest Hit", "Phong"};

}
