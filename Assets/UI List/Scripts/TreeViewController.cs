using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Schema;
using UnityEngine.EventSystems;
using UnityEngine.UI;



public class TreeViewController : MonoBehaviour, IEventSystemHandler
{

    public bool ExpertMode = false;
    public int TextFontSize;

    public float ArrowSize;
    public float TextFieldSize;

    public float Indent;
    public float Spacing;
    public float TopPadding;
    public float LeftPadding;
    public bool EnableLensEffect = false;

    public bool FoldedAtLaunch = false;

    public GameObject BaseItemPrefab;
    public CutObjectUIController cutObjectUiController;

    public int _rnaInstanceNodeId = -1;

    //*******//

    private BaseItem _selectedNode;
    private List<BaseItem> _rootNodes;


    private float _currentDragValue;
    private CutParameters _currentCutParameters;

    // Use this for initialization
    void Start()

    {
        if (_rootNodes == null) _rootNodes = new List<BaseItem>();

        // fix membrane order
        //SceneManager.Get.SceneHierarchy.Sort(); // nope

        foreach (var path in SceneManager.Get.SceneHierarchy)
        {
            AddNodeObject(path, new object[] { MyUtility.GetNameFromUrlPath(path) }, "Text");
        }
        InitNodeItems();

        // Register event callbacks
        foreach (var node in _rootNodes)
        {
            node.SetFoldedState(FoldedAtLaunch);
            node.PointerClick += OnNodePointerClick;
            node.PointerExit += OnNodePointerExit;
            node.PointerEnter += OnNodePointerEnter;
            node.RangeFieldItem.CustomRangeSliderUi.RangeSliderDrag += OnRangeSliderDrag;
        }

        SetSelectedNode(_rootNodes[0]);
        cutObjectUiController.OnSelectedCutObjectChange += OnSelectedCutObjectChange;

        foreach (var node in _rootNodes)
        {
            node.SwitchToMode(false);
        }

        OnNodePointerExit(null);
    }

    public void FixedUpdate()
    {
        UpdateNodeItems();
        UpdateHistograms();
    }

    public void SwitchToMode(bool toExpertMode)
    {
        ExpertMode = toExpertMode;

        foreach (var node in _rootNodes)
        {
            node.SwitchToMode(toExpertMode);
        }
        
    }

    public void UnhighlightSelected()
    {
        _selectedNode.RangeFieldItem.UnhighlightInHierarchy();
    }

    //public void SwitchMode()
    //{
    //    ExpertMode = !ExpertMode;
    //    var toExpertMode = ExpertMode;

    //    foreach (var node in _rootNodes)
    //    {
    //        node.SwitchToMode(toExpertMode);
    //    }

    //    var newText = toExpertMode ? "NORMAL MODE" : "EXPERT MODE";
    //    var button = GameObject.Find("Expert Mode Button").GetComponent<Button>();
    //    button.GetComponentInChildren<Text>().text = newText;
    //}

    //*** Selection ****//

    public void OnNodePointerEnter(BaseItem enterNode)
    {
        SceneManager.Get.AllIngredientStates.Clear();
        var selectedLeaveNames = GetAllLeaveNodesFrom(enterNode).Select(leave => leave.Path).ToList();

        for (int i = 0; i < SceneManager.Get.AllIngredientNames.Count; i++)
        {
            var highlighted = selectedLeaveNames.Contains(SceneManager.Get.AllIngredientNames[i]);
            SceneManager.Get.AllIngredientStates.Add(highlighted ? 2 : 1);
        }

        GPUBuffers.Get.IngredientStates.SetData(SceneManager.Get.AllIngredientStates.ToArray());
    }

    public void OnNodePointerExit(BaseItem exitNode)
    {
        SceneManager.Get.AllIngredientStates.Clear();

        for (int i = 0; i < SceneManager.Get.AllIngredientNames.Count; i++)
        {
            SceneManager.Get.AllIngredientStates.Add(0);
        }

        GPUBuffers.Get.IngredientStates.SetData(SceneManager.Get.AllIngredientStates.ToArray());
    }

