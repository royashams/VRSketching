using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.Extras;

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
    //private SteamVR_Controller.Device controller {
    //    get { return SteamVR_Controller.Input((int)trackedObj.index); }
    //}
    private ProjectionMode projectionMode;
    public Ray OcclusionRay;
    public Ray SprayRay;
    private Ray ComboRay;

    private enum Mode {
        Drawing,
        Menu
    };

    private Vector2 touchCoords  = new Vector2(0.0f, 0.0f); 
    private Mode mode = Mode.Drawing;

    private int counter = 0;

    void Awake() {
        pm = mc.GetComponent<PartitionMesh>();
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        laserPtr = GetComponent<SteamVR_LaserPointer>();
        projectionMode = ProjectionMode.ClosestHit;
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
        // counter++;
        // if (counter % 20 != 0)
        //     return;
        cursor.transform.position = gameObject.transform.TransformPoint(0f, -0.1f, 0.05f);
        // Swtich b/w drawing and erasing
        //if (controller.GetPressDown(SteamVR_Controller.ButtonMask.ApplicationMenu))
        if (SteamVR_Actions.default_DrawEraseToggle.state)
        {
            cursor.GetComponent<SwitchCursor>().Switch();
            closestDraw.SwitchMode();
            occlusionDraw.SwitchMode();
            sprayDraw.SwitchMode();
            comboDraw.SwitchMode();
            projectionLaser.SetActive(!projectionLaser.activeSelf);
            SwitchMode();
        }

        // Choose a direction, currently used for the the projection direction in the spraypaint mode
        //if (controller.GetTouch(SteamVR_Controller.ButtonMask.Touchpad))
        if (SteamVR_Actions.default_DirectionSelectToggle.state)
        {
            Vector2 touch = SteamVR_Actions.default_DirectionSelectPositionHelper.axis;
            //Debug.Log("i was touched...");
            touchCoords.x = touch.x;
            touchCoords.y = touch.y;
        }

        // Switch brush mode using a button and a 2-axis input
        if (SteamVR_Actions.default_BrushSelectToggle.state)
        {
            Vector2 modeSelectVector = SteamVR_Actions.default_BrushSelectPositionHelper.axis;
            print("Pressing Touchpad");

            //if (touchpad.y > 0.7f)
            //{
            //    // PROBLEM! need to change projectionMode here as well, otherwise it will use the wrong ray :( 
            //    print("Moving Up");
            //    projectionMode = ProjectionMode.Occlusion;
            //    ChangeStroke(projectionMode);
            //    occlusionDraw.enabled = true;

            //}

            //else if (touchpad.y < -0.7f)
            //{
            //    print("Down: Combined Ray");
            //    projectionMode = ProjectionMode.Combo;
            //    ChangeStroke(projectionMode);
            //    comboDraw.enabled = true;
            //}

            if (modeSelectVector.x > 0.7f)
            {
                print("Moving Right");
                var projectionModeIntVal = (int)projectionMode;
                projectionModeIntVal = (projectionModeIntVal + 1) % System.Enum.GetNames(typeof(ProjectionMode)).Length;
                projectionMode = (ProjectionMode)projectionModeIntVal;
                ChangeStroke(projectionMode);
                sprayDraw.enabled = true;

                //if (Input.touchCount > 0) {
                //    Touch touch = Input.GetTouch(0);
                //    Vector2 pos = touch.position;
                //    Debug.Log(pos.x);
                //    Debug.Log(pos.y);
                //}
            }

            else if (modeSelectVector.x < -0.7f)
            {
                print("Moving left");
                var projectionModeIntVal = (int)projectionMode;
                projectionModeIntVal = (projectionModeIntVal + System.Enum.GetNames(typeof(ProjectionMode)).Length - 1) % System.Enum.GetNames(typeof(ProjectionMode)).Length;
                projectionMode = (ProjectionMode)projectionModeIntVal;
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
                //float triggeraxis = controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger).x;
                // // `hit` is the point on the mesh closest to the controller
                hit = pm.GlobalClosestHit(cursor.transform.position);
                cursor.transform.position = hit.point;
                closestDraw.SetTargetHit(hit);
                Ray ray = new Ray();
                OcclusionRay = new Ray(Camera.main.transform.position, gameObject.transform.TransformPoint(0f, -0.1f, 0.05f) - Camera.main.transform.position);
                SprayRay = new Ray(gameObject.transform.TransformPoint(0f, -0.1f, 0.05f), gameObject.transform.TransformVector(0f + (touchCoords.x *0.1f), 0f + (touchCoords.y * 0.1f), 0.05f));
                // ray is the ray from the headset to the controller (in `Occlusion` mode, which we want to use)
                switch (projectionMode) {
                    case ProjectionMode.Occlusion:
                        ray = OcclusionRay;
                        break;
                    case ProjectionMode.Spray:
                         //ray = new Ray(gameObject.transform.TransformPoint(0f, -0.1f, 0.05f), gameObject.transform.TransformVector(0f, -0.1f, 0.05f));
                        ray = SprayRay;
                        //ray = new Ray(gameObject.transform.position, gameObject.transform.forward);
                        break;
                    case ProjectionMode.Combo:
                        ComboRay = makeComboRay();
                        ray = ComboRay;
                        break;
                    case ProjectionMode.ClosestHit:
                        // PartitionMesh.CustomHitInfo hit = new PartitionMesh.CustomHitInfo();
                        // float triggeraxis = controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger).x;
                        // `hit` is the point on the mesh closest to the controller
                        // hit = pm.GlobalClosestHit(cursor.transform.position);
                        // cursor.transform.position = hit.point;
                        // //Debug.Log("Inside");
                        // closestDraw.SetTargetHit(hit);
                        // ray = new Ray();
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
                    projectedHit.success = true;
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
        // Vector3 hiP = Camera.main.transform.position;
        Vector3 hiO = (ciP - Camera.main.transform.position).normalized;
        // mesh
        // Mesh mesh = mc.GetComponent<PartitionMesh>().GetComponent<MeshFilter>().sharedMesh;
        // Vector3[] meshNorms = mesh.normals;

        // ray from controller and camera
        Ray controllerRay = new Ray(ciP, ciO);
        Ray viewRay = new Ray(ciP, hiO);

        // hit info from two rays
        PartitionMesh.CustomHitInfo controllerHitInfo = getProjectedHit(controllerRay);
        PartitionMesh.CustomHitInfo viewHitInfo = getProjectedHit(viewRay);

        // TODO: take care of threshold stuff later
        // float dist = Vector3.Distance(comboDraw.points[comboDraw.points.Count-1], comboDraw.points[comboDraw.points.Count-2]);
        // float threshold = 2f;
    
        Vector3 controllerHitPoint = controllerHitInfo.point; //cm_i
        Vector3 controllerHitNormal = controllerHitInfo.normal; //cm'_i
        //Distance cd_i=||p_i-cm_i||, Angle ca_i= max(0,-(c’_i dot cm’_i ));  
        float dist_cdI =  Mathf.Infinity;
        if (controllerHitInfo.success)
            dist_cdI = Vector3.Distance(ciP, controllerHitPoint);
        float angle_caI = Mathf.Max(0, -Vector3.Dot(controllerRay.direction, controllerHitNormal));    

        // make view viO, which is an orientation  view v’_i = normalize(p_i-h_i)
        // Vector3 viO = Vector3.Normalize(ciP-hiP);
        // Ray viewRay = new Ray(ciP, viO);
        // PartitionMesh.CustomHitInfo viewHitInfo = getProjectedHit(cameraRay);
        Vector3 viewHitPoint = viewHitInfo.point;
        Vector3 viewHitNormal = viewHitInfo.normal;
        float dist_hdI = Mathf.Infinity;
        if (viewHitInfo.success)
            dist_hdI = Vector3.Distance(ciP, viewHitPoint);
        float angle_haI = Mathf.Max(0, -Vector3.Dot(viewRay.direction, viewHitNormal));

        float sigmoidDist = 0.5f; //edit later

        float w1 = Sigmoid(dist_cdI/sigmoidDist) * angle_caI;
        float w2 = Sigmoid(dist_hdI/sigmoidDist) * angle_haI;

        var weightSum = Mathf.Epsilon + w1 + w2;
        w1 = w1/weightSum;
        w2 = w2/weightSum;

        // do check for w1 w2 = 0 later

        // Ray ComboRay = new Ray(ciP, (w1 * controllerRay.direction + w2 * viewRay.direction));
         Ray ComboRay = new Ray(ciP, Vector3.Slerp(controllerRay.direction, viewRay.direction, w2));
        // Debug.Log("occ " + OcclusionRay.ToString());
        // Debug.Log("spr "+ SprayRay.ToString());
        // Debug.Log("combo" + ComboRay.ToString());
        if (SteamVR_Actions.default_Draw.state)
        {
            Debug.Log(
                dist_cdI.ToString() + ' ' + angle_caI.ToString() + ' ' +
                dist_hdI.ToString() + ' ' + angle_haI.ToString() + "    " +
                w1.ToString() + ' ' + w2.ToString()
            );
            Debug.Log(
                controllerRay.direction.ToString("F3") + 
                viewRay.direction.ToString("F3") + 
                ComboRay.direction.ToString("F3"));

            // if(angle_caI < 0.4)
            //     Debug.Log("Blah");
        }    

        return ComboRay;
    }


    public static float Sigmoid(float value) {
        // float k = Mathf.Exp(value);
        // return k / (1.0f + k);
        float k = Mathf.Pow(1.0f-value*value, 2);
        return Mathf.Clamp(k, 0, 1);
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
            projectedHit.success = true;
        }
        else {
            projectedHit = HitCursor();
            projectedHit.success = false;
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
        customHitInfo.success = false;
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

    private void ChangeStroke(ProjectionMode projectionMode)
    {
        DisableStroke();
        projectionCursor.SetActive(true);
        projectionLaser.SetActive(true);

        switch (projectionMode)
        {
            case ProjectionMode.Occlusion:
                occlusionDraw.enabled = true;
                laserPtr.enabled = false;
                projectionLaser.SetActive(true);
                projectionCursor.SetActive(true);
                break;
            case ProjectionMode.ClosestHit:
                closestDraw.enabled = true;
                projectionLaser.SetActive(false);
                projectionCursor.SetActive(false);
                break;
            case ProjectionMode.Spray:
                sprayDraw.enabled = true;
                laserPtr.enabled = false;
                projectionLaser.SetActive(true);
                projectionCursor.SetActive(true);
                break;
            case ProjectionMode.Combo:
                comboDraw.enabled = true;
                laserPtr.enabled = false;
                projectionLaser.SetActive(true);
                projectionCursor.SetActive(true);
                break;
            case ProjectionMode.Phong:
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
