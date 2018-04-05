using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : MonoBehaviour {
    private void Awake() {
        gameObject.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 0.4f;
        gameObject.transform.rotation = Camera.main.transform.rotation;
    }
    // Use this for initialization
    void Start () {
        gameObject.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 0.4f;
        gameObject.transform.rotation = Camera.main.transform.rotation;
    }
	
	// Update is called once per frame
	void Update () {
        gameObject.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 0.4f;
        gameObject.transform.rotation = Camera.main.transform.rotation;
    }
}