    public void UpdateSelectedIngredientStates()
    {
        SceneManager.Get.AllIngredientStates.Clear();
        var selectedLeaveNames = GetAllLeaveNodesFrom(_selectedNode).Select(leave => leave.Path).ToList();

        for (int i = 0; i < SceneManager.Get.AllIngredientNames.Count; i++)
        {
            var highlighted = selectedLeaveNames.Contains(SceneManager.Get.AllIngredientNames[i]);
            SceneManager.Get.AllIngredientStates.Add(highlighted ? 1 : 0);
        }

        GPUBuffers.Get.IngredientStates.SetData(SceneManager.Get.AllIngredientStates.ToArray());
    }

    public void OnNodeFoldClick(BaseItem selectedNode)
    {
        UpdateAllToggles();
        //OnFocusToggleClick(selectedNode);
    }

    public void OnNodePointerClick(BaseItem selectedNode)
    {
        SetSelectedNode(selectedNode);
    }

    public void SetSelectedNode(BaseItem selectedNode)
    {
        if (_selectedNode != null)
        {
            _selectedNode.RangeFieldItem.UnhighlightInHierarchy();
        }

        if (selectedNode == null) return; 

        CutObjectManager.Get.GetSelectedCutObject().LastNodeFocusId = selectedNode.Id;
        _selectedNode = selectedNode;
        UpdateSelectedNodeFuzziValue();
        UpdateSelectedNodeApertureValue();
        UpdateSelectedNodeOcclusionValue();
        UpdateHideShowOcclusionUIFields();

        //UpdateSelectedIngredientStates();
    }

    public void OnSelectedCutObjectChange()
    {
        if (_rootNodes == null || CPUBuffers.Get.HistogramData == null) return;

        UpdateAllToggles();
        UpdateInvertValue();
        UpdateSelectedNodeFuzziValue();
        UpdateSelectedNodeApertureValue();
        UpdateSelectedNodeOcclusionValue();
        UpdateHideShowOcclusionUIFields();
        UpdateThreeStateToogleStates();
        UpdateVisibilityToogleStates();

        if (CutObjectManager.Get.GetSelectedCutObject().LastNodeFocusId != -1)
        {
            SetSelectedNode(_rootNodes[CutObjectManager.Get.GetSelectedCutObject().LastNodeFocusId]);
        }
    }

    //*** Invert ****//

    void UpdateInvertValue()
    {
        //cutObjectUiController.SetInvertToggleValue(CutObjectManager.Get.GetSelectedCutObject().Inverse);
    }

    //*** Aperture ****//

    private bool _ignoreApertureUIChangeFlag;

    void UpdateSelectedNodeApertureValue()
    {
        _ignoreApertureUIChangeFlag = true;
        var averageValues = GetAverageCutParamsFromLeafNodes(GetAllLeaveNodesFrom(_selectedNode));
        SetApertureUIValue(averageValues.Aperture);
    }

    public void SetApertureUIValue(float value)
    {
        //cutObjectUiController.SetApertureUIValue(value);
    }

    public void OnApertureValueChanged(float value)
    {
        if (!_ignoreApertureUIChangeFlag)
        {
            foreach (var leafNodes in GetAllLeaveNodesFrom(_selectedNode))
            {
                foreach (var cutObject in CutObjectManager.Get.GetSelectedCutObjects())
                {
                    cutObject.IngredientCutParameters[CPUBuffers.Get.NodeToIngredientLookup[leafNodes.Id]].Aperture = value;
                }
            }
        }
        else
        {
            _ignoreApertureUIChangeFlag = false;
        }

        UpdateVisibilityToogleStates();
    }

    //*** Fuzzi ****//

    private bool _ignoreFuzzinessUIChangeFlag;
    private bool _ignoreDistanceUIChangesFlag;
    private bool _ignoreCurveUIChangesFlag;

