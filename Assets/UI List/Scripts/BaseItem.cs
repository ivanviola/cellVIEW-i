using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.EventSystems;

public class BaseItem : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler/*, IPointerDownHandler, IPointerUpHandler*/
{
    public bool ExpertMode = false;
    public delegate void NodeItemClick(BaseItem node);
    public event NodeItemClick PointerClick;

    public void OnPointerClick(PointerEventData eventData)
    {
        PointerClick(this);
        //cameraController.ShouldBlockInteraction = true;
    }

    //public void OnPointerDown(PointerEventData eventData)
    //{
    //    cameraController.ShouldBlockInteraction = true;
    //}

    //public void OnPointerUp(PointerEventData eventData)
    //{
    //    cameraController.ShouldBlockInteraction = false;
    //}

    public delegate void NodeItemEnter(BaseItem node);
    public event NodeItemEnter PointerEnter;

    public void OnPointerEnter(PointerEventData eventData)
    {
        PointerEnter(this);
    }

    public delegate void NodeItemExit(BaseItem node);
    public event NodeItemExit PointerExit;

    public void OnPointerExit(PointerEventData eventData)
    {
        PointerExit(this);
        //cameraController.ShouldBlockInteraction = false;
    }

    public bool IsFolded = true;
    public bool IsVisible = true;
    
    public string Name;
    public int Id;
    public string Path;
    public string Type;

    [SerializeField]
    public object[] Args;

    public RangeFieldItem RangeFieldItem;

    // Children ui elements
    public GameObject ArrowObject;
    public GameObject FieldObject;
    
    public TreeViewController ViewController;
    public List<BaseItem> Children = new List<BaseItem>();

    private MainCameraController cameraController = null;


    void Start()
    {
        RangeFieldItem = GetComponentInChildren<RangeFieldItem>();

        if (cameraController == null) cameraController = FindObjectOfType<MainCameraController>();
    }

    public void OnEnable()
    {
        SetArrowState(IsFolded);

        if (cameraController == null) cameraController = FindObjectOfType<MainCameraController>();
    }


    public void SwitchToMode(bool toExpertMode)
    {
        ExpertMode = toExpertMode;

        RangeFieldItem.SwitchToMode(toExpertMode);
    }

    private void DropDownToggleDelegate(bool value)
    {
        IsFolded = value;
        SetChildrenVisibility(!value);
        ViewController.OnNodeFoldClick(this);
        ViewController.UpdateLayout();

    }

    public void SetFoldedState(bool value)
    {
        IsFolded = value;
        SetChildrenVisibility(!value);
        ViewController.UpdateLayout();
        SetArrowState(IsFolded);
    }

   
    public void Initialize(string name, string path, string type, object[] args, bool isFolded, TreeViewController controller, int id)
	{
        Name = name;
        Path = path;
        Type = type;
        Args = args;
        IsFolded = isFolded;
        ViewController = controller;
        Id = id;

        var rt = GetComponent<RectTransform>();
        //rt.sizeDelta = new Vector2(rt.sizeDelta.x, 15);

        SetArrowSize(ViewController.ArrowSize);
        SetArrowAlpha(0.5f);
        SetArrowState(IsFolded);
        SetArrowVisibility(false);
        SetChildrenVisibility(!IsFolded);

        ArrowObject.GetComponent<ArrowScript>().DropDownToggle += DropDownToggleDelegate;
        ArrowObject.GetComponent<ArrowScript>().SetState(isFolded);
        ArrowObject.GetComponent<ArrowScript>().SetState(IsVisible);
        ArrowObject.GetComponent<ArrowScript>().SetAlpha(0.5f);
        

        //FieldObject = TreeUtility.InstantiateNodeField(Type);
        FieldObject = Instantiate(Resources.Load("RangeItem", typeof(GameObject))) as GameObject;
        FieldObject.GetComponent<IItemInterface>().Parameters = Args;
        FieldObject.GetComponent<IItemInterface>().SetTextFontSize(ViewController.TextFontSize);
        FieldObject.GetComponent<IItemInterface>().SetContentAlpha(0.1f);
        FieldObject.transform.SetParent(this.transform, false);
        SetFieldObjectSize(ViewController.TextFieldSize);
        RangeFieldItem = GetComponentInChildren<RangeFieldItem>();

        SetFoldedState(isFolded);
	}

