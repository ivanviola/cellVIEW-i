using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;


[ExecuteInEditMode]
public class NewSelectionManager : MonoBehaviour
{
    

    public Vector2              MousePosition = new Vector2();
    public bool                 ClickingDisabled = false;

    public List<int> SelectedGroupInstanceIndices; // this is where I save indices of highlighted instances so that I can unhighlight it without going
                                                   // through the whole buffer                                     
    public List<int> SelectedGroupLipidInstanceIndices; // same thing, just for lipids

    private MainCameraController            _cameraController = null;
    private PanelsUIController              _panelsUIController = null;
    private static NewSelectionManager      _instance = null;
    private TreeViewController              _treeViewController = null;

    private int                             _selectedProteinInstanceId = -1;
    private int                             _selectedLipidInstanceId = -1;
    private bool                            _rnaSelected = false;

    private int                             _hoveredInstanceId = -1;
    private string                          _hierarchySelectedItemPath = "";
    
    private bool                            _mouseClicked = false;
    private float                           _leftClickTimeStart = 0;

    private float                           _timeForDoubleClickInS = 0.5f;
    private float                           _firstClickStart;
    private bool                            _firstClickDetected = false;

    public static NewSelectionManager Instance
    {
        get
        {
            if (_instance != null) return _instance;

            _instance = FindObjectOfType<NewSelectionManager>();
            if (_instance == null)
            {
                var go = GameObject.Find("_NewSelectionManager");
                if (go != null) DestroyImmediate(go);

                go = new GameObject("_NewSelectionManager") { hideFlags = HideFlags.HideInInspector };
                _instance = go.AddComponent<NewSelectionManager>();
            }
            return _instance;
        }
    }

    public int SelectedInstanceId2
    {
        get
        {
            if (_selectedProteinInstanceId != -1 && _selectedLipidInstanceId == -1 && !_rnaSelected)
            {
                return _selectedProteinInstanceId;
            }
            else if (_selectedProteinInstanceId == -1 && _selectedLipidInstanceId != -1 && !_rnaSelected)
            {
                return _selectedLipidInstanceId + 100000;
            }
            else if (_selectedProteinInstanceId == -1 && _selectedLipidInstanceId == -1 && _rnaSelected)
            {
                return -123;
            }
            else
            {
                return -1;
            }
        }
    }
    

    public string HierarchySelectedItemPath
    {
        get
        {
            return _hierarchySelectedItemPath;
        }
        set
        {
            //if (value != "") // if the path that is to be set as selected is actually something
            //{
            //    SelectedProteinInstanceId = -1;
            //    SelectedLipidInstanceId = -1;
            //    RnaSelected = false;
            //}
            _hierarchySelectedItemPath = value;
        }
    }

    public Vector3 SelectedInstancePosition
    {
        get
        {
            return CPUBuffers.Get.ProteinInstancePositions[_selectedProteinInstanceId];
        }
    }

    public Vector3 HoveredInstancePosition
    {
        get
        {
            if (_hoveredInstanceId >= 100000)
            {
                return CPUBuffers.Get.LipidInstancePositions[_hoveredInstanceId - 100000];
            } 
            else
            {
                return CPUBuffers.Get.ProteinInstancePositions[_hoveredInstanceId];
            }
        }
    }

    public bool RnaSelected
    {
        get
        {
            return _rnaSelected;
        }
        set
        {
            _rnaSelected = value;
        }
    }

