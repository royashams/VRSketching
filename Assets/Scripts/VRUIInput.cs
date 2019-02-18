using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Valve.VR;
using Valve.VR.Extras;

[RequireComponent(typeof(SteamVR_LaserPointer))]
public class VRUIInput : MonoBehaviour {
    private SteamVR_LaserPointer laserPointer;
    private SteamVR_TrackedObject trackedObj;
    //private SteamVR_Controller.Device controller {
    //    get { return SteamVR_Controller.Input((int)trackedObj.index); }
    //}


    private void OnEnable() {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        laserPointer = GetComponent<SteamVR_LaserPointer>();
        laserPointer.PointerIn -= HandlePointerIn;
        laserPointer.PointerIn += HandlePointerIn;
        //original code but I don't need this
        /*laserPointer.PointerOut -= HandlePointerOut;
        laserPointer.PointerOut += HandlePointerOut;*/

    }

    private void HandlePointerIn(object sender, PointerEventArgs e) {
        var button = e.target.GetComponent<Button>();
        if (SteamVR_Actions.default_Draw.state && button != null) {
            button.onClick.Invoke();
        }
    }

    private void HandlePointerOut(object sender, PointerEventArgs e) {

        var button = e.target.GetComponent<Button>();
        if (button != null) {
            EventSystem.current.SetSelectedGameObject(null);
            Debug.Log("HandlePointerOut", e.target.gameObject);
        }
    }
}