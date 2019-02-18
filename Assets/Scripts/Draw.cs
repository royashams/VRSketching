using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Valve.VR;

public class Draw : MonoBehaviour {
    public int verticesPerPoint = 6;
    public float stroke_width = 0.0015f;
    public List<int> ns;
    public List<Vector3> points;
    public List<Vector3> normals;
    public List<float> timestamps;
    public float cursorSize = 32.0f;
    public SteamVR_TrackedObject trackedObj;
    public GameObject Stroke;
    public PartitionMesh pm;
    //private SteamVR_Controller.Device controller {
    //    get { return SteamVR_Controller.Input((int)trackedObj.index); }
    //}
    public string name;
    private int pointsInCurStroke = 0;
    private MeshFilter mf;
    private PartitionMesh.CustomHitInfo targetHit;
    public const float minDistance = 0.001f;
    public const float maxAngle = 60f;
    public const float angularChangeThreshold = 20f;
    public const float vThreshold = 0.4f;
    public const float aThreshold = 0.2f;
    private Vector3 qold, vold, a;
    private float lastTimeAddPoint;
    private int outlierCount = 0;
    private int resetPoint = 4;
    public const int lookBack = 2;
    private enum Mode {
        Drawing,
        Erasing
    }

    ;

    private Mode mode = Mode.Drawing;
    private MeshCollider mc;
    private bool drawnLastFrame = false;
    private int counter = 0;
    //private Material mat;
    // Use this for initialization
    void Start() {
        ns = new List<int>();
        points = new List<Vector3>();
        normals = new List<Vector3>();
        timestamps = new List<float>();
        //mat = GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void LateUpdate() {
        // counter++;
        // if (counter % 20 != 0)
        //     return;
        if (name=="draw")
            return;
        switch (mode) {
            case Mode.Drawing:
                bool drawn = false;
                //float triggeraxis = controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger).x;
                //if (triggeraxis == 1.0f) {
                bool drawButtonPressed = SteamVR_Actions.default_Draw.state;
                if (drawButtonPressed) {
                    if (!drawnLastFrame) {
                        CreateNewStroke();
                    }
                    drawn = DrawPoint();
                }
                else if (drawnLastFrame) {
                    ns.Add(pointsInCurStroke);
                    if(pm) {
                        pm.tOld = -1;
                    }                 
                    pointsInCurStroke = 0;
                }
                drawnLastFrame = drawn;
                break;
            case Mode.Erasing:
                break;
        }
    }