    public int SelectedLipidInstanceId
    {
        get
        {
            return _selectedLipidInstanceId;
        }
        set
        {
            // This is for unhighlighting in the hierarchy
            //if (SomethingIsSelected)
            if (_hierarchySelectedItemPath != "")
            {   // reset highlighted groups
                foreach (var index in SelectedGroupInstanceIndices)
                {
                    UnhighlightProteinInstance(index);
                    _hierarchySelectedItemPath = "";
                }

                foreach (var index in SelectedGroupLipidInstanceIndices)
                {
                    UnhighlightLipidInstance(index);
                    _hierarchySelectedItemPath = "";
                }
                _treeViewController.SetSelectedNode(null);
            }

            var oldValue = _selectedLipidInstanceId;
            if (value == -1)
            {
                _selectedLipidInstanceId = -1;
            }
            else
            {
                _selectedLipidInstanceId = value - 100000;
            }

            if (oldValue == -1 && _selectedLipidInstanceId != -1)                                                       // Nothing -> Something Selected
            { 
                HighlightLipidInstance(_selectedLipidInstanceId);
            }
            else if (oldValue != _selectedLipidInstanceId && oldValue != -1 && _selectedLipidInstanceId != -1)        // Something -> Something Different Selected
            {
                UnhighlightLipidInstance(oldValue);
                HighlightLipidInstance(_selectedLipidInstanceId);
            }
            else if (oldValue != -1 && _selectedLipidInstanceId == -1)                                                  // Something -> Nothing Selected
            {
                UnhighlightLipidInstance(oldValue);
            }


            GPUBuffers.Get.LipidInstancesInfo2.SetData(CPUBuffers.Get.LipidInstanceInfos2.ToArray());
            //GPUBuffers.Get.ProteinInstancesInfo.SetData(CPUBuffers.Get.ProteinInstanceInfos.ToArray());
            //_treeViewController.UnhighlightSelected();
        }
    }

    public int SelectedProteinInstanceId
    {
        get
        {
            return _selectedProteinInstanceId;
        }
        set
        {

            // This is for unhighlighting in the hierarchy:
            // if we have something selected in hierarchy (some compartment) we want to deselect that - which is why we saved the indices
            // now we are selecting one instance, so we need to unselect the compartment first
            //if (SomethingIsSelected)
            if (_hierarchySelectedItemPath != "")
            {   // reset highlighted groups
                foreach (var index in SelectedGroupInstanceIndices)
                {
                    UnhighlightProteinInstance(index);
                    _hierarchySelectedItemPath = "";
                }

                foreach (var index in SelectedGroupLipidInstanceIndices)
                {
                    UnhighlightLipidInstance(index);
                    _hierarchySelectedItemPath = "";
                }
                _treeViewController.SetSelectedNode(null);
                _treeViewController.UnhighlightSelected();

            }

            // Selecting Individual Instances
            var oldValue = _selectedProteinInstanceId;
            var oldLipidValue = _selectedLipidInstanceId;
            _selectedProteinInstanceId = value;

            if (value == -1) // unselecting everything
            {
                //_hierarchySelectedItemPath = "";
                //TreeViewController.SetSelectedNode(null); // something like that
                //_treeViewController.SetSelectedNode(null);
                //InfoTextController.Get.HideInfo();
            }

            //if (oldValue == -1 && value != -1)                                  // Nothing -> Something Selected
            if (oldValue == -1 && value != -1)                                  // Nothing -> Something Selected
            { 
                // => set seleted instance to Highlighted
                HighlightProteinInstance(value);
            }
            //else if (oldValue != value && oldValue != -1 && value != -1)        // Something -> Something Different Selected
            else if (oldValue != value && oldValue != -1 && value != -1)        // Something -> Something Different Selected
            { 
                // => set old instance to Normal
                UnhighlightProteinInstance(oldValue);
                // => set new instance to Highlighted
                HighlightProteinInstance(value);
            }
            else if (oldValue != -1 && value == -1)                             // Something -> Nothing Selected
            { 
                UnhighlightProteinInstance(oldValue);
            }

            // Copy data with information about highlighted/not highlighted instances to GPU
            GPUBuffers.Get.ProteinInstancesInfo.SetData(CPUBuffers.Get.ProteinInstanceInfos.ToArray());
            //GPUBuffers.Get.LipidInstancesInfo2.SetData(CPUBuffers.Get.LipidInstanceInfos2.ToArray());
        }
    }

    public bool SomethingIsSelected
    {
        get
        { // there are two ways something can be selected - 1) by clicking on an instance 2) by clicking into the hierarchy
            return (_hierarchySelectedItemPath != "") || (_selectedProteinInstanceId != -1) || (_selectedLipidInstanceId != -1) || (_rnaSelected);
        }
    }

    public int HoveredInstanceId
    {
        get
        {
            return _hoveredInstanceId;
        }
        set
        {
            _hoveredInstanceId = value;
        }
    }


    public bool MouseClicked
    {
        get
        {
            var ret = _mouseClicked;
            _mouseClicked = false; // setting this back to false after the click has been already requested (and thus serviced) - I don't know if this will work...
            return ret;
        }
    }



    /******/

