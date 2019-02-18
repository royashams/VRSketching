using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartitionMesh : MonoBehaviour {
    public struct CustomHitInfo {
        public Vector3 point;
        public Vector3 normal;
        public int triangleIndex;
        public Collider collider;

        public bool success;
    }

    public int tOld = -1;
    private KdTree kdTree;
    private List<int>[] vertexToTriangle;
    private MeshFilter mf;
    private MeshCollider mc;
    // Use this for initialization
    private void Awake() {
        kdTree = gameObject.GetComponent<KdTree>();
    }

    public CustomHitInfo LocalClosestHit(Vector3 position) {
        if (tOld == -1) {
            CustomHitInfo hitInfo = GlobalClosestHit(position);
            tOld = hitInfo.triangleIndex;
            return hitInfo;
        }

        Mesh mesh = mf.sharedMesh;
        int[] visited = new int[mesh.triangles.Length/3];
        Vector3 positionModelSpace = mf.gameObject.transform.InverseTransformPoint(position);
        CustomHitInfo hit = new CustomHitInfo();
        Vector3 a = mesh.vertices[mesh.triangles[3 * tOld]];
        Vector3 b = mesh.vertices[mesh.triangles[3 * tOld + 1]];
        Vector3 c = mesh.vertices[mesh.triangles[3 * tOld + 2]];
        Vector3 s = ClosestPointOnTriangle(a, b, c, positionModelSpace);
        hit.point = mf.gameObject.transform.TransformPoint(s);
        float minDistance = 10000f;
        List<int> trianglesInCurRing = new List<int>();
        List<int> trianglesInNextRing = new List<int>();
        trianglesInCurRing.Add(tOld);
        int count = 0;
        while (true) {
            bool gettingCloser = false;
            foreach (int triangleIdx in trianglesInCurRing) {
                visited[triangleIdx] = 1;
                a = mesh.vertices[mesh.triangles[3 * triangleIdx]];
                b = mesh.vertices[mesh.triangles[3 * triangleIdx + 1]];
                c = mesh.vertices[mesh.triangles[3 * triangleIdx + 2]];
                s = ClosestPointOnTriangle(a, b, c, positionModelSpace);
                float distance = Vector3.Distance(s, positionModelSpace);
                if (distance < minDistance) {
                    gettingCloser = true;
                    if(count>=1) {
                        Debug.Log("rings away");
                        Debug.Log(count);
                    }
                    minDistance = distance;
                    hit.point = mf.gameObject.transform.TransformPoint(s);
                    Vector3 n = Vector3.Cross(b - a, c - a);
                    n.Normalize();
                    hit.normal = n;
                    hit.triangleIndex = triangleIdx;
                }
                AddTrianglesToRing(trianglesInNextRing, triangleIdx, visited);
            }
            trianglesInCurRing = trianglesInNextRing;
            trianglesInNextRing = new List<int>();
            if (!gettingCloser) {
                break;
            }
            count++;
        }
        hit.collider = mc;
        tOld = hit.triangleIndex;
        hit.success = true;
        return hit;
    }

    private void AddTrianglesToRing(List<int> triangleRing, int triangleIdx, int[] visited) {
        Mesh mesh = mf.sharedMesh;
        int vertexA = mesh.triangles[3 * triangleIdx];
        int vertexB = mesh.triangles[3 * triangleIdx + 1];
        int vertexC = mesh.triangles[3 * triangleIdx + 2];
        IEnumerable<int> intersections = vertexToTriangle[vertexA].Intersect(vertexToTriangle[vertexB]);
        foreach (int triangle in intersections) {
            if (visited[triangle]==0) {
                triangleRing.Add(triangle);
            }
        }
        intersections = vertexToTriangle[vertexB].Intersect(vertexToTriangle[vertexC]);
        foreach (int triangle in intersections) {
            if (visited[triangle] == 0) {
                triangleRing.Add(triangle);
            }
        }

        intersections = vertexToTriangle[vertexC].Intersect(vertexToTriangle[vertexA]);
        foreach (int triangle in intersections) {
            if (visited[triangle] == 0) {
                triangleRing.Add(triangle);
            }
        }
    }

    public CustomHitInfo ProjectUsingPhong(Vector3 position, GameObject modelContainer)
    {
        CustomHitInfo hit = new CustomHitInfo();
        Vector3 positionModelSpace = Globals.ChangeHandedness(mf.gameObject.transform.InverseTransformPoint(position));
        Vector3 projection = new Vector3();
        int triangleIdx = -1;
        var phong = modelContainer.GetComponent<ModelsController>().GetPhongProjector();
        var res = phong.Project(positionModelSpace, out projection, out triangleIdx);
        if (res == Phong.PhongProjectionResult.Success)
        {
            hit.collider = mc;
            hit.point = mf.gameObject.transform.TransformPoint(Globals.ChangeHandedness(projection));
            hit.normal = mf.gameObject.transform.TransformVector(Globals.ChangeHandedness(phong.GetTriangleNormal(triangleIdx)));
            hit.triangleIndex = triangleIdx;
            hit.success = true;
        }
        else
        {
            Debug.LogWarning("Phong projection failed! Projection result: " + res.ToString());
            hit = GlobalClosestHit(position);
            hit.success = false;
        }

        return hit;
    }

    public CustomHitInfo GlobalClosestHit(Vector3 position) {
        Mesh mesh = mf.sharedMesh;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        Vector3 positionModelSpace = mf.gameObject.transform.InverseTransformPoint(position);
        int vertexIdx = kdTree.nearest(positionModelSpace);
        float minDistance = 100000f;
        CustomHitInfo hit = new CustomHitInfo();
        hit.point = mf.gameObject.transform.TransformPoint(vertices[vertexIdx]);
        foreach (var triangleIdx in vertexToTriangle[vertexIdx]) {
            Vector3 a = vertices[triangles[3 * triangleIdx]];
            Vector3 b = vertices[triangles[3 * triangleIdx + 1]];
            Vector3 c = vertices[triangles[3 * triangleIdx + 2]];
            Vector3 s = ClosestPointOnTriangle(a, b, c, positionModelSpace);
            float distance = Vector3.Distance(s, positionModelSpace);
            if (distance < minDistance) {
                minDistance = distance;
                hit.point = mf.gameObject.transform.TransformPoint(s);
                Vector3 n = Vector3.Cross(b - a, c - a);
                n.Normalize();
                hit.normal = n;
                hit.triangleIndex = triangleIdx;
            }
        }
        hit.collider = mc;
        hit.success = true;
        return hit;
    }

    public void MapVertexToTriangles(MeshFilter meshFilter, MeshCollider meshCollider) {
        mf = meshFilter;
        mc = meshCollider;
        int vertexSize = mf.sharedMesh.vertices.Length; 
        vertexToTriangle = new List<int>[vertexSize];
        for (int i = 0; i < vertexSize; i++) {
            vertexToTriangle[i] = new List<int>();
        }
        int[] triangles = mf.sharedMesh.triangles;
        int triSize = triangles.Length / 3;
        for (int i = 0; i < triSize; ++i) {
            vertexToTriangle[triangles[3 * i]].Add(i);
            vertexToTriangle[triangles[3 * i + 1]].Add(i);
            vertexToTriangle[triangles[3 * i + 2]].Add(i);
        }
    }

    private Vector3 ClosestPointOnTriangle(Vector3 a, Vector3 b, Vector3 c, Vector3 poi) {
        Plane p = new Plane(a, b, c);
        Vector3 s = p.ClosestPointOnPlane(poi);
        if(PointInTri(a,b,c,s)) {
            return s;
        }
        else {
            Vector3 pSide1 = PointOnLine(a, b, poi);
            Vector3 pSide2 = PointOnLine(b, c, poi);
            Vector3 pSide3 = PointOnLine(c, a, poi);
            if(Vector3.Distance(pSide1, poi) < Vector3.Distance(pSide2, poi)) {
                s = pSide1;
            }
            else {
                s = pSide2;
            }

            if (Vector3.Distance(pSide3, poi) < Vector3.Distance(s, poi)) {
                s = pSide3;
            }
        }

        return s;
    }

    Boolean PointInTri(Vector3 a, Vector3 b, Vector3 c, Vector3 p) {
        Vector3 v0 = b - c;
        Vector3 v1 = a - c;
        Vector3 v2 = p - c;
        float dot00 = Vector3.Dot(v0, v0);
        float dot01 = Vector3.Dot(v0, v1);
        float dot02 = Vector3.Dot(v0, v2);
        float dot11 = Vector3.Dot(v1, v1);
        float dot12 = Vector3.Dot(v1, v2);
        float invDenom = 1.0f / (dot00 * dot11 - dot01 * dot01);
        float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
        float v = (dot00 * dot12 - dot01 * dot02) * invDenom;
        return (u > 0.0f)&&(v > 0.0f)&&(u + v < 1.0f);
    }

    Vector3 PointOnLine(Vector3 a, Vector3 b, Vector3 poi) {
        Vector3 side = b - a;
        Vector3 projectedPoint = Vector3.Project(poi-a, side) + a;
        float t1 = Vector3.Dot(projectedPoint - a, side);
        float t2 = Vector3.Dot(b - projectedPoint, side);
        if (t1>=0f && t2>=0f) {
            return projectedPoint;
        }
        else if (Vector3.Distance(a, poi) < Vector3.Distance(b, poi)) {
            return a;
        }
        else {
            return b;
        }
    }
}
