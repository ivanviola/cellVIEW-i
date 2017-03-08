using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class SelectionManager : MonoBehaviour
{
    public TransformHandle _selectedTransformHandle;
    public HandleSelectionState _currentState = HandleSelectionState.Translate;
    //private TransformHandle _selectedTransformHandle;
    //private HandleSelectionState _currentState = HandleSelectionState.Translate;

    // Declare the scene manager as a singleton
    private static SelectionManager _instance = null;
    public static SelectionManager Instance
    {
        get
        {
            if (_instance != null) return _instance;

            _instance = FindObjectOfType<SelectionManager>();
            if (_instance == null)
            {
                var go = GameObject.Find("_SelectionManager");
                if (go != null) DestroyImmediate(go);

                go = new GameObject("_SelectionManager") { hideFlags = HideFlags.HideInInspector };
                _instance = go.AddComponent<SelectionManager>();
            }
            return _instance;
        }
    }

    //--------------------------------------------------------------

    private int _selectedObjectID = -1;

    private GameObject _selectionGameObject;
    public GameObject SelectionGameObject
    {
        get
        {
            if (_selectionGameObject != null) return _selectionGameObject;

            var go = GameObject.FindGameObjectWithTag("Selection");
            if (go != null) _selectionGameObject = go;
            else
            {
                _selectionGameObject = new GameObject("Selection");
                _selectionGameObject.tag = "Selection";
                _selectionGameObject.AddComponent<SphereCollider>();
                _selectionGameObject.AddComponent<TransformHandle>();
            }

            return _selectionGameObject;
        }
    }

    //*****//

    public bool MouseRightClickFlag = false;
    public Vector2 MousePosition = new Vector2();

    private bool _ctrlKeyFlag = false;
    private float _leftClickTimeStart = 0;
    private float _rightClickTimeStart = 0;
    
    void OnEnable()
    {
#if UNITY_EDITOR
        SceneView.onSceneGUIDelegate = null;
        SceneView.onSceneGUIDelegate += OnSceneGUI;
#endif
    }

#if UNITY_EDITOR
    public void OnSceneGUI(SceneView sceneView)
    {
        // Select objects with right mouse
        if (Event.current.type == EventType.mouseDown && Event.current.button == 0)
        {
            _ctrlKeyFlag = Event.current.control;
            MouseRightClickFlag = true;
            MousePosition = Event.current.mousePosition;
        }
    }
#endif

    public void OnGUI()
    {
        if (Event.current.keyCode == KeyCode.Alpha1)
        {
            _currentState = HandleSelectionState.Translate;
            if (_selectedTransformHandle)
            {
                _selectedTransformHandle.SetSelectionState(_currentState);
            }
        }

        if (Event.current.keyCode == KeyCode.Alpha2)
        {
            _currentState = HandleSelectionState.Rotate;
            if (_selectedTransformHandle)
            {
                _selectedTransformHandle.SetSelectionState(_currentState);
            }
        }

        if (Event.current.keyCode == KeyCode.Alpha3)
        {
            _currentState = HandleSelectionState.Scale;
            if (_selectedTransformHandle)
            {
                _selectedTransformHandle.SetSelectionState(_currentState);
            }
        }

        // Select objects with right mouse
        if (!Event.current.alt && MouseLeftClickTest())
        {
            _ctrlKeyFlag = Event.current.control;
            MouseRightClickFlag = true;
            MousePosition = Event.current.mousePosition;
        }

        // Select cut objects with left mouse
        if (!Event.current.alt && MouseLeftClickTest())
        {
            if (_selectedTransformHandle && _selectedTransformHandle.FreezeObjectPicking)
            {
                _selectedTransformHandle.FreezeObjectPicking = false;
            }
            else
            {
                DoCutObjectPicking();
            }
        }
    }

    private MainCameraController controller;

    
    public void SetSelectedObject(int instanceID)
    {
        if (instanceID >= CPUBuffers.Get.ProteinInstancePositions.Count) return;

        Debug.Log("*****");

        //if (!ValidateInstanceID(_selectedObjectID)) return;

        Debug.Log("Selected element id: " + instanceID); if (instanceID >= CPUBuffers.Get.ProteinInstancePositions.Count) return;
        if (instanceID > 0) Debug.Log("Selected element type: " + CPUBuffers.Get.ProteinInstanceInfos[instanceID].x);
        if (instanceID > 0) Debug.Log("Selected element name: " + SceneManager.Get.ProteinIngredientNames[(int)CPUBuffers.Get.ProteinInstanceInfos[instanceID].x]);
        
        // If element id is different than the currently selected element
        if (_selectedObjectID != instanceID)
        {
            // if new selected element is greater than one update set and set position to game object
            if (instanceID > -1 )
            {
                if (_ctrlKeyFlag)
                {
                    float radius = CPUBuffers.Get.ProteinIngredientsRadii[(int)CPUBuffers.Get.ProteinInstanceInfos[instanceID].x] * GlobalProperties.Get.Scale;

                    SelectionGameObject.GetComponent<SphereCollider>().radius = radius;

                    SelectionGameObject.transform.position = CPUBuffers.Get.ProteinInstancePositions[instanceID] * GlobalProperties.Get.Scale;
                    SelectionGameObject.transform.rotation = MyUtility.Vector4ToQuaternion(CPUBuffers.Get.ProteinInstanceRotations[instanceID]);

                    // Enable handle
                    SelectionGameObject.GetComponent<TransformHandle>().Enable();
                    //MainCamera.main.GetComponent<NavigateCamera>().TargetGameObject = SelectionGameObject;
                    if (controller == null)
                    {
                        controller = GameObject.FindObjectOfType<MainCameraController>();
                    }
                    controller.TargetTransform = SelectionGameObject.transform; 

                    if (_selectedTransformHandle)
                    {
                        _selectedTransformHandle.Disable();
                        _selectedTransformHandle = null;
                    }

                    _ctrlKeyFlag = false;
                    _selectedObjectID = instanceID;
                    _selectedTransformHandle = SelectionGameObject.GetComponent<TransformHandle>();
#if UNITY_EDITOR
                    Selection.activeGameObject = SelectionGameObject;
#endif
                }

            }
            else
            {
                // Disable handle
                SelectionGameObject.GetComponent<TransformHandle>().Disable();
                _selectedObjectID = instanceID;
            }
        }
    }

    // This gets called from CutObject.SetHidden() so that's why you need to keep _currentState and _selectedTransformHandle updated in here (it's kinda stupid...)
    public void SetHandleSelected(TransformHandle handle)
    {
        handle.Enable();
        handle.SetSelectionState(_currentState);
        _selectedTransformHandle = handle;
    }

    MainCameraController _mainCameraController;

    private void DoCutObjectPicking()
    {
        var mousePos = Event.current.mousePosition;
        Ray CameraRay = Camera.main.ScreenPointToRay(new Vector3(mousePos.x, Screen.height - mousePos.y, 0));
        RaycastHit hit;

        // If we hit an object
        if (Physics.Raycast(CameraRay, out hit, 1000))
        {
            var cutObject = hit.collider.gameObject.GetComponent<CutObject>();
            var transformHandle = hit.collider.gameObject.GetComponent<TransformHandle>();

            // If we hit a new selectable object
            if (cutObject && transformHandle && transformHandle != _selectedTransformHandle)
            {
                if (_selectedTransformHandle != null)
                {
                    //Debug.Log("Reset");
                    _selectedTransformHandle.Disable();
                }

                Debug.Log("Selected transform: " + transformHandle.gameObject.name);

                if (SelectionGameObject && SelectionGameObject.GetComponent<TransformHandle>())
                {
                    SelectionGameObject.GetComponent<TransformHandle>().Disable();
                }

                transformHandle.Enable();
                transformHandle.SetSelectionState(_currentState);
                _selectedTransformHandle = transformHandle;

                if (_mainCameraController == null)
                    _mainCameraController = GameObject.FindObjectOfType<MainCameraController>();

                _mainCameraController.TargetTransform = hit.collider.gameObject.transform;
                //MainCamera.main.GetComponent<NavigateCamera>().TargetGameObject = hit.collider.gameObject;
            }
            // If we hit a non-selectable object
            else if (transformHandle == null && _selectedTransformHandle != null)
            {
                //Debug.Log("Missed hit");
                _selectedTransformHandle.Disable();
                _selectedTransformHandle = null;
            }
        }
        // If we miss a hit
        else if (_selectedTransformHandle != null)
        {
            //Debug.Log("Missed hit");
            _selectedTransformHandle.Disable();
            _selectedTransformHandle = null;
        }
    }
    

    // Update is called once per frame
    void Update ()
    {
        //UpdateSelectedElement();
    }
    
    private bool ValidateInstanceID(int value)
    {
        if (value > 100000) return false;
        if (value < 0 || value > SceneManager.Get.NumProteinInstances) return false;
        return true;
    }

    private void UpdateSelectedElement()
    {
        if (!ValidateInstanceID(_selectedObjectID)) _selectedObjectID = -1;

        if (_selectedObjectID == -1)
        {
            //SelectedElement.SetActive(false);
            return;
        }

        if (_selectionGameObject.transform.hasChanged)
        {
            //Debug.Log("Selected instance transform changed");

            CPUBuffers.Get.ProteinInstancePositions[_selectedObjectID] = _selectionGameObject.transform.position / GlobalProperties.Get.Scale;
            CPUBuffers.Get.ProteinInstanceRotations[_selectedObjectID] = MyUtility.QuanternionToVector4(_selectionGameObject.transform.rotation);

            GPUBuffers.Get.ProteinInstancePositions.SetData(CPUBuffers.Get.ProteinInstancePositions.ToArray());
            GPUBuffers.Get.ProteinInstanceRotations.SetData(CPUBuffers.Get.ProteinInstanceRotations.ToArray());

            _selectionGameObject.transform.hasChanged = false;
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

    bool MouseRightClickTest()
    {
        var rightClick = false;

        if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
        {
            _leftClickTimeStart = Time.realtimeSinceStartup;
        }

        if (Event.current.type == EventType.MouseDrag && Event.current.button == 1)
        {
            _leftClickTimeStart = 0;
        }

        if (Event.current.type == EventType.MouseUp && Event.current.button == 1)
        {
            var delta = Time.realtimeSinceStartup - _leftClickTimeStart;
            if (delta < 0.5f)
            {
                rightClick = true;
            }
        }

        return rightClick;
    }
}