    public void UpdateSelectedNodeFuzziValue()
    {
        _ignoreFuzzinessUIChangeFlag = _ignoreDistanceUIChangesFlag = _ignoreCurveUIChangesFlag = true;
        var averageValues = GetAverageCutParamsFromLeafNodes(GetAllLeaveNodesFrom(_selectedNode));
        SetFuzzinessUIValues(averageValues.fuzziness, averageValues.fuzzinessDistance, averageValues.fuzzinessCurve);
    }

    public void OnFuzzinessChanged(float value)
    {
        if (!_ignoreFuzzinessUIChangeFlag)
        {
            foreach (var leafNodes in GetAllLeaveNodesFrom(_selectedNode))
            {
                foreach (var cutObject in CutObjectManager.Get.GetSelectedCutObjects())
                {
                    cutObject.IngredientCutParameters[CPUBuffers.Get.NodeToIngredientLookup[leafNodes.Id]].fuzziness = value;
                }
            }
        }
        else
        {
            _ignoreFuzzinessUIChangeFlag = false;
        }
    }

    public void OnFuzzinessDistanceChanged(float value)
    {
        if (!_ignoreDistanceUIChangesFlag)
        {
            foreach (var leafNodes in GetAllLeaveNodesFrom(_selectedNode))
            {
                foreach (var cutObject in CutObjectManager.Get.GetSelectedCutObjects())
                {
                    cutObject.IngredientCutParameters[CPUBuffers.Get.NodeToIngredientLookup[leafNodes.Id]].fuzzinessDistance = value;
                }
            }
        }
        else
        {
            _ignoreDistanceUIChangesFlag = false;
        }
    }

    public void OnFuzzinessCurveChanged(float value)
    {
        if (!_ignoreCurveUIChangesFlag)
        {
            foreach (var leafNodes in GetAllLeaveNodesFrom(_selectedNode))
            {
                foreach (var cutObject in CutObjectManager.Get.GetSelectedCutObjects())
                {
                    cutObject.IngredientCutParameters[CPUBuffers.Get.NodeToIngredientLookup[leafNodes.Id]].fuzzinessCurve = value;
                }
            }
        }
        else
        {
            _ignoreCurveUIChangesFlag = false;
        }
    }

    public void SetFuzzinessUIValues(float value1, float value2, float value3)
    {
        //cutObjectUiController.SetFuzzinessSliderValue(value1);
        //cutObjectUiController.SetDistanceSliderValue(value2);
        //cutObjectUiController.SetCurveSliderValue(value3);
    }

    public void HideFuzzinessUIPanel(bool value)
    {
        //cutObjectUiController.HideFuzzinessUIPanel(value);
    }

    //*** Occlusion ****//

    private bool _ignoreOcclusionUIChangeFlag;

    public void UpdateSelectedNodeOcclusionValue()
    {
        _ignoreOcclusionUIChangeFlag = true;
        var averageValues = GetAverageCutParamsFromLeafNodes(GetAllLeaveNodesFrom(_selectedNode));
        SetOcclusionUIValue(averageValues.value2);
    }

    public void OnOcclusionUIValueChanged(float value)
    {
        if (!_ignoreOcclusionUIChangeFlag)
        {
            foreach (var leafNodes in GetAllLeaveNodesFrom(_selectedNode))
            {
                foreach (var cutObject in CutObjectManager.Get.GetSelectedCutObjects())
                {
                    cutObject.IngredientCutParameters[CPUBuffers.Get.NodeToIngredientLookup[leafNodes.Id]].value2 = value;
                }
            }
        }
        else
        {
            _ignoreOcclusionUIChangeFlag = false;
        }

        UpdateVisibilityToogleStates();
    }

    public void SetOcclusionUIValue(float value)
    {
        //cutObjectUiController.SetOcclusionUIValue(value);
    }

    public void HideOcclusionUIPanel(bool value)
    {
        //cutObjectUiController.HideOcclusionUIFields(value);
    }

    //*** Toggles ****//

