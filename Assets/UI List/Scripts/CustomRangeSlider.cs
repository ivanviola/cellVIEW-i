using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Deployment.Internal;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomRangeSlider : MonoBehaviour, IPointerClickHandler
{
    public Texture2D cursor;
    public int totalLength = 200;

    public List<RectTransform> ranges;
    public List<RectTransform> handles;

    public int HandleWidth = 5;

    public bool LockState = false;
    public bool SlowDownState = false;
    public bool DragState = false;

    public bool StoppedDragging = false;
    public bool StartedDragging = false;
    public bool recalcOnce = false;
    public bool disableDragging = false;

    public bool useFakeRangeValues = false;

    [NonSerialized] public List<float> rangeValues = new List<float> {0.33333f, 0.33333f, 0.33333f };
    [NonSerialized] public List<float> fakeRangeValues = new List<float> { 0.33333f, 0.33333f, 0.33333f };

    public delegate void OnRangeSliderDrag(BaseItem node, int rangeIndex, float dragDelta);
    public event OnRangeSliderDrag RangeSliderDrag;

    

    // Use this for initialization
    private void Start()
    {
        /*for (int i = 0; i < rangeValues.Count; i++)
        {
            SetRangeGradientColors(i, Color.red, Color.blue);
        }*/

        /*SetRangeGradientColors(0, new Color(0.0f, 0.0f, 1.0f, 1.0f), new Color(1.0f, 0.0f, 0.0f, 1.0f));
        SetRangeGradientColors(1, new Color(1.0f, 0.0f, 0.0f, 1.0f), new Color(0.0f, 0.0f, 1.0f, 1.0f));
        SetRangeGradientColors(2, new Color(0.0f, 0.0f, 1.0f, 1.0f), new Color(0.0f, 1.0f, 0.0f, 1.0f));*/

        SetRangeGradientColors(0, new Color(0.0f, 0.1f, 0.0f, 1.0f), new Color(0.0f, 0.3f, 0.0f, 1.0f));
        SetRangeGradientColors(1, new Color(0.0f, 0.6f, 0.0f, 1.0f), new Color(0.0f, 0.9f, 0.0f, 1.0f));
        SetRangeGradientColors(2, new Color(0.0f, 0.0f, 0.0f, 0.15f), new Color(0.0f, 0.0f, 0.0f, 0.25f));

        GetComponent<LayoutElement>().minWidth = totalLength + HandleWidth * 2;
    }

    public BaseItem GetBaseItemParent()
    {
        return transform.parent.parent.GetComponent<BaseItem>();
    }

    private List<float> requestRangeValues()
    {
        if (useFakeRangeValues)
        {
            return fakeRangeValues;
        }
        else
        {
            return rangeValues;
        }
    }

    private int GetRangeWidth(float value)
    {
        return Mathf.RoundToInt(totalLength * value);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        var parentRectTransform = gameObject.transform.parent.GetComponent<RectTransform>();
        GetComponent<LayoutElement>().preferredWidth = totalLength + HandleWidth * 2;

        float widthInc = 0;
        for (int i = 0; i < rangeValues.Count; i++)
        {
            var rangeWidth = GetRangeWidth(rangeValues[i]);

            var rangeRectTransform = ranges[i];
            rangeRectTransform.sizeDelta = new Vector2(rangeWidth, parentRectTransform.sizeDelta.y);
            rangeRectTransform.localPosition = new Vector3(widthInc, 0, 0);
            widthInc += rangeWidth;

            if (i < handles.Count)
            {
                var handleRectTransform = handles[i];
                handleRectTransform.sizeDelta = new Vector2(HandleWidth, parentRectTransform.sizeDelta.y);
                handleRectTransform.localPosition = new Vector3(widthInc, 0, 0);
                widthInc += HandleWidth;
            }

            var textUI = ranges[i].GetChild(0).GetComponent<Text>();
            var newText = (textUI.gameObject.GetComponent<RectTransform>().rect.width > 5) ? Mathf.Round(requestRangeValues()[i] * 100.0f) + " %" : "";
            if (textUI.text != newText)
            {
                textUI.text = newText;
            }
        }

        if (GetComponent<CanvasGroup>().alpha < 0.9f)
        {
            SlowDownState = false;
        }

        if (GetComponent<CanvasGroup>().alpha < 0.8f)
        {
            LockState = false;
        }
        //if(LockState) Debug.Log("LockState");
    }

    //public void SetRangeColor(int rangeIndex, Color color)
    //{
    //    ranges[rangeIndex].GetComponent<Image>().material.SetColor("_ColorLeft", color);
    //    ranges[rangeIndex].GetComponent<Image>().material.SetColor("_ColorRight", color);
    //}

    public void SetRangeGradientColors(int rangeIndex, Color left, Color right)
    {
        ranges[rangeIndex].GetComponent<UnityEngine.UI.Extensions.Gradient>().vertex1 = left;
        ranges[rangeIndex].GetComponent<UnityEngine.UI.Extensions.Gradient>().vertex2 = right;
    }

    public void OnDrag(BaseEventData eventData)
    {
        LockState = true;
        DragState = true;
        StoppedDragging = false;

        Cursor.SetCursor(cursor, new Vector2(14, 14), CursorMode.Auto);

        /*if (disableDragging)
            return;*/

        var pointerEvent = (PointerEventData) eventData;
        var gameObject = pointerEvent.pointerDrag;
        var handleIndex = handles.IndexOf(gameObject.GetComponent<RectTransform>());

        //var previousRangeValue = rangeValues[handleIndex];
        //var nextRangeValue = rangeValues[handleIndex + 1];
        //var total = previousRangeValue + nextRangeValue;

        //var ratio = 100.0f * 2;
        //previousRangeValue += pointerEvent.delta.x / ratio;
        //previousRangeValue = Mathf.Clamp(previousRangeValue, 0.0f, total);
        //nextRangeValue = total - previousRangeValue;

        //rangeValues[handleIndex] = previousRangeValue;
        //rangeValues[handleIndex + 1] = nextRangeValue;

        RangeSliderDrag(GetBaseItemParent(), handleIndex, pointerEvent.delta.x);
    }

    public void OnEnter()
    {
        
        LockState = true;
        Cursor.SetCursor(cursor, new Vector2(14, 14), CursorMode.Auto);
    }

    public void OnExit()
    {
        if (!DragState)
        {
            LockState = false;
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }

    public void OnPointerUp()
    {
        StoppedDragging = true;
        useFakeRangeValues = false;
        StartedDragging = false;
        DragState = false;
        LockState = false;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public void OnPointerDown(BaseEventData eventData)
    {
        StoppedDragging = false;
        StartedDragging = true;
        DragState = true;
        LockState = true;
        Cursor.SetCursor(cursor, new Vector2(14, 14), CursorMode.Auto);
        ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.pointerClickHandler);
    }

    public void OnDragExit()
    {
        useFakeRangeValues = false;
        StartedDragging = false;
        StoppedDragging = true;
        DragState = false;
        LockState = false;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public void OnPointerEnter()
    {
        //Debug.Log("Hello");
        SlowDownState = true;
    }

    public void OnPointerExit()
    {
        //Debug.Log("Leave");
        SlowDownState = false;
    }

    public void OnPointerClick(BaseEventData eventData)
    {
        // Handle event here AND in ancestors
        ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.pointerClickHandler);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //int a = 0;
    }
}
