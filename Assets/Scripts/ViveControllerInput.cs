using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViveControllerInput : MonoBehaviour {
    public ModelsController mc;
    public Draw closestDraw;
    public Draw occlusionDraw;
    public Draw sprayDraw;
    public GameObject projectionCursor;
    public GameObject projectionLaser;
    public GameObject visualPointer;
    public float threshhold = 0.05f;
    public GameObject menu;
    private SteamVR_TrackedObject trackedObj;
    private SteamVR_LaserPointer laserPtr;
    private PartitionMesh pm;
    //private Renderer visualPointerRend;
    //;
    //;
    private GameObject cursor;
    private Vector3 hitCursorNormalLocal;
    private int hitCursorTriangleIdx;
    private SteamVR_Controller.Device controller {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }
    private string projectionMode;
    //public ProjectionMode projectionMode;
    //public enum ProjectionMode {
    //    ClosestHit,
    //    Occlusion,
    //    Spray
    //}
    private enum Mode {
        Drawing,
        Menu
    };

    private Mode mode = Mode.Drawing;

    void Awake() {
        pm = mc.GetComponent<PartitionMesh>();
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        laserPtr = GetComponent<SteamVR_LaserPointer>();
        projectionMode = "Closest Hit";
        ChangeStroke(projectionMode);
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
            closestDraw.SwitchMode();
            occlusionDraw.SwitchMode();
            sprayDraw.SwitchMode();
            projectionLaser.SetActive(!projectionLaser.activeSelf);
            SwitchMode();
        }

        if (controller.GetPress(SteamVR_Controller.ButtonMask.Touchpad))
        {
            Vector2 touchpad = (controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0));
            print("Pressing Touchpad");

            if (touchpad.y > 0.7f)
            {
                // PROBLEM! need to change projectionMode here as well, otherwise it will use the wrong ray :( 
                print("Moving Up");
                projectionMode = "Occlusion";
                ChangeStroke(projectionMode);
                occlusionDraw.enabled = true;

            }

            else if (touchpad.y < -0.7f)
            {
                print("Moving Down");
            }

            if (touchpad.x > 0.7f)
            {
                print("Moving Right");
                projectionMode = "Spray";
                ChangeStroke(projectionMode);
                sprayDraw.enabled = true;
            }

            else if (touchpad.x < -0.7f)
            {
                print("Moving left");
                projectionMode = "Closest Hit";
                ChangeStroke(projectionMode);
                closestDraw.enabled = true;
            }

        }

        // this part makes the model disappear
        //if (controller.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad) && !controller.GetPress(SteamVR_Controller.ButtonMask.Grip)) {
        //    mc.GetComponent<ModelsController>().ToggleModelRenderer();
        //}
        switch (mode) {
            case Mode.Drawing:
                PartitionMesh.CustomHitInfo hit = new PartitionMesh.CustomHitInfo();
                float triggeraxis = controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger).x;
                // // `hit` is the point on the mesh closest to the controller
                hit = pm.GlobalClosestHit(cursor.transform.position);
                cursor.transform.position = hit.point;
                closestDraw.SetTargetHit(hit);
                Ray ray = new Ray();
                // ray is the ray from the headset to the controller (in `Occlusion` mode, which we want to use)
                switch (projectionMode) {
                    case "Occlusion":
                        ray = new Ray(Camera.main.transform.position, gameObject.transform.TransformPoint(0f, -0.1f, 0.05f) - Camera.main.transform.position);
                        break;
                    case "Spray":
                        ray = new Ray(gameObject.transform.TransformPoint(0f, -0.1f, 0.05f), gameObject.transform.TransformVector(0f, -0.1f, 0.05f));
                        //ray = new Ray(gameObject.transform.position, gameObject.transform.forward);
                        break;
                    case "Closest Hit":
                        // PartitionMesh.CustomHitInfo hit = new PartitionMesh.CustomHitInfo();
                        // float triggeraxis = controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger).x;
                        // `hit` is the point on the mesh closest to the controller
                        hit = pm.GlobalClosestHit(cursor.transform.position);
                        cursor.transform.position = hit.point;
                        //Debug.Log("Inside");
                        closestDraw.SetTargetHit(hit);
                        ray = new Ray();
                        break;

                        //case ProjectionMode.Occlusion:
                        //    ray = new Ray(Camera.main.transform.position, gameObject.transform.TransformPoint(0f, -0.1f, 0.05f) - Camera.main.transform.position);                       
                        //    break;
                        //case ProjectionMode.Spray:
                        //    ray = new Ray(gameObject.transform.TransformPoint(0f, -0.1f, 0.05f), gameObject.transform.TransformVector(0f, -0.1f, 0.05f));
                        //    //ray = new Ray(gameObject.transform.position, gameObject.transform.forward);
                        //    break;
                        //case ProjectionMode.ClosestHit:
                        //    // PartitionMesh.CustomHitInfo hit = new PartitionMesh.CustomHitInfo();
                        //    // float triggeraxis = controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger).x;
                        //    // `hit` is the point on the mesh closest to the controller
                        //    hit = pm.GlobalClosestHit(cursor.transform.position);
                        //    cursor.transform.position = hit.point;
                        //    //Debug.Log("Inside");
                        //    closestDraw.SetTargetHit(hit);
                        //    ray = new Ray();
                        //    break;
                }
                RaycastHit hitInfo;
                int layermask = 1 << 9;
                PartitionMesh.CustomHitInfo projectedHit;
                // cast `ray` towards the mesh and find the intersection point `projectedHit`
                if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, layermask))
                {
                    //Debug.Log("Found intersection " + (hitInfo.collider == null).ToString());
                    projectedHit.point = hitInfo.point;
                    projectedHit.triangleIndex = hitInfo.triangleIndex;
                    projectedHit.normal = hitInfo.normal;
                    projectedHit.collider = hitInfo.collider;
                }
                else {
                    projectedHit = HitCursor();
                    //Debug.Log("Fallback " + (projectedHit).ToString());
                }
                occlusionDraw.SetTargetHit(projectedHit);
                sprayDraw.SetTargetHit(projectedHit);
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

    private void ChangeStroke(string projectionMode)
    {
        DisableStroke();
        projectionCursor.SetActive(true);
        projectionLaser.SetActive(true);

        switch (projectionMode)
        {
            case "Occlusion":
                occlusionDraw.enabled = true;
                laserPtr.enabled = false;
                break;
            case "Closest Hit":
                closestDraw.enabled = true;
                projectionLaser.SetActive(false);
                projectionCursor.SetActive(false);
                break;
            case "Spray":
                sprayDraw.enabled = true;
                laserPtr.enabled = false;
                break;
                //case ProjectionMode.Occlusion:
                //    occlusionDraw.enabled = true;
                //    laserPtr.enabled = false;
                //    break;
                //case ProjectionMode.ClosestHit:
                //    closestDraw.enabled = true;
                //    projectionLaser.SetActive(false);
                //    projectionCursor.SetActive(false);
                //    break;
                //case ProjectionMode.Spray:
                //    sprayDraw.enabled = true;
                //    laserPtr.enabled = false;
                //    break;
        }
    }

    private void DisableStroke() {
        closestDraw.enabled = false;
        occlusionDraw.enabled = false;
        sprayDraw.enabled = false;
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
