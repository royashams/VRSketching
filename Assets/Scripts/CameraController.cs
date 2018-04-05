using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	public float mouseSensitivity = 2.0f;
	public float zoomSensitivity = 0.1f;
	public float translationSpeed = 0.2f;
	public float orbitDampening = 2f;
	private GameObject curModel;
    private Vector3 pivot;
    private Vector3 initialPos;
    private Quaternion initialRot;
	//private Vector3 oldScreenPos;
	// Use this for initialization
	void Awake ()
	{
		Vector3 angles = transform.eulerAngles;
		initialPos = transform.position;
		initialRot = transform.rotation;

    }
	
	// Update is called once per frame
	void LateUpdate ()
	{
		float distance = Vector3.Distance (Camera.main.transform.position, curModel.transform.position);
		Camera.main.transform.position += zoomSensitivity * distance * Input.GetAxis ("Mouse ScrollWheel") * Camera.main.transform.forward;		
		if (Input.GetMouseButton (2)) {
			TranslateCamera (0);
		}
		else if (Input.GetMouseButton (1)) {
			RotateCameraAroundOrigin ();
		}

	}

	void RotateCameraAroundOrigin ()
	{
        if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0) {
            float distance = Vector3.Distance(Camera.main.transform.position, curModel.transform.position);
            TranslateCamera(1);
            //transform.LookAt (curModel.transform);
            transform.LookAt(pivot);
            transform.position *= (distance / transform.position.magnitude);
        }
	}

	public static float ClampAngle (float angle, float min, float max)
	{
		if (angle < -360F)
			angle += 360F;
		if (angle > 360F)
			angle -= 360F;
		return Mathf.Clamp (angle, min, max);
	}

	void TranslateCamera (int flag)
	{
		float distance = Vector3.Distance (Camera.main.transform.position, curModel.transform.position);
		if (Input.GetAxis ("Mouse X") != 0 || Input.GetAxis ("Mouse Y") != 0) {
            Vector3 rightTranslation = Camera.main.transform.right * -Input.GetAxis("Mouse X") * mouseSensitivity * distance * translationSpeed;
            Vector3 upTranslation = Camera.main.transform.up * -Input.GetAxis("Mouse Y") * mouseSensitivity * distance * translationSpeed;
            Camera.main.transform.position += rightTranslation;
            Camera.main.transform.position += upTranslation;
            if (flag == 0) {
                pivot += rightTranslation;
                pivot += upTranslation;
            }
		}
	}

    public void SetCurModel(GameObject model) {
        curModel = model;
		transform.position = initialPos;
		transform.rotation = initialRot;
		transform.LookAt (curModel.transform);
		pivot = new Vector3(0f, 0f, 0f);
    }
}
