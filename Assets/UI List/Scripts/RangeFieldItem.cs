using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class RangeFieldItem : MonoBehaviour, IItemInterface, IPointerClickHandler, IPointerEnterHandler
{
    public Text TextUI;
    public CustomToggle Toggle;
    public GameObject VisibilityToggle;
    public LockToggle LockToggle;
    public ThreeStateToggle ThreeStateToggle;
    public CustomRangeSlider CustomRangeSliderUi;

    /// <summary>
    /// Parameters = new object[]{ string DisplayText }   OR
    /// Parameters = new object[]{ string DisplayText, Color FontColor }  OR
    /// Parameters = new object[]{ string DisplayText, Color FontColor, int FontSize }  OR
    /// Parameters = new object[]{ string DisplayText, Color FontColor, int FontSize, FontStyle fontstyle }  OR
    /// Parameters = new object[]{ string DisplayText, Color FontColor, int FontSize, FontStyle fontstyle, Font font } 
    /// </summary>
    /// <value>The parameters.</value>

    private BaseItem baseItem;
    private bool expertMode = true;
    
    public void Start()
    {
        baseItem = transform.parent.GetComponent<BaseItem>();
        //Toggle.isOn = false;
        //LockToggle.gameObject.SetActive(false);

        LockToggle.LockToggleEvent += OnLockToggleClick;
        Toggle.CustomToggleClickEvent += OnCustomToggleClick;
        ThreeStateToggle.ThreeStateToggleClickEvent += OnThreeStateToggleClick;
    }

    public void SwitchToMode(bool toExpertMode)
    {
        expertMode = toExpertMode;

        CustomRangeSliderUi.gameObject.SetActive(expertMode);
        Toggle.gameObject.SetActive(expertMode);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //Debug.Log("ENTERING: " + baseItem.Path + "; is leaf? " + baseItem.IsLeafNode());
        //HighlightInstances(baseItem.Path, baseItem.Id);
    }

    /// <summary>
    /// This gets called when user clicks on some item in the hierarchy
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    { // when user clicks on item in Hierarchy:
        // fetch the name or id of the selected item
        // set this in NewSelectionManager as the selected protein type
        // adjust GPU buffers accordingly so that these instances are rendered highlighted

        InfoTextController.Get.ShowInfoFor(baseItem.Path);

        HighlightInstances(baseItem.Path, baseItem.Id);

        HighlightInHierarchy();

        baseItem.ViewController.SetSelectedNode(baseItem);
    }

    public void HighlightInHierarchy()
    {
        var bgImg = gameObject.GetComponent<Image>();
        bgImg.color = new Color(1, 1, 1, 0.815f);
        TextUI.color = new Color(0,0,0);

        //baseItem.ViewController.SetSelectedNode(baseItem);
    }

    public void UnhighlightInHierarchy()
    {
        var bgImg = gameObject.GetComponent<Image>();
        bgImg.color = new Color(1, 1, 1, 0.2f);
        TextUI.color = new Color(1, 1, 1);
    }

    public void HighlightInstances(string path, int index)
    {
        NewSelectionManager.Instance.HierarchySelectedItemPath = path;
        //NewSelectionManager.Instance.SelectedProteinInstanceId = -1;
        //NewSelectionManager.Instance.SelectedLipidInstanceId = -1;

        if (index == baseItem.ViewController._rnaInstanceNodeId) {
            NewSelectionManager.Instance.RnaSelected = true;
        } else {
            NewSelectionManager.Instance.RnaSelected = false;
        }

        if (path == "root")
        {
            NewSelectionManager.Instance.RnaSelected = true;
        }
        
        int clickedItemType = CPUBuffers.Get.NodeToIngredientLookup[index];

        // here I save indices of intances that I highlight so that I can unhighlight them more efficiently
        NewSelectionManager.Instance.SelectedGroupInstanceIndices = new List<int>();
        NewSelectionManager.Instance.SelectedGroupLipidInstanceIndices = new List<int>();

        if (baseItem.IsLeafNode()) // Selected item is Protein or Lipid Type
        {
            for (var i = 0; i < CPUBuffers.Get.ProteinInstanceInfos.Count; i++)
            {
                var info = CPUBuffers.Get.ProteinInstanceInfos[i];
                var type = (int)info.x;
                if (type == clickedItemType)
                {
                    CPUBuffers.Get.ProteinInstanceInfos[i] = new Vector4(type, (int)InstanceState.Highlighted, info.z, info.w);
                    NewSelectionManager.Instance.SelectedGroupInstanceIndices.Add(i);
                }
                else
                {
                    CPUBuffers.Get.ProteinInstanceInfos[i] = new Vector4(type, (int)InstanceState.Normal, info.z, info.w);
                }
            }
            GPUBuffers.Get.ProteinInstancesInfo.SetData(CPUBuffers.Get.ProteinInstanceInfos.ToArray());

            for (var i = 0; i < CPUBuffers.Get.LipidInstanceInfos.Count; i++)
            {
                var info = CPUBuffers.Get.LipidInstanceInfos[i];
                var type = (int)info.x;
                if (type == clickedItemType)
                {
                    CPUBuffers.Get.LipidInstanceInfos2[i] = new Vector4((int)InstanceState.Highlighted, info.y, info.z, info.w);
                    NewSelectionManager.Instance.SelectedGroupLipidInstanceIndices.Add(i);
                }
                else
                {
                    CPUBuffers.Get.LipidInstanceInfos2[i] = new Vector4((int)InstanceState.Normal, info.y, info.z, info.w);
                }
            }
            GPUBuffers.Get.LipidInstancesInfo2.SetData(CPUBuffers.Get.LipidInstanceInfos2.ToArray());
        }
        else                       // Selected item is Compartment (multiple Protein Types)
        {
            var typesList = new List<int>();
            foreach (var child in baseItem.GetAllChildren()) // GetAllChildren returns ALL children under the node, even going to lower "levels" (at least I think)
            {
                if (child.IsLeafNode())
                {
                    var id = CPUBuffers.Get.NodeToIngredientLookup[child.Id];
                    typesList.Add(id);
                }
            }

            for (var i = 0; i < CPUBuffers.Get.ProteinInstanceInfos.Count; i++)
            {
                var info = CPUBuffers.Get.ProteinInstanceInfos[i];
                var type = (int)info.x;
                if (typesList.Contains(type)) // proteins
                {
                    CPUBuffers.Get.ProteinInstanceInfos[i] = new Vector4(type, (int)InstanceState.Highlighted, info.z, info.w);
                    NewSelectionManager.Instance.SelectedGroupInstanceIndices.Add(i);
                }
                else
                {
                    CPUBuffers.Get.ProteinInstanceInfos[i] = new Vector4(type, (int)InstanceState.Normal, info.z, info.w);
                }
            }
            GPUBuffers.Get.ProteinInstancesInfo.SetData(CPUBuffers.Get.ProteinInstanceInfos.ToArray());

            for (var i = 0; i < CPUBuffers.Get.LipidInstanceInfos.Count; i++)
            {
                var info = CPUBuffers.Get.LipidInstanceInfos[i];
                var type = (int)info.x;
                if (typesList.Contains(type))
                {
                    CPUBuffers.Get.LipidInstanceInfos2[i] = new Vector4((int)InstanceState.Highlighted, info.y, info.z, info.w);
                    NewSelectionManager.Instance.SelectedGroupLipidInstanceIndices.Add(i);
                }
                else
                {
                    CPUBuffers.Get.LipidInstanceInfos2[i] = new Vector4((int)InstanceState.Normal, info.y, info.z, info.w);
                }
            }
            GPUBuffers.Get.LipidInstancesInfo2.SetData(CPUBuffers.Get.LipidInstanceInfos2.ToArray());
        }
    }

    public List<float> GetRangeValues()
    {
        return CustomRangeSliderUi.rangeValues;
    }

    public void SetRangeValues(List<float> rangeValues)
    {
        /*CustomRangeSliderUi.rangeValues.Clear();
        CustomRangeSliderUi.rangeValues.AddRange(rangeValues);*/
        for (int i = 0; i < rangeValues.Count; i++)
        {
            if (CustomRangeSliderUi.rangeValues.Count > i)
                CustomRangeSliderUi.rangeValues[i] = rangeValues[i];
        }

        if (CustomRangeSliderUi.rangeValues.Count >= 3)
            CustomRangeSliderUi.rangeValues[2] = 1.0f - CustomRangeSliderUi.rangeValues[0] - CustomRangeSliderUi.rangeValues[1];
    }

    //public void SetFakeRangeValues(List<float> fakeRangeValues)
    //{
    //    /*CustomRangeSliderUi.rangeValues.Clear();
    //    CustomRangeSliderUi.rangeValues.AddRange(rangeValues);*/
    //    for (int i = 0; i < fakeRangeValues.Count; i++)
    //    {
    //        if (CustomRangeSliderUi.fakeRangeValues.Count > i)
    //            CustomRangeSliderUi.fakeRangeValues[i] = fakeRangeValues[i];
    //    }

    //    if (CustomRangeSliderUi.fakeRangeValues.Count >= 3)
    //        CustomRangeSliderUi.fakeRangeValues[2] = 1.0f - CustomRangeSliderUi.fakeRangeValues[0] - CustomRangeSliderUi.fakeRangeValues[1];

    //    CustomRangeSliderUi.useFakeRangeValues = true;
    //}

    //******* Event Callbacks *********//

    public void OnLockToggleClick(bool value)
    {
        baseItem.ViewController.SetAllLockState(value);
    }

    public void OnCustomToggleClick(bool value)
    {
        baseItem.ViewController.OnFocusToggleClick(baseItem);
    }

    public void OnThreeStateToggleClick(ThreeStateToggleState value)
    {
        baseItem.ViewController.OnThreeStateToggleClick(baseItem);
    }

    public object[] Parameters
    {
        get
        {
            return GetVals();
        }
        set
        {
            SetVals(value);
        }
    }
    
    public void SetTextFontSize(int fontSize)
    {
        TextUI.fontSize = fontSize;
    }

    public void SetContentAlpha(float alpha)
    {
        CustomRangeSliderUi.GetComponent<CanvasGroup>().alpha = alpha;
    }

    public bool GetLockState()
    {
        //if(RangeSliderUI.LockState) Debug.Log("Lock state");
        return CustomRangeSliderUi.LockState;
    }

    public bool GetSlowDownState()
    {
        //if (RangeSliderUI.LockState) Debug.Log("Lock state");
        return CustomRangeSliderUi.SlowDownState;
    }

    private object[] GetVals()
    {
        return new object[] { TextUI.text, TextUI.color, TextUI.fontSize, TextUI.fontStyle, TextUI.font };
    }

    private void SetVals(object[] Vals)
    {
        if (Vals.Length <= 5)
        {
            bool good = true;
            for (int i = 0; i < Vals.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        if (!(Vals[i] is string))
                        {
                            good = false;
                        }
                        break;
                    case 1:
                        if (!((Vals[i] is Color) || (Vals[i] == null)))
                        {
                            good = false;
                        }
                        break;
                    case 2:
                        if (!((Vals[i] is int) || (Vals[i] == null)))
                        {
                            good = false;
                        }
                        break;
                    case 3:
                        if (!((Vals[i] is FontStyle) || (Vals[i] == null)))
                        {
                            good = false;
                        }
                        break;
                    case 4:
                        if (!((Vals[i] is Font) || (Vals[i] == null)))
                        {
                            good = false;
                        }
                        break;
                    default:
                        break;
                }
            }
            if (good)
            {
                for (int i = 0; i < Vals.Length; i++)
                {
                    switch (i)
                    {
                        case 0:
                            TextUI.text = (string)Vals[i];
                            break;
                        case 1:
                            if (Vals[i] != null)
                            {
                                TextUI.color = (Color)Vals[i];
                            }
                            break;
                        case 2:
                            if (Vals[i] != null)
                            {
                                TextUI.fontSize = (int)Vals[i];
                            }
                            break;
                        case 3:
                            if (Vals[i] != null)
                            {
                                TextUI.fontStyle = (FontStyle)Vals[i];
                            }
                            break;
                        case 4:
                            if (Vals[i] != null)
                            {
                                TextUI.font = (Font)Vals[i];
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
