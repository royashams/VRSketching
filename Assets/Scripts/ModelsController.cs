using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class ModelsController : MonoBehaviour {
    public GameObject sketch;
    public GameObject Projectionsketch;
    //public SteamVR_TrackedObject trackedObj;
    //private SteamVR_Controller.Device controller {
    //    get { return SteamVR_Controller.Input((int)trackedObj.index); }
    //}
    private int modelIdx;
    private GameObject curModel;
    private PartitionMesh pm;
    private KdTree kdTree;
    private Phong.PhongProjection phong;
    // Use this for initialization
    void Start() {
        //phong = GetComponent<Phong.PhongProjection>();
        pm = GetComponent<PartitionMesh>();
        kdTree = GetComponent<KdTree>();
        phong = new Phong.PhongProjection();
        if (gameObject.transform.childCount > 0) {
            modelIdx = 0;
            curModel = gameObject.transform.GetChild(modelIdx).gameObject;
            curModel.SetActive(true);
            phong.modelName = curModel.GetComponent<ModelInfo>().name;
            phong.Init();
            MeshFilter mf = GetCurModelComponent<MeshFilter>();
            MeshCollider mc = GetCurModelComponent<MeshCollider>();
            if (mf) {
                kdTree.build(mf.sharedMesh.vertices);
                pm.MapVertexToTriangles(mf, mc);
            }           
        }
    }

    // Update is called once per frame
    void LateUpdate() {
        //if ((controller.GetPress(SteamVR_Controller.ButtonMask.Grip) && controller.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad)) || Input.GetKeyDown(KeyCode.Return)) {
        if (SteamVR_Actions.default_SaveButton.GetState(Globals.HAND) || Input.GetKeyDown(KeyCode.Return))
        {
            SaveDrawing();
            curModel.SetActive(false);
            if (modelIdx + 1 < gameObject.transform.childCount) {
                modelIdx++;
                curModel = gameObject.transform.GetChild(modelIdx).gameObject;
                curModel.SetActive(true);
                phong.modelName = curModel.GetComponent<ModelInfo>().name;
                phong.Init();
                MeshFilter mf = GetCurModelComponent<MeshFilter>();
                MeshCollider mc = GetCurModelComponent<MeshCollider>();
                if (mf) {
                    kdTree.build(mf.sharedMesh.vertices);
                    pm.MapVertexToTriangles(mf, mc);
                }
            }
            else {
                this.enabled = false;
            }
        }
    }

    void SaveDrawing () {
		// TODO: Actually saving the stuff!!
		Draw draw = sketch.GetComponent<Draw> ();
		draw.ns.Clear ();
		draw.points.Clear ();
		draw.normals.Clear ();
		draw.timestamps.Clear ();
		foreach(Transform child in sketch.transform) {
            Destroy(child.gameObject);
        }
        draw = Projectionsketch.GetComponent<Draw>();
        draw.ns.Clear();
        draw.points.Clear();
        draw.normals.Clear();
        draw.timestamps.Clear();
        foreach (Transform child in Projectionsketch.transform) {
            Destroy(child.gameObject);
        }
    }

    public void ToggleModelRenderer() {
        Renderer rend = GetCurModelComponent<Renderer>();
        if (rend.enabled) {
            rend.enabled = false;
        }
        else {
            rend.enabled = true;
        }
    }


    public T GetCurModelComponent<T>() {
        T component = curModel.GetComponent<T>();
        if (component.Equals(null)) {
            T[] components = curModel.GetComponentsInChildren<T>();
            if (components.Length != 1) {
                Debug.Log("Model" + curModel.name + "has zero or more than one.");
            }
            else {
                component = components[0];
            }
        }
        return component;
    }

    public GameObject GetCurrentModel()
    {
        return curModel;
    }
}