    bool DrawPoint() {
        //Early return processing
        //Condition 1: if the point is less than minDistance from the previous point, probably that the user is just drawing very slowly, so skip adding the point
       /* if (mf.sharedMesh && mf.sharedMesh.vertices.Length >= verticesPerPoint) {
            if (Vector3.Distance(targetHit.point, points[points.Count - 1]) < minDistance) {
                return true;
            }
        }
        //Condition 2: find if it is an outliner. Will skip if it is.
        if (mf.sharedMesh && mf.sharedMesh.vertices.Length >= lookBack * verticesPerPoint) {
            Vector3 prev = points[points.Count - (lookBack - 1)] - points[points.Count - lookBack];
            float maxRotation = 0f;
            for (int i = points.Count - (lookBack - 2); i < points.Count; i++) {
                Vector3 cur = points[i] - points[i - 1];
                float rotation = Vector3.Angle(prev, cur);
                if (rotation > maxRotation) {
                    maxRotation = rotation;
                }
                prev = cur;
            }
            float nextRot = Vector3.Angle(targetHit.point - points[points.Count - 1], prev);
            if (nextRot > maxAngle && nextRot > maxRotation + angularChangeThreshold) {
                return true;
            }
        }
        float curTime = Time.time;
        if (qold != Vector3.zero && vold != Vector3.zero && a != Vector3.zero) {
            Vector3 testv = (targetHit.point - qold) / (curTime - lastTimeAddPoint);
            float speed = testv.magnitude;
            if (speed > vThreshold && speed - (vold + a).magnitude > aThreshold) {
                outlierCount++;
            }
            else {
                outlierCount = 0;
            }
        }

        if (!mf.sharedMesh || mf.sharedMesh.vertexCount == 0) {
            qold = targetHit.point;
        }
        else if (outlierCount == 0) {
            Vector3 testv = (targetHit.point - qold) / (curTime - lastTimeAddPoint);
            if (vold!=Vector3.zero) {
                a = testv - vold;
            }
            vold = testv;
            qold = targetHit.point;
            lastTimeAddPoint = curTime;
        }
        else {
            if (outlierCount > resetPoint) {
                outlierCount = 0;
            }
            return true;
        }*/

        //Processing ends
        Vector3[] vertices;
        Vector3[] normals;
        int[] triangles;
        // creating a stroke by storing in a mesh
        if (mf.sharedMesh) {
            vertices = mf.sharedMesh.vertices;
            triangles = mf.sharedMesh.triangles;
            normals = mf.sharedMesh.normals;
            mf.sharedMesh.Clear();
        }
        else {
            vertices = new Vector3[0];
            normals = new Vector3[0];
            triangles = new int[0];
        }
        int oldVerticeLength = vertices.Length;
        Array.Resize(ref vertices, oldVerticeLength + verticesPerPoint);
        Array.Resize(ref normals, oldVerticeLength + verticesPerPoint);
        MeshCollider meshCollider = targetHit.collider as MeshCollider;
        
        Mesh mesh = meshCollider.sharedMesh;
        Vector3 bn;
        if (oldVerticeLength == 0) {
            //First point of the stroke, no way to predict the tangent, so can only make a coarse approximation
            Vector3 vertex1 = mesh.vertices[mesh.triangles[targetHit.triangleIndex * 3]];
            Vector3 vertex2 = mesh.vertices[mesh.triangles[targetHit.triangleIndex * 3 + 1]];
            bn = vertex1 - vertex2;
        }
        else {
            Vector3 prevPOS = points[points.Count - 1];
            Vector3 tangent = (targetHit.point - prevPOS).normalized;
            bn = Vector3.Cross(tangent, targetHit.normal);
        }
        Vector3 n = targetHit.normal;
        bn = bn.normalized;
        n = n.normalized;
        float r = stroke_width / 2.0f;

        for (int i = 0; i < verticesPerPoint; ++i) {
            vertices[oldVerticeLength + i] =
            targetHit.point +
            (float)Mathf.Cos(2 * Mathf.PI * (i) / verticesPerPoint) * r * bn +
            (float)Mathf.Sin(2 * Mathf.PI * (i) / verticesPerPoint) * r * n;
            normals[oldVerticeLength + i] = (vertices[oldVerticeLength + i] - targetHit.point).normalized;
        }

        if (oldVerticeLength > 0) {
            int oldTriangleLength = triangles.Length;
            Array.Resize(ref triangles, oldTriangleLength + verticesPerPoint * 6);
            for (int quad = 0; quad < verticesPerPoint; ++quad) {
                triangles[oldTriangleLength + quad * 6 + 0] = (oldVerticeLength - 6) + quad;
                triangles[oldTriangleLength + quad * 6 + 1] = oldVerticeLength + quad;
                triangles[oldTriangleLength + quad * 6 + 2] = (oldVerticeLength - 6) + (quad + 1) % verticesPerPoint;
                triangles[oldTriangleLength + quad * 6 + 3] = (oldVerticeLength - 6) + (quad + 1) % verticesPerPoint;
                triangles[oldTriangleLength + quad * 6 + 4] = oldVerticeLength + quad;
                triangles[oldTriangleLength + quad * 6 + 5] = oldVerticeLength + (quad + 1) % verticesPerPoint;
            }
        }
        Mesh stroke = new Mesh();
        stroke.vertices = vertices;
        stroke.normals = normals;
        stroke.triangles = triangles;
        mf.sharedMesh = stroke;
        /*if (mc.sharedMesh) {
            mc.sharedMesh = null;
        }*/
        mc.sharedMesh = stroke;
        //Debug code
        /*GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.transform.localScale = new Vector3(stroke_width, stroke_width, stroke_width);
        quad.transform.position = hitInfo.point;
        quad.transform.right = hitInfo.normal;
        quad.GetComponent<Renderer>().material = mat;
        if (!Input.GetMouseButtonDown(0)) {
            Vector3 prevPOS = points[points.Count - 1];
            Vector3 tangent = (hitInfo.point - prevPOS).normalized;
            Vector3 bn = Vector3.Cross(tangent, hitInfo.normal);//First point of the stroke, no way to predict the tangent, so can only make a coarse
            bn.Normalize();
            quad.transform.up = bn;
            GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinder.transform.localScale = new Vector3(stroke_width/10f, stroke_width/10f, stroke_width/10f);
            cylinder.transform.position = hitInfo.point;
            cylinder.transform.up = tangent;
            cylinder.GetComponent<Renderer>().material = mat2;
        }*/
        AddNewPoint(ref targetHit);
        return true;
    }

    public void RemoveUserData(int childIdx) {
        int firstPt = 0;
        for (int i=0; i<childIdx; ++i) {
            firstPt += ns[i];
        }
        /*points.RemoveRange(firstPt, ns[childIdx]);
        normals.RemoveRange(firstPt, ns[childIdx]);
        timestamps.RemoveRange(firstPt, ns[childIdx]);
        ns.RemoveAt(childIdx);*/                         
    }

    void AddNewPoint(ref PartitionMesh.CustomHitInfo hitInfo) {
        // We want to store information relative to the virtual object
        points.Add(hitInfo.point);
        normals.Add(hitInfo.normal);
        timestamps.Add(Time.time);
        pointsInCurStroke++;
    }

    void CreateNewStroke() {
        GameObject stroke = Instantiate(Stroke);
        mf = stroke.GetComponent<MeshFilter>();
        mc = stroke.GetComponent<MeshCollider>();
        stroke.transform.parent = gameObject.transform;
    }

    public void SwitchMode() {
        if (mode == Mode.Drawing) {
            mode = Mode.Erasing;
        }
        else {
            mode = Mode.Drawing;
        }
    }

    public void SetTargetHit(PartitionMesh.CustomHitInfo hitInfo) {
        targetHit = hitInfo;
    }

}
