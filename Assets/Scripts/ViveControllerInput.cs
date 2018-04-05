using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViveControllerInput : MonoBehaviour {
    public ModelsController mc;
    public Draw draw;
    public GameObject visualPointer;
    public float threshhold = 0.05f;
    public GameObject menu;
    private SteamVR_TrackedObject trackedObj;
    private SteamVR_LaserPointer laserPtr;
    private PartitionMesh pm;
    private Renderer visualPointerRend;
    private GameObject cursor;
    private Vector3 hitCursorNormalLocal;
    private int hitCursorTriangleIdx;
    private SteamVR_Controller.Device controller {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }
    private enum Mode {
        Drawing,
        Menu
    };
    private Mode mode = Mode.Drawing;

    void Awake() {
        visualPointerRend = visualPointer.GetComponent<Renderer>();
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
            SwitchMode();
        }
        switch (mode) {
            case Mode.Drawing:
                PartitionMesh.CustomHitInfo hit = pm.ClosestHit(cursor.transform.position);
                visualPointer.transform.position = hit.point;
                if (controller.GetHairTrigger()) {                   
                    //Snapping condition
                    Ray ray = new Ray(Camera.main.transform.position, cursor.transform.position - Camera.main.transform.position);
                    RaycastHit hitInfo;
                    int layermask = 1 << 9;
                    bool testhit = Physics.Raycast(ray, out hitInfo, Mathf.Infinity, layermask);
                    if (Vector3.Distance(hit.point, cursor.transform.position) <= threshhold || (testhit && Vector3.Distance(hitInfo.point, Camera.main.transform.position) < Vector3.Distance(cursor.transform.position, Camera.main.transform.position))) {
                        visualPointerRend.enabled = false;
                        cursor.transform.position = hit.point;
                        draw.SetTargetHit(hit);
                    }
                    else {
                        visualPointerRend.enabled = true;
                        hit = HitCursor();
                        draw.SetTargetHit(hit);
                    }
                }
                else {
                    visualPointerRend.enabled = true;
                }
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
                ShowMenuAndLaser();
                visualPointerRend.enabled = false;
                mode = Mode.Menu;
                break;
            case Mode.Menu:
                HideMenuAndLaser();
                visualPointerRend.enabled = true;
                mode = Mode.Drawing;
                break;
        }
    }

}