    void UpdateAllToggles()
    {
        foreach (var node in _rootNodes)
        {
            if (node.IsLeafNode())
            {
                var toggleState = CutObjectManager.Get.GetSelectedCutObject().IngredientCutParameters[CPUBuffers.Get.NodeToIngredientLookup[node.Id]].IsFocus;
                node.RangeFieldItem.Toggle.SetState(toggleState);
                node.RangeFieldItem.LockToggle.gameObject.SetActive(toggleState);

                if (toggleState && CutObjectManager.Get.GetSelectedCutObject().CurrentLockState == LockState.Unlocked)
                {
                    node.RangeFieldItem.LockToggle.SetState(false);
                }

                if (toggleState && CutObjectManager.Get.GetSelectedCutObject().CurrentLockState == LockState.Locked)
                {
                    node.RangeFieldItem.LockToggle.SetState(true);
                }
            }
        }

        foreach (var node in _rootNodes)
        {
            if (!node.IsLeafNode())
            {
                var toggleState = node.HasSomeChildrenFocus();
                node.RangeFieldItem.Toggle.SetState(toggleState);
                node.RangeFieldItem.LockToggle.gameObject.SetActive(toggleState);

                if (toggleState && CutObjectManager.Get.GetSelectedCutObject().CurrentLockState == LockState.Unlocked || CutObjectManager.Get.GetSelectedCutObject().CurrentLockState == LockState.Restore)
                {
                    node.RangeFieldItem.LockToggle.SetState(false);
                }

                if (toggleState && CutObjectManager.Get.GetSelectedCutObject().CurrentLockState == LockState.Locked || CutObjectManager.Get.GetSelectedCutObject().CurrentLockState == LockState.Consumed)
                {
                    node.RangeFieldItem.LockToggle.SetState(true);
                }
            }
        }
    }

    public void SetAllLockState(bool value)
    {
        foreach (var node in _rootNodes)
        {
            if (node.RangeFieldItem.Toggle.GetState())
                node.RangeFieldItem.LockToggle.SetState(value);
        }

        if (value && CutObjectManager.Get.GetSelectedCutObject().CurrentLockState == LockState.Unlocked)
        {
            CutObjectManager.Get.GetSelectedCutObject().CurrentLockState = LockState.Locked;
        }

        if (!value && CutObjectManager.Get.GetSelectedCutObject().CurrentLockState == LockState.Consumed)
        {
            CutObjectManager.Get.GetSelectedCutObject().CurrentLockState = LockState.Restore;
        }
    }

    public void UpdateVisibilityToogleStates()
    {
        // Update the +/-/0 toggle on each modifcation of value1
        foreach (var node in _rootNodes)
        {
            var leafNodes = new List<BaseItem>();
            if (node.IsLeafNode()) leafNodes.Add(node);
            else leafNodes.AddRange(node.GetAllLeafChildren());

            var averageCutParams = GetAverageCutParamsFromLeafNodes(leafNodes);
            node.RangeFieldItem.VisibilityToggle.SetActive(!averageCutParams.IsFocus && (averageCutParams.value2 < 1 || averageCutParams.Aperture > 0));
        }
    }

    public void UpdateThreeStateToogleStates()
    {
        // Update the +/-/0 toggle on each modifcation of value1
        foreach (var node in _rootNodes)
        {
            var leafNodes = new List<BaseItem>();
            if (node.IsLeafNode()) leafNodes.Add(node);
            else leafNodes.AddRange(node.GetAllLeafChildren());

            var averageCutParams = GetAverageCutParamsFromLeafNodes(leafNodes);
            node.RangeFieldItem.ThreeStateToggle.SetState(averageCutParams.value1);
        }
    }

