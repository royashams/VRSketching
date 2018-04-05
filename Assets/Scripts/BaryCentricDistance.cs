using UnityEngine;
using System.Collections;
using System.Linq;

public class BaryCentricDistance : MonoBehaviour {


    public MeshFilter mf;
    
    public struct Result {
        public float distanceSquared;
        public float distance {
            get {
                return Mathf.Sqrt(distanceSquared);
            }
        }

        public int triangle;
        public Vector3 normal;
        public Vector3 centre;
        public Vector3 closestPoint;
    }


    public Result GetClosestTriangleAndPoint(Vector3 point) {
        point = mf.transform.InverseTransformPoint(point);
        var minDistance = float.PositiveInfinity;
        var finalResult = new Result();
        var length = (int)(mf.sharedMesh.triangles.Length / 3);
        for (var t = 0; t < length; t++) {
            var result = GetTriangleInfoForPoint(point, t);
            if (minDistance > result.distanceSquared) {
                minDistance = result.distanceSquared;
                finalResult = result;
            }
        }
        finalResult.centre = mf.transform.TransformPoint(finalResult.centre);
        finalResult.closestPoint = mf.transform.TransformPoint(finalResult.closestPoint);
        finalResult.normal = mf.transform.TransformDirection(finalResult.normal);
        finalResult.distanceSquared = (finalResult.closestPoint - point).sqrMagnitude;
        return finalResult;
    }

    Result GetTriangleInfoForPoint(Vector3 point, int triangle) {
        Result result = new Result();

        result.triangle = triangle;
        result.distanceSquared = float.PositiveInfinity;

        if (triangle >= mf.sharedMesh.triangles.Length / 3)
            return result;


        //Get the vertices of the triangle
        var p1 = mf.sharedMesh.vertices[mf.sharedMesh.triangles[0 + triangle * 3]];
        var p2 = mf.sharedMesh.vertices[mf.sharedMesh.triangles[1 + triangle * 3]];
        var p3 = mf.sharedMesh.vertices[mf.sharedMesh.triangles[2 + triangle * 3]];

        result.normal = Vector3.Cross((p2 - p1).normalized, (p3 - p1).normalized);

        //Project our point onto the plane
        var projected = point + Vector3.Dot((p1 - point), result.normal) * result.normal;

        //Calculate the barycentric coordinates
        var u = ((projected.x * p2.y) - (projected.x * p3.y) - (p2.x * projected.y) + (p2.x * p3.y) + (p3.x * projected.y) - (p3.x * p2.y)) /
                ((p1.x * p2.y) - (p1.x * p3.y) - (p2.x * p1.y) + (p2.x * p3.y) + (p3.x * p1.y) - (p3.x * p2.y));
        var v = ((p1.x * projected.y) - (p1.x * p3.y) - (projected.x * p1.y) + (projected.x * p3.y) + (p3.x * p1.y) - (p3.x * projected.y)) /
                ((p1.x * p2.y) - (p1.x * p3.y) - (p2.x * p1.y) + (p2.x * p3.y) + (p3.x * p1.y) - (p3.x * p2.y));
        var w = ((p1.x * p2.y) - (p1.x * projected.y) - (p2.x * p1.y) + (p2.x * projected.y) + (projected.x * p1.y) - (projected.x * p2.y)) /
                ((p1.x * p2.y) - (p1.x * p3.y) - (p2.x * p1.y) + (p2.x * p3.y) + (p3.x * p1.y) - (p3.x * p2.y));

        result.centre = p1 * 0.3333f + p2 * 0.3333f + p3 * 0.3333f;

        //Find the nearest point
        var vector = (new Vector3(u, v, w)).normalized;


        //work out where that point is
        var nearest = p1 * vector.x + p2 * vector.y + p3 * vector.z;
        result.closestPoint = nearest;
        result.distanceSquared = (nearest - point).sqrMagnitude;

        if (float.IsNaN(result.distanceSquared)) {
            result.distanceSquared = float.PositiveInfinity;
        }
        return result;
    }

}
