using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectionCursor : MonoBehaviour {
    public Transform start;
    public Transform end;
	
	// Update is called once per frame
	void Update () {
        Vector3 startTip = start.TransformPoint(0f, -0.1f, 0.05f);
        gameObject.transform.position = (startTip + end.position) / 2.0f;
        gameObject.transform.up = end.position - startTip;
        gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x, (end.position - startTip).magnitude/2.0f, gameObject.transform.localScale.z);
	}
}