    public void OnFocusToggleClick(BaseItem item)
    {
        var value = item.RangeFieldItem.Toggle.GetState();

        item.RangeFieldItem.Toggle.SetState(value);
        item.RangeFieldItem.LockToggle.gameObject.SetActive(value);

        if (item.IsLeafNode())
        {
            var cutObject = CutObjectManager.Get.GetSelectedCutObject();
            CutObjectManager.Get.GetSelectedCutObject().IngredientCutParameters[CPUBuffers.Get.NodeToIngredientLookup[item.Id]].IsFocus = value;
        }

        foreach (var child in item.GetAllChildren())
        {
            if (child.IsLeafNode())
            {
                var cutObject = CutObjectManager.Get.GetSelectedCutObject();
                CutObjectManager.Get.GetSelectedCutObject().IngredientCutParameters[CPUBuffers.Get.NodeToIngredientLookup[child.Id]].IsFocus = value;
            }

            child.RangeFieldItem.Toggle.SetState(value);
            child.RangeFieldItem.LockToggle.gameObject.SetActive(value);
        }

        SetAllLockState(false);
        UpdateHideShowOcclusionUIFields();
        UpdateVisibilityToogleStates();
    }

    

    public void OnThreeStateToggleClick(BaseItem baseItem)
    {
        var value = baseItem.RangeFieldItem.ThreeStateToggle.GetState();
        var value1 = (value == ThreeStateToggleState.Zero) ? 0.5f : (value == ThreeStateToggleState.Plus) ? 1.0f : 0.0f;

        if (baseItem.IsLeafNode())
        {
            var cutObject = CutObjectManager.Get.GetSelectedCutObject();
            CutObjectManager.Get.GetSelectedCutObject().IngredientCutParameters[CPUBuffers.Get.NodeToIngredientLookup[baseItem.Id]].value1 = value1;
        }

        foreach (var child in baseItem.GetAllChildren())
        {
            if (child.IsLeafNode())
            {
                var cutObject = CutObjectManager.Get.GetSelectedCutObject();
                CutObjectManager.Get.GetSelectedCutObject().IngredientCutParameters[CPUBuffers.Get.NodeToIngredientLookup[child.Id]].value1 = value1;
            }

            child.RangeFieldItem.ThreeStateToggle.SetState(value1);
        }
        
    }

    //*** Utils ****//

    private List<BaseItem> GetAllLeaveNodesFrom(BaseItem baseItem)
    {
        var selectedLeafNodes = new List<BaseItem>();
        if (baseItem.IsLeafNode()) selectedLeafNodes.Add(baseItem);
        else selectedLeafNodes.AddRange(baseItem.GetAllLeafChildren());
        return selectedLeafNodes;
    }

    public CutParameters GetAverageCutParamsFromLeafNodes(List<BaseItem> leafNodes)
    {
        var cutParams = new CutParameters();

        foreach (var leafNode in leafNodes)
        {
            var index = CPUBuffers.Get.NodeToIngredientLookup[leafNode.Id];
            if(index < 0) throw new Exception("Node to protein lookup error");

            foreach (var cutObject in CutObjectManager.Get.GetSelectedCutObjects())
            {
                var cutParam = cutObject.GetCutParametersFor(index);
                cutParams.IsFocus |= cutParam.IsFocus;
                cutParams.value1 += cutParam.value1;
                cutParams.value2 += cutParam.value2;
                cutParams.fuzziness += cutParam.fuzziness;
                cutParams.fuzzinessDistance += cutParam.fuzzinessDistance;
                cutParams.fuzzinessCurve += cutParam.fuzzinessCurve;
                cutParams.Aperture += cutParam.Aperture;
            }
        }

        var averageTotalCount = (float)(leafNodes.Count * CutObjectManager.Get.GetSelectedCutObjects().Count);
        cutParams.value1 /= averageTotalCount;
        cutParams.value2 /= averageTotalCount;
        cutParams.fuzziness /= averageTotalCount;
        cutParams.fuzzinessDistance /= averageTotalCount;
        cutParams.fuzzinessCurve /= averageTotalCount;
        cutParams.Aperture /= averageTotalCount;
        return cutParams;
    }

    public BaseItem FindBaseItem(string path)
    {
        return _rootNodes.FirstOrDefault(n => n.Path == path);
    }

