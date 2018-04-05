using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartitionMesh : MonoBehaviour {
    public struct CustomHitInfo {
        public Vector3 point;
        public Vector3 normal;
        public int triangleIndex;
        public Collider collider;
    }

    private KdTree kdTree;
    /*private float midX;
    private float midY;
    private float midZ;
    private List<int>[] verticeMap;*/
    private List<int>[] vertexToTriangle;
    /*private List<List<Vector3>> partitionedVertices;
    private int numPartition = 8;*/
    private MeshFilter mf;
    private MeshCollider mc;
    // Use this for initialization
   private void Awake() {
        /*partitionedVertices = new List<List<Vector3>>();
        for (int i=0; i<numPartition; ++i) {
            partitionedVertices.Add(new List<Vector3>());
        }*/
        kdTree = gameObject.GetComponent<KdTree>();
    }

    /*public void PartitionMeshWorldSpace(MeshFilter meshFilter, MeshCollider meshCollider) {
        mf = meshFilter;
        mc = meshCollider;
        Vector3[] vertices = meshFilter.sharedMesh.vertices;
        Vector3[] normals = meshFilter.sharedMesh.normals;
        int[] triangles = meshFilter.sharedMesh.triangles;
        GameObject model = meshFilter.gameObject;
        float minX = 10000f;
        float minY = 10000f;
        float minZ = 10000f;
        float maxX = -10000f;
        float maxY = -10000f;
        float maxZ = -10000f;
        int size = vertices.Length;
        verticeMap = new List<int>[numPartition];
        vertexToTriangle = new List<int>[size];
        for (int i = 0; i < numPartition; ++i) {
            partitionedVertices[i].Clear();
            verticeMap[i] = new List<int>();
        }
        for (int i=0; i<size; i++) {
            vertexToTriangle[i] = new List<int>();
            Vector3 vertexWorld = model.transform.TransformPoint(vertices[i]);
            vertices[i] = vertexWorld;
            if (vertexWorld.x < minX) {
                minX = vertexWorld.x;
            }
            if (vertexWorld.x > maxX) {
                maxX = vertexWorld.x;
            }

            if (vertexWorld.y < minY) {
                minY = vertexWorld.y;
            }
            if (vertexWorld.y > maxY) {
                maxY = vertexWorld.y;
            }

            if (vertexWorld.z < minZ) {
                minZ = vertexWorld.z;
            }
            if (vertexWorld.z > maxZ) {
                maxZ = vertexWorld.z;
            }
        }
        midX = (maxX + minX) / 2;
        midY = (maxY + minY) / 2;
        midZ = (maxZ + minZ) / 2;
        for(int i=0; i<size; ++i) {
            if (vertices[i].y > midY) {
                if (vertices[i].x > midX) {
                    if (vertices[i].z > midZ) {
                        partitionedVertices[0].Add(vertices[i]);
                        verticeMap[0].Add(i);

                    }
                    else {
                        partitionedVertices[1].Add(vertices[i]);
                        verticeMap[1].Add(i);
                    }
                }
                else {
                    if (vertices[i].z > midZ) {
                        partitionedVertices[2].Add(vertices[i]);
                        verticeMap[2].Add(i);
                    }
                    else {
                        partitionedVertices[3].Add(vertices[i]);
                        verticeMap[3].Add(i);
                    }
                }
            }
            else {
                if (vertices[i].x > midX) {
                    if (vertices[i].z > midZ) {
                        partitionedVertices[4].Add(vertices[i]);
                        verticeMap[4].Add(i);
                    }
                    else {
                        partitionedVertices[5].Add(vertices[i]);
                        verticeMap[5].Add(i);
                    }
                }
                else {
                    if (vertices[i].z > midZ) {
                        partitionedVertices[6].Add(vertices[i]);
                        verticeMap[6].Add(i);
                    }
                    else {
                        partitionedVertices[7].Add(vertices[i]);
                        verticeMap[7].Add(i);
                    }
                }
            }
        }

        int triSize = triangles.Length / 3;
        for (int i=0; i<triSize; ++i) {
            vertexToTriangle[triangles[3 * i]].Add(i);
            vertexToTriangle[triangles[3 * i + 1]].Add(i);
            vertexToTriangle[triangles[3 * i + 2]].Add(i);
        }
    }*/

    public CustomHitInfo ClosestHit(Vector3 position) {
        Mesh mesh = mf.sharedMesh;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        Vector3 positionModelSpace = mf.gameObject.transform.InverseTransformPoint(position);
        int vertexIdx = kdTree.nearest(positionModelSpace);
        float minDistance = 100000f;
        CustomHitInfo hit = new CustomHitInfo();
        hit.point = mf.gameObject.transform.TransformPoint(vertices[vertexIdx]);
        foreach(var triangleIdx in vertexToTriangle[vertexIdx]) {
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
        return hit;
    }

    /*private int closestPartition(Vector3 position) {
        if (position.y > midY) {
            if (position.x > midX) {
                if (position.z > midZ) {
                    return 0;
                }
                else {
                    return 1;
                }
            }
            else {
                if (position.z > midZ) {
                    return 2;
                }
                else {
                    return 3;
                }
            }
        }
        else {
            if (position.x > midX) {
                if (position.z > midZ) {
                    return 4;
                }
                else {
                    return 5;
                }
            }
            else {
                if (position.z > midZ) {
                    return 6;
                }
                else {
                    return 7;
                }
            }
        }
    }

    private int ClosestVertexIdxInPartition(int partitionIdx, Vector3 position) {
        float minDistance = 100000f;
        int closestVertexIdx = 0;
        int size = partitionedVertices[partitionIdx].Count;
        for(int i=0; i<size; ++i) {
            float distance = Vector3.Distance(partitionedVertices[partitionIdx][i], position);
            if (distance < minDistance) {
                closestVertexIdx = i;
                minDistance = distance;
            }
        }
        return closestVertexIdx;

    }*/

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
        Vector3 projectedPoint = Vector3.Project(poi, side);
        float t1 = Vector3.Dot(projectedPoint - a, side);
        float t2 = Vector3.Dot(b - projectedPoint, side);
        if (t1 >= 0f && t2 >= 0f) {
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