    void OnEnable()
    {
        if (_cameraController == null)
        {
            _cameraController = FindObjectOfType<MainCameraController>();
            _treeViewController = FindObjectOfType<TreeViewController>();
            _panelsUIController = FindObjectOfType<PanelsUIController>();
            // TODO make more robust
        }

    }

    void OnStart()
    {
        // Unhighlight all
        for (int i = 0; i < CPUBuffers.Get.ProteinInstanceInfos.Count; i++)
        {
            UnhighlightProteinInstance(i);
        }

        for (int i = 0; i < CPUBuffers.Get.LipidInstanceInfos2.Count; i++)
        {
            UnhighlightLipidInstance(i);
        }

        _selectedProteinInstanceId = -1;
        _selectedLipidInstanceId = -1;
        _hierarchySelectedItemPath = "";

        GPUBuffers.Get.ProteinInstancesInfo.SetData(CPUBuffers.Get.ProteinInstanceInfos.ToArray());
        GPUBuffers.Get.LipidInstancesInfo2.SetData(CPUBuffers.Get.LipidInstanceInfos2.ToArray());
    }

    /*
     * OnGUI is called for each event, so OnGUI is potentially called multiple times per frame
     */
    void OnGUI()
    {
        if (ClickingDisabled) // this is set to true when cursor is above some UI
        {
            return;
        }

        MousePosition = Event.current.mousePosition; // let's just try this - YUP, it works - I don't need to test for a mouse event, I can just set the mousePosition always
        
        //if (!Event.current.alt && MouseLeftClickTest())
        if (MouseLeftClickTest())
        {
            _mouseClicked = true;

            //var selectedInstanceId = NewSelectionManager.Instance.SelectedInstanceId2;
            //InfoTextController.Get.ShowInfoForInstance(selectedInstanceId);

            if (!_firstClickDetected)
            {
                _firstClickDetected = true;
                _firstClickStart = Time.realtimeSinceStartup;
            }
            else
            {
                if (Time.realtimeSinceStartup <= _firstClickStart + _timeForDoubleClickInS)
                {
                    //Debug.Log("DOUBLE CLICK");
                    _firstClickDetected = false;
                    
                    _cameraController.ZoomInOnSelected();
                }
                else
                {
                    //_firstClickDetected = false;
                    _firstClickStart = Time.realtimeSinceStartup;
                }
            }

            // hide cut object
            var co = CutObjectManager.Get.GetSelectedCutObject();
            co.SetHidden(true);

        }

        if (Event.current.control) // debug - just to free the camera without needing to click into an empty space
        {
            //_selectedInstanceId = -1;
            SelectedProteinInstanceId = -1;
        }
    }


    bool MouseLeftClickTest()
    {
        var leftClick = false;

        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            _leftClickTimeStart = Time.realtimeSinceStartup;
        }

        if (Event.current.type == EventType.MouseDrag && Event.current.button == 0)
        {
            _leftClickTimeStart = 0;
        }

        if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
        {
            var delta = Time.realtimeSinceStartup - _leftClickTimeStart;
            if (delta < 0.5f)
            {
                leftClick = true;
            }
        }

        return leftClick;
    }

    bool MouseDoubleClickTest()
    {
        return false;
    }

    void HighlightProteinInstance(int index)
    {
        var info = CPUBuffers.Get.ProteinInstanceInfos[index];
        CPUBuffers.Get.ProteinInstanceInfos[index] = new Vector4(info.x, (int)InstanceState.Highlighted, info.z, info.w);
    }

    void UnhighlightProteinInstance(int index)
    {
        var infoOld = CPUBuffers.Get.ProteinInstanceInfos[index];
        CPUBuffers.Get.ProteinInstanceInfos[index] = new Vector4(infoOld.x, (int)InstanceState.Normal, infoOld.z, infoOld.w);
    }

    void HighlightLipidInstance(int index)
    {
        var info = CPUBuffers.Get.LipidInstanceInfos2[index];
        CPUBuffers.Get.LipidInstanceInfos2[index] = new Vector4((int)InstanceState.Highlighted, info.y, info.z, info.w);
    }

    void UnhighlightLipidInstance(int index)
    {
        var info = CPUBuffers.Get.LipidInstanceInfos2[index];
        CPUBuffers.Get.LipidInstanceInfos2[index] = new Vector4((int)InstanceState.Normal, info.y, info.z, info.w);
    }
}