    public void LogRangeValues()
    {
        foreach (var Node in _rootNodes)
        {
            Node.FieldObject.GetComponent<RangeFieldItem>().GetRangeValues();
            //Node.Name
        }
    }

    //*** Routines ****//

    public void OnRangeSliderDrag(BaseItem targetNode, int rangeIndex, float dragDelta)
    {
        var selectedLeafNodes = new List<BaseItem>();

        if (targetNode.IsLeafNode()) selectedLeafNodes.Add(targetNode);
        else selectedLeafNodes.AddRange(targetNode.GetAllLeafChildren());

        // Init current drag value
        if (targetNode.RangeFieldItem.CustomRangeSliderUi.StartedDragging)
        {
            var averageCutParams = GetAverageCutParamsFromLeafNodes(selectedLeafNodes);
            _currentDragValue = rangeIndex == 0 ? averageCutParams.value2 : averageCutParams.value1;
        }
        
        _currentDragValue += dragDelta / 200;
        _currentDragValue = Mathf.Min(1.0f, Mathf.Max(0.0f, _currentDragValue));

        // Set new cut params values
        foreach (var child in selectedLeafNodes)
        {
            var index = CPUBuffers.Get.NodeToIngredientLookup[child.Id];
            if (index < 0) throw new Exception("Node to protein lookup error");
            
            foreach (var cutObject in CutObjectManager.Get.GetSelectedCutObjects())
            {
                if (rangeIndex == 0)
                {
                    cutObject.SetValue2For(index, _currentDragValue);
                    //GlobalProperties.Get.AdjustVisible = _currentDragValue;
                }

                if (rangeIndex == 1)
                {
                    cutObject.SetValue1For(index, _currentDragValue);
                }
            }
        }

        UpdateThreeStateToogleStates();
        UpdateVisibilityToogleStates();
    }

    public void UpdateHideShowOcclusionUIFields()
    {
        var averageValues = GetAverageCutParamsFromLeafNodes(GetAllLeaveNodesFrom(_selectedNode));
        HideOcclusionUIPanel(averageValues.IsFocus);
    }

    public void UpdateHistograms()
    {
        if (_rootNodes == null || CPUBuffers.Get.HistogramData == null) return;

        foreach (var node in _rootNodes)
        {
            var histData = CPUBuffers.Get.HistogramData[node.Id];
            if (histData.all <= 0)
            {
                continue;
            }
            else
            {
                var newRangeValues = new List<float>();
                var oldRangeValues = node.RangeFieldItem.GetRangeValues();
                var newRange0 = (float)histData.visible / (float)histData.all;
                var newRange1 = (float)histData.cutaway / (float)histData.all;

                var delta = newRange0 - oldRangeValues[0];
                if (Mathf.Abs(delta) < 0.01f) delta = 0;
                var newRange0Smooth = oldRangeValues[0] + delta * 0.1f;

                newRangeValues.Add(newRange0Smooth);
                newRangeValues.Add(1.0f - newRange1 - newRangeValues[0]);
                newRangeValues.Add(1.0f - newRangeValues[0] - newRangeValues[1]);
                node.RangeFieldItem.SetRangeValues(newRangeValues);
            }
        }
    }
    
    //*** Nodes stuffs ****//

    // Add a new object to the tree
    public void AddNodeObject(string fullPath, object[] args, string type)
	{
        var name = MyUtility.GetNameFromUrlPath(fullPath);
	    var parentPath = MyUtility.GetParentUrlPath(fullPath);

        // If the node is a root node
        if (string.IsNullOrEmpty(parentPath))
	    {
            var node = CreateNodeObject(name, fullPath, args, type);
            _rootNodes.Add(node);
        }
        // If the node is a child node
	    else
        {
            var parentNode = FindBaseItem(parentPath);

            if (parentNode != null)
            {
                var node = CreateNodeObject(name, fullPath, args, type);
                _rootNodes.Add(node);
                parentNode.AddChild(node);

                if (name.Contains("RNA"))
                {
                    _rnaInstanceNodeId = node.Id;
                }
            }
            else
            {
                throw new Exception("System error");
            }
        }
    }
    
