using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct CutInfoStruct
{
    public Vector4 info;
    public Vector4 info2;
    public Vector4 info3;
}

public struct HistStruct
{
    public int parent; //also write data to this id, unless it is < 0

    public int all;
    public int cutaway;
    public int occluding;
    public int visible;

    public int pad0;
    public int pad1;
    public int pad2;
}

public class CutObjectManager : MonoBehaviour
{
    // Declare the manager as a singleton
    private static CutObjectManager _instance = null;
    public static CutObjectManager Get
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<CutObjectManager>();
                if (_instance == null)
                {
                    var go = GameObject.Find("_CutObjectManager");
                    if (go != null)
                        DestroyImmediate(go);

                    go = new GameObject("_CutObjectManager") { hideFlags = HideFlags.HideInInspector };
                    _instance = go.AddComponent<CutObjectManager>();
                }
            }
            return _instance;
        }
    }

    //************************************************************//

    [NonSerialized]
    public List<CutObject> CutObjects = new List<CutObject>();
    
    [NonSerialized]
    public int ResetCutSnapshot = -1;

    [NonSerialized]
    public int SelectedCutObject = 0;
    

    public CutObject GetSelectedCutObject()
    {
        return CutObjects[SelectedCutObject];
    }

    public List<CutObject> GetSelectedCutObjects()
    {
        var selectedCutObjects = new List<CutObject>();
        selectedCutObjects.Add(CutObjects[SelectedCutObject]);
        return selectedCutObjects;
    }

    // Todo: proceed only if changes are made 
    public void UpdateCutObjects()
    {
        var CutInfos = new List<CutInfoStruct>();
        var CutScales = new List<Vector4>();
        var CutPositions = new List<Vector4>();
        var CutRotations = new List<Vector4>();

        // For each cut object
        foreach (var cut in CutObjects)
        {
            if (cut == null) throw new Exception("Cut object not fofund");

            CutScales.Add(cut.transform.lossyScale);
            CutPositions.Add(cut.transform.position);
            CutRotations.Add(MyUtility.QuanternionToVector4(cut.transform.rotation));
            //CutInfos.Add(new Vector4((float)cut.CutType, cut.Value1, cut.Value2, cut.Inverse ? 1.0f : 0.0f));
        }

        foreach (var cut in CutObjects)
        {
            foreach (var cutParam in cut.IngredientCutParameters)
            {
                CutInfos.Add(new CutInfoStruct
                {
                    info = new Vector4((float)cut.CutType, cutParam.value1, cutParam.value2, cut.Inverse ? 1.0f : 0.0f),
                    info2 = new Vector4(cutParam.fuzziness, cutParam.fuzzinessDistance, cutParam.fuzzinessCurve, cutParam.Aperture),
                    info3 = new Vector4(0, 0, 0, Convert.ToSingle(cut.HardCut))
                });
            }
        }

        GPUBuffers.Get.CutInfo.SetData(CutInfos.ToArray());
        GPUBuffers.Get.CutScales.SetData(CutScales.ToArray());
        GPUBuffers.Get.CutPositions.SetData(CutPositions.ToArray());
        GPUBuffers.Get.CutRotations.SetData(CutRotations.ToArray());
        GPUBuffers.Get.IngredientEdgeOpacity.SetData(CPUBuffers.Get.IngredientEdgeOpacity.ToArray());
    }

    public void UpdateCutObjectParams()
    {
        CutObjects.Clear();
        foreach (var cutObject in FindObjectsOfType<CutObject>())
        {
            cutObject.InitCutParameters();
            CutObjects.Add(cutObject);
        }
    }

    public void ShowMoveHandle()
    {
        var co = CutObjects[SelectedCutObject];
        co.GetComponent<TransformHandle>().enabled = true;
        //var handle = co.GetComponent<TransformHandle>();
        //handle.SetSelectionState(HandleSelectionState.Translate);
        SelectionManager.Instance._currentState = HandleSelectionState.Translate;
        co.SetHidden(false, true);
    }

    public void ShowRotateHandle()
    {
        var co = CutObjects[SelectedCutObject];
        co.GetComponent<TransformHandle>().enabled = true;
        //co.GetComponent<TransformHandle>().SetSelectionState(HandleSelectionState.Rotate);
        SelectionManager.Instance._currentState = HandleSelectionState.Rotate;
        //var handle = co.GetComponent<TransformHandle>();
        //handle.SetSelectionState(HandleSelectionState.Rotate);
        co.SetHidden(false, true);
    }

    public void ShowScaleHandle()
    {
        var co = CutObjects[SelectedCutObject];
        co.GetComponent<TransformHandle>().enabled = true;
        //var handle = co.GetComponent<TransformHandle>();
        //handle.SetSelectionState(HandleSelectionState.Scale);
        SelectionManager.Instance._currentState = HandleSelectionState.Scale;
        co.SetHidden(false, true);
    }
}
