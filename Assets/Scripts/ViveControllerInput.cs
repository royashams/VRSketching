using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViveControllerInput : MonoBehaviour {
    public ModelsController mc;
    public Draw draw;
    public Draw projectionDraw;
    public GameObject projectionCursor;
    public GameObject projectionLaser;
    public GameObject visualPointer;
    public float threshhold = 0.05f;
    public GameObject menu;
    private SteamVR_TrackedObject trackedObj;
    private SteamVR_LaserPointer laserPtr;
    private PartitionMesh pm;
    //private Renderer visualPointerRend;
    private GameObject cursor;
    private Vector3 hitCursorNormalLocal;
    private int hitCursorTriangleIdx;
    private SteamVR_Controller.Device controller {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }
    public ProjectionMode projectionMode;
    public enum ProjectionMode {
        ClosestHit,
        Occlusion,
        Spray
    }
    private enum Mode {
        Drawing, 
        Menu
    };

    private Mode mode = Mode.Drawing;

    void Awake() {
        Debug.Log("VCI");
        Debug.Log(projectionMode);
        //visualPointerRend = visualPointer.GetComponent<Renderer>();
        pm = mc.GetComponent<PartitionMesh>();
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        laserPtr = GetComponent<SteamVR_LaserPointer>();
        foreach (Transform child in transform) {
            if (child.name == "Cursor") {
                cursor = child.gameObject;
                break;
            }
        }
    }

    private void Start() {
        RaycastHit hitInfo;
        Ray ray = new Ray(gameObject.transform.position, cursor.transform.position - gameObject.transform.position);
        int layermask = 1 << 10;
        bool cast = Physics.Raycast(ray, out hitInfo, Mathf.Infinity, layermask);
        while(!cast) {
            cast = Physics.Raycast(ray, out hitInfo, Mathf.Infinity, layermask);
        }
        MeshCollider meshCollider = hitInfo.collider as MeshCollider;
        int a = meshCollider.sharedMesh.triangles[hitInfo.triangleIndex * 3];
        int b = meshCollider.sharedMesh.triangles[hitInfo.triangleIndex * 3 + 1];
        int c = meshCollider.sharedMesh.triangles[hitInfo.triangleIndex * 3 + 2];
        hitCursorNormalLocal = Vector3.Cross(100f * meshCollider.sharedMesh.vertices[b] - 100f * meshCollider.sharedMesh.vertices[a], 100f * meshCollider.sharedMesh.vertices[c] - 100f * meshCollider.sharedMesh.vertices[a]);
        hitCursorTriangleIdx = hitInfo.triangleIndex;
    }

    // Update is called once per frame
    void Update() {
        cursor.transform.position = gameObject.transform.TransformPoint(0f, -0.1f, 0.05f);
        if (controller.GetPressDown(SteamVR_Controller.ButtonMask.ApplicationMenu)) {
            cursor.GetComponent<SwitchCursor>().Switch();
            draw.SwitchMode();
            projectionDraw.SwitchMode();
            projectionLaser.SetActive(!projectionLaser.activeSelf);
            SwitchMode();
        }

        if (controller.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad) && !controller.GetPress(SteamVR_Controller.ButtonMask.Grip)) {
            mc.GetComponent<ModelsController>().ToggleModelRenderer();
        }
        switch (mode) {
            case Mode.Drawing:
                PartitionMesh.CustomHitInfo hit = new PartitionMesh.CustomHitInfo();
                float triggeraxis = controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger).x;
                hit = pm.GlobalClosestHit(cursor.transform.position);
                cursor.transform.position = hit.point;
                draw.SetTargetHit(hit);
                Ray ray = new Ray();
                switch (projectionMode) {
                    case ProjectionMode.Occlusion:
                        //Debug.Log("OCCLUSION");
                        ray = new Ray(Camera.main.transform.position, gameObject.transform.TransformPoint(0f, -0.1f, 0.05f) - Camera.main.transform.position);                       
                        break;
                    case ProjectionMode.Spray:
                        //Debug.Log("SPRAY");
                        ray = new Ray(gameObject.transform.TransformPoint(0f, -0.1f, 0.05f), gameObject.transform.TransformVector(0f, -0.1f, 0.05f));
                        break;
                    case ProjectionMode.ClosestHit:
                       //Debug.Log("CLOSEST HIT");
                        break;
                }
                RaycastHit hitInfo;
                int layermask = 1 << 9;
                PartitionMesh.CustomHitInfo projectedHit;
                if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, layermask)) {
                    projectedHit.point = hitInfo.point;
                    projectedHit.triangleIndex = hitInfo.triangleIndex;
                    projectedHit.normal = hitInfo.normal;
                    projectedHit.collider = hitInfo.collider;
                }
                else {
                    projectedHit = HitCursor();
                }
                projectionDraw.SetTargetHit(projectedHit);
                projectionCursor.transform.position = projectedHit.point;
                break;
        }
        
    }

    private PartitionMesh.CustomHitInfo HitCursor() {
        //A little hack
        PartitionMesh.CustomHitInfo customHitInfo;
        customHitInfo.point = cursor.transform.position;
        customHitInfo.normal = cursor.transform.TransformVector(hitCursorNormalLocal);
        customHitInfo.triangleIndex = hitCursorTriangleIdx;
        customHitInfo.collider = cursor.GetComponent<MeshCollider>();
        return customHitInfo;
    }

    private void ShowMenuAndLaser() {
        laserPtr.enabled = true;
        menu.SetActive(true);
    }

    private void HideMenuAndLaser() {
        laserPtr.enabled = false;
        menu.SetActive(false);
    }

    public void SwitchMode() {
        switch (mode) {
            case Mode.Drawing:
                //ShowMenuAndLaser();
                //visualPointerRend.enabled = false;
                mode = Mode.Menu;
                break;
            case Mode.Menu:
                //HideMenuAndLaser();
                //visualPointerRend.enabled = true;
                mode = Mode.Drawing;
                break;
        }
    }

}