    public BaseItem CreateNodeObject(string name, string fullPath, object[] args, string type)
    {
        // Instantiate prefac
        var go = GameObject.Instantiate(BaseItemPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        go.transform.SetParent(this.transform, false);

        var node = go.GetComponent<BaseItem>();
        node.Initialize(name, fullPath, type, args, false, this, _rootNodes.Count);
        
        return node;
    }

    // Reorder the tree elements
    public void UpdateLayout()
    {
        // TODO: Use the real base node height here
        float currentYPos = -(maxDistanceY + 10);

        foreach (var node in _rootNodes)
        {
            var treeLevel = Mathf.Max(node.GetTreeLevel() - 1, 0);

            var rt = node.GetComponent<RectTransform>();
            rt.localPosition = new Vector3(treeLevel * Indent + LeftPadding, currentYPos);
            //rt.localPosition = new Vector3(Indent + LeftPadding, currentYPos);
            node.SaveInitPositionY();

            if (node.gameObject.activeInHierarchy)
            {
                currentYPos -= rt.rect.height + Spacing;
                //Debug.Log(rt.rect.height);
            }
        }

        GetComponent<RectTransform>().sizeDelta = new Vector2(300, Mathf.Abs(currentYPos - maxDistanceY));

        UpdateNodeItems();
    }

    private float maxDistanceX = 300;
    private float maxDistanceY = 25;
    private float acc;

    private Vector3 currentMousePos;

    private bool GetLockState()
    {
        bool lockState = false;
        foreach (var node in _rootNodes.Where(node => node.gameObject.activeInHierarchy))
        {
            var l = node.FieldObject.GetComponent<IItemInterface>().GetLockState();
            lockState |= l;
        }

        return lockState;
    }

    private bool GetSlowDownState()
    {
        bool slowDown = false;
        foreach (var node in _rootNodes.Where(node => node.gameObject.activeInHierarchy))
        {
            var l = node.FieldObject.GetComponent<IItemInterface>().GetSlowDownState();
            slowDown |= l;
        }

        return slowDown;
    }

    public void InitNodeItems()
    {
        initState = true;
        UpdateLayout();
    }

    private bool initState = false;
    private bool _treeIsActive = true;

    void UpdateNodeItems()
    {
        // Do list scrolling when hovering the items
        if (Input.mousePosition.x < maxDistanceX && Input.mousePosition.x != 0)
        {
            if (Input.mousePosition.y < Screen.height && Input.mousePosition.y > Screen.height - 50 && transform.localPosition.y > 0)
            {
                transform.localPosition -= new Vector3(0, acc, 0);
                acc *= 1.01f;
            }
            else if (Input.mousePosition.y > 0 && Input.mousePosition.y < 50 && transform.localPosition.y < GetComponent<RectTransform>().sizeDelta.y - Screen.height)
            {
                transform.localPosition += new Vector3(0, acc, 0);
                acc *= 1.01f;
            }
            else
            {
                acc = 8;
            }
        }

        if (!EnableLensEffect)
        {
            currentMousePos = Input.mousePosition;

            // Fetch the scroll offset from scroll view content (this)
            var scrollOffset = transform.localPosition.y;

            // Do the apple dock layout list style
            foreach (var node in _rootNodes.Where(node => node.gameObject.activeInHierarchy))
            {
                if (Input.mousePosition.x < 300)
                {
                    // TODO: Use the real base node height here
                    var distanceY = (node.InitGlobalPositionY + scrollOffset + maxDistanceY) - 5 - currentMousePos.y;
                    //var distanceY = node.transform.position.y - node.GetComponent<RectTransform>().sizeDelta.y * 0.5f - mousePos.y;
                    distanceY = Mathf.Clamp(distanceY, -maxDistanceY, maxDistanceY);

                    var x = (Math.Abs(distanceY) / maxDistanceY);
                    var alpha = Mathf.Max(1 - x, 0.2f);
                    node.FieldObject.GetComponent<IItemInterface>().SetContentAlpha(alpha);
                }
                else
                {
                    node.FieldObject.GetComponent<IItemInterface>().SetContentAlpha(0.2f);
                }

                node.transform.localScale = new Vector3(0.5f, 0.5f, 1);
                node.transform.localPosition = new Vector3(node.transform.localPosition.x,
                    node.InitLocalPositionY + maxDistanceY, node.transform.localPosition.z);
            }

            if (_selectedNode != null) _selectedNode.FieldObject.GetComponent<IItemInterface>().SetContentAlpha(1);
        }
        else
        {
            if (_treeIsActive && Input.GetMouseButtonDown(0) && Input.mousePosition.x < 200)
            {
                _treeIsActive = false;
                //MainCamera.main.GetComponent<NavigateCamera>().FreezeState = true;
                currentMousePos = Input.mousePosition;

                foreach (var node in _rootNodes.Where(node => node.gameObject.activeInHierarchy))
                {
                    node.FieldObject.GetComponent<RangeFieldItem>().CustomRangeSliderUi.gameObject.SetActive(true);
                }
            }

            if (initState || Input.GetMouseButtonDown(1))
            {
                _treeIsActive = true;
                //MainCamera.main.GetComponent<NavigateCamera>().FreezeState = false || true;
                //MainCamera.main.GetComponent<NavigateCamera>().FreezeState = false;

                // Do the apple dock layout list style
                foreach (var node in _rootNodes.Where(node => node.gameObject.activeInHierarchy))
                {
                    //node.FieldObject.GetComponent<RangeFieldItem>().RangeSliderUI.gameObject.SetActive(false);
                    node.FieldObject.GetComponent<IItemInterface>().SetContentAlpha(0.5f);
                    node.transform.localScale = new Vector3(0.5f, 0.5f, 1);
                    node.transform.localPosition = new Vector3(node.transform.localPosition.x,
                        node.InitLocalPositionY + maxDistanceY, node.transform.localPosition.z);

                }

                initState = false;
            }

            if (GetLockState() || _treeIsActive) return;

            if (GetSlowDownState())
            {
                currentMousePos += (Input.mousePosition - currentMousePos) * 0.005f;
            }
            else
            {
                currentMousePos += (Input.mousePosition - currentMousePos) * 0.1f;
            }


            // Fetch the scroll offset from scroll view content (this)
            var scrollOffset = transform.localPosition.y;

            // Do the apple dock layout list style
            foreach (var node in _rootNodes.Where(node => node.gameObject.activeInHierarchy))
            {
                //if (mousePos.x > maxDistanceX || mousePos.x <= 0)
                //{
                //    node.transform.localScale = new Vector3(0.5f, 0.5f, 1);
                //    node.transform.localPosition = new Vector3(node.transform.localPosition.x, node.InitLocalPositionY + maxDistanceY, node.transform.localPosition.z);
                //    continue;
                //}

                // TODO: Use the real base node height here
                var distanceY = (node.InitGlobalPositionY + scrollOffset) - 15 - currentMousePos.y;
                //var distanceY = node.transform.position.y - node.GetComponent<RectTransform>().sizeDelta.y * 0.5f - mousePos.y;
                distanceY = Mathf.Clamp(distanceY, -maxDistanceY, maxDistanceY);

                var x = (Math.Abs(distanceY) / maxDistanceY);
                var scale = 1 - (Math.Abs(distanceY) / maxDistanceY);
                scale = 0.5f + (0.25f * scale);

                var alpha = Mathf.Max(1 - x, 0.2f);
                node.FieldObject.GetComponent<IItemInterface>().SetContentAlpha(alpha);

                node.transform.localScale = new Vector3(scale, scale, 1);
                node.transform.localPosition = new Vector3(node.transform.localPosition.x, node.InitLocalPositionY + distanceY, node.transform.localPosition.z);
            }
        }
    }

    
}
