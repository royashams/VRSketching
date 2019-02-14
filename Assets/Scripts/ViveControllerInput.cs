using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViveControllerInput : MonoBehaviour {
    public ModelsController mc;
    public Draw closestDraw;
    public Draw occlusionDraw;
    public Draw sprayDraw;
    public Draw comboDraw;
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
    public Ray OcclusionRay;
    public Ray SprayRay;
    private Ray ComboRay;

    private enum Mode {
        Drawing,
        Menu
    };

    private Vector2 touchCoords  = new Vector2(0.0f, 0.0f); 
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
            comboDraw.SwitchMode();
            projectionLaser.SetActive(!projectionLaser.activeSelf);
            SwitchMode();
        }

        if (controller.GetTouch(SteamVR_Controller.ButtonMask.Touchpad))
        {
            Vector2 touch = (controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0));
            //Debug.Log("i was touched...");
            touchCoords.x = touch.x;
            touchCoords.y = touch.y;
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
                print("Down: Combined Ray");
                projectionMode = "Combo";
                ChangeStroke(projectionMode);
                comboDraw.enabled = true;
            }

            if (touchpad.x > 0.7f)
            {
                print("Moving Right");
                projectionMode = "Spray";
                ChangeStroke(projectionMode);
                sprayDraw.enabled = true;

                //if (Input.touchCount > 0) {
                //    Touch touch = Input.GetTouch(0);
                //    Vector2 pos = touch.position;
                //    Debug.Log(pos.x);
                //    Debug.Log(pos.y);
                //}
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
                OcclusionRay = new Ray(Camera.main.transform.position, gameObject.transform.TransformPoint(0f, -0.1f, 0.05f) - Camera.main.transform.position);
                SprayRay = new Ray(gameObject.transform.TransformPoint(0f, -0.1f, 0.05f), gameObject.transform.TransformVector(0f + (touchCoords.x *0.1f), 0f + (touchCoords.y * 0.1f), 0.05f));
                // ray is the ray from the headset to the controller (in `Occlusion` mode, which we want to use)
                switch (projectionMode) {
                    case "Occlusion":
                        ray = OcclusionRay;
                        break;
                    case "Spray":
                         //ray = new Ray(gameObject.transform.TransformPoint(0f, -0.1f, 0.05f), gameObject.transform.TransformVector(0f, -0.1f, 0.05f));
                        ray = SprayRay;
                        //ray = new Ray(gameObject.transform.position, gameObject.transform.forward);
                        break;
                    case "Combo":
                        ComboRay = makeComboRay();
                        ray = ComboRay;
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
                comboDraw.SetTargetHit(projectedHit);
                projectionCursor.transform.position = projectedHit.point;
                break;
        }
        
    }

    // Create the combined ray from two rays by calculating confidence values and averaging them
    private Ray makeComboRay() {
        // controller positions ciP and orientation ciO, ciP is p_i
        Vector3 ciP = gameObject.transform.TransformPoint(0f, -0.1f, 0.05f);
        Vector3 ciO = gameObject.transform.TransformVector(0f, -0.1f, 0.05f);
        // head: hiP and hiO
        Vector3 hiP = Camera.main.transform.position;
        Vector3 hiO = Camera.main.transform.forward;  // idk if forward is right but w/e
        // mesh
        // Mesh mesh = mc.GetComponent<PartitionMesh>().GetComponent<MeshFilter>().sharedMesh;
        // Vector3[] meshNorms = mesh.normals;

        // ray from controller and camera
        Ray controllerRay = new Ray(ciP, ciO);
        Ray cameraRay = new Ray(hiP, hiO);

        // hit info from two rays
        PartitionMesh.CustomHitInfo controllerHitInfo = getProjectedHit(controllerRay);
        PartitionMesh.CustomHitInfo cameraHitInfo = getProjectedHit(cameraRay);

        // TODO: take care of threshold stuff later
        float dist = Vector3.Distance(comboDraw.points[comboDraw.points.Count-1], comboDraw.points[comboDraw.points.Count-2]);
        float threshhold = 2f;
    
        Vector3 controllerHitPoint = controllerHitInfo.point; //cm_i
        Vector3 controllerHitNormal = controllerHitInfo.normal; //cm'_i
        //Distance cd_i=||p_i-cm_i||, Angle ca_i= max(0,-(c’_i dot cm’_i ));  
        float dist_cdI = Vector3.Distance(ciP, controllerHitPoint);
        float angle_caI = Mathf.Max(0, -Vector3.Dot(ciO, controllerHitNormal));    

        // make view viO, which is an orientation  view v’_i = normalize(p_i-h_i)
        Vector3 viO = Vector3.Normalize(ciP-hiP);
        Ray viewRay = new Ray(ciP, viO);
        PartitionMesh.CustomHitInfo viewHitInfo = getProjectedHit(cameraRay);
        Vector3 viewHitPoint = viewHitInfo.point;
        Vector3 viewHitNormal = viewHitInfo.normal;
        float dist_hdI = Vector3.Distance(ciP, viewHitPoint);
        float angle_haI = Mathf.Max(0, -Vector3.Dot(viO, viewHitNormal)); 

        float sigmoidDist = 0.1f; //edit later

        float w1 = Sigmoid(dist_cdI/sigmoidDist) * angle_caI;
        float w2 = Sigmoid(dist_hdI/sigmoidDist) * angle_haI;

        // do check for w1 w2 = 0 later

        Ray ComboRay = new Ray(ciO, (w1 * ciO + w2 * viO));
        Debug.Log("occ " + OcclusionRay.ToString());
        Debug.Log("spr "+ SprayRay.ToString());
        Debug.Log("combo" + ComboRay.ToString());

        return ComboRay;
    }


    public static float Sigmoid(float value) {
        float k = Mathf.Exp(value);
        return k / (1.0f + k);
    }

    private PartitionMesh.CustomHitInfo getProjectedHit(Ray ray) {
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
        return projectedHit;

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
                projectionLaser.SetActive(true);
                projectionCursor.SetActive(true);
                break;
            case "Closest Hit":
                closestDraw.enabled = true;
                projectionLaser.SetActive(false);
                projectionCursor.SetActive(false);
                break;
            case "Spray":
                sprayDraw.enabled = true;
                laserPtr.enabled = false;
                projectionLaser.SetActive(true);
                projectionCursor.SetActive(true);
                break;
            case "Combo":
                comboDraw.enabled = true;
                laserPtr.enabled = false;
                projectionLaser.SetActive(true);
                projectionCursor.SetActive(true);
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
