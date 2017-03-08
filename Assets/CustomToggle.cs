using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomToggle : MonoBehaviour, IPointerClickHandler
{
    private bool isOn = false;

    public Image Background;
    public Image Checkmark;

    public delegate void CustomToggleClick(bool open);
    public event CustomToggleClick CustomToggleClickEvent;

    // Use this for initialization
    void Start()
    {
        //UpdateSprite();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public bool GetState()
    {
        return isOn;
    }

    public void SetState(bool value)
    {
        isOn = value;
        UpdateSprite();
    }

    void ToggleState()
    {
        isOn = !isOn;
        UpdateSprite();
    }

    void UpdateSprite()
    {
        if (isOn)
        {
            Checkmark.enabled = true;
        }
        else
        {
            Checkmark.enabled = false;
        }
    }

    public void OnPointerClick(PointerEventData ped)
    {
        ToggleState();
        CustomToggleClickEvent(isOn);
    }
}
