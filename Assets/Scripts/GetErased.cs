using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class GetErased : MonoBehaviour {
    private Draw parent;
    //private SteamVR_TrackedObject trackedObj;
    //private SteamVR_Controller.Device controller {
    //    get { return SteamVR_Controller.Input((int)trackedObj.index); }
    //}
    // Use this for initialization
    void Start () {
        parent = gameObject.transform.parent.gameObject.GetComponent<Draw>();
        //trackedObj = parent.trackedObj;
    }

    private void OnTriggerStay(Collider other) {
        if (other.tag == "Eraser" && SteamVR_Actions.default_Draw.GetState(Globals.HAND)) {
            parent.RemoveUserData(gameObject.transform.GetSiblingIndex());
            Destroy(gameObject);
        }
    }
}