    public void AddChild(BaseItem item)
    {
        if (this.Name.Contains("capsid"))
        {
            int a = 0;
        }

        Children.Add(item);
        SetArrowVisibility(true);
    }

    public float InitGlobalPositionY;
    public float InitLocalPositionY;

    public void SaveInitPositionY()
    {
        InitGlobalPositionY = transform.position.y;
        InitLocalPositionY = transform.localPosition.y;
    }

    public float GetPositionY()
    {
        return GetComponent<RectTransform>().position.y;
    }

    public int GetTreeLevel()
    {
        return Path.Split(new char[] {'.'}, StringSplitOptions.RemoveEmptyEntries).ToList().Count;
    }

    private void SetChildrenVisibility(bool value)
    {
        foreach (var child in Children)
        {
            child.gameObject.SetActive(value);

            if (!child.IsFolded)
            {
                child.SetChildrenVisibility(value);
            }
        }
    }

    /*****/
    
    private void SetArrowAlpha(float value)
    {
        var arrowScript = ArrowObject.GetComponent<ArrowScript>();
        arrowScript.SetAlpha(value);
    }

    public void SetArrowVisibility(bool value)
    {
        var arrowScript = ArrowObject.GetComponent<ArrowScript>();
        arrowScript.SetEnabled(value);
    }

    private void SetArrowState(bool value)
    {
        var arrowScript = ArrowObject.GetComponent<ArrowScript>();
        arrowScript.SetState(value);
    }

    private void SetArrowSize(float value)
    {
        var rt = GetComponent<RectTransform>();
        var offsetY = -rt.rect.height / 2;

        var arrowScript = ArrowObject.GetComponent<ArrowScript>();
        var arrowRt = ArrowObject.GetComponent<RectTransform>();
        arrowRt.pivot = new Vector2(.5f, .5f);
        arrowRt.localPosition = new Vector3(value / 2, offsetY, 0);
        arrowRt.sizeDelta = new Vector2(value, value);
        arrowScript.Size = value;
    }

    private void SetFieldObjectSize(float value)
    {
        var rt = GetComponent<RectTransform>();
        var offsetY = -rt.rect.height / 2;

        var arrowSize = ArrowObject.GetComponent<ArrowScript>().Size;
        var fieldRt = FieldObject.GetComponent<RectTransform>();
        offsetY += value / 2;

        fieldRt.pivot = new Vector2(0, 1);
        int arrowToItemPadding = 10;
        fieldRt.localPosition = new Vector3(arrowSize + arrowToItemPadding, offsetY, 0);
        fieldRt.sizeDelta = new Vector2(fieldRt.sizeDelta.x, value);
    }

    public List<BaseItem> GetAllChildren()
    {
        List<BaseItem> allChildren = new List<BaseItem>();

        foreach (var child in Children)
        {
            allChildren.Add(child);
            allChildren.AddRange(child.GetAllChildren());
        }

        return allChildren;
    }

    public List<BaseItem> GetAllLeafChildren()
    {
        return GetAllChildren().Where(child => child.Children.Count == 0).ToList();
    }

    public bool IsLeafNode()
    {
        return Children.Count == 0;
    }

    

    public bool HasAllChildrenFocus()
    {
        foreach (var child in GetAllLeafChildren())
        {
            if (!child.RangeFieldItem.Toggle.GetState()) return false;
        }

        return true;
    }

    public bool HasSomeChildrenFocus()
    {
        foreach (var child in GetAllLeafChildren())
        {
            if (child.RangeFieldItem.Toggle.GetState()) return true;
        }

        return false;
    }
   
}
