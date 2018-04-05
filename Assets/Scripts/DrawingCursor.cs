using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawingCursor : MonoBehaviour {


    // Update is called once per frame
    void Update() {
        gameObject.transform.position = (gameObject.transform.parent.position + gameObject.transform.parent.parent.position) / 2.0f;
        gameObject.transform.up = (gameObject.transform.parent.position - gameObject.transform.parent.parent.position).normalized;
    }
}
