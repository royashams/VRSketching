using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleText : MonoBehaviour {

	// Use this for initialization
	public string initialValue;
	public string toggleValue;
	private Text text;
	void Awake () {
		text = gameObject.GetComponent<Text> ();
		text.text = initialValue;
	}
	
	// Update is called once per frame
	public void Toggle () {
		if (text.text == initialValue) {
			text.text = toggleValue;
		}
		else {
			text.text = initialValue;
		}
	}
}
