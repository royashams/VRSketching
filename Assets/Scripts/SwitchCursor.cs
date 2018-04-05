using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchCursor : MonoBehaviour {

	// Use this for initialization
	public void Switch() {
        if (transform.childCount < 2) {
            return;
        }

        if (transform.GetChild(0).gameObject.activeSelf) {
            transform.GetChild(0).gameObject.SetActive(false);
            transform.GetChild(1).gameObject.SetActive(true);
        }
        else {
            transform.GetChild(0).gameObject.SetActive(true);
            transform.GetChild(1).gameObject.SetActive(false);
        }
    }
}
