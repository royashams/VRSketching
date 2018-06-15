using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawingCursor : MonoBehaviour {


    // Update is called once per frame
    void LateUpdate() {
        Vector3 origin = gameObject.transform.parent.parent.TransformPoint(0f, -0.1f, 0.05f);
        gameObject.transform.position = (gameObject.transform.parent.position + origin) / 2.0f;
        gameObject.transform.up = (gameObject.transform.parent.position - origin).normalized;
        //I honestly do not understand why we need a divide by two here
        float globalScale = Vector3.Distance(gameObject.transform.parent.position, origin) / 2f;
        Transform parent = gameObject.transform.parent;
        gameObject.transform.parent = null;
        gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x, globalScale, gameObject.transform.localScale.z);
        gameObject.transform.parent = parent;
    }
}
