using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LockToggle : MonoBehaviour, IPointerClickHandler
{
    private bool Locked = false;

    public Image LockedImage;
    public Image OpenImage;

    public delegate void LockToggleClick(bool open);
    public event LockToggleClick LockToggleEvent;

    // Use this for initialization
    void Start ()
	{
	    //UpdateSprite();
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    public bool GetState()
    {
        return Locked;
    }

    public void SetState(bool value)
    {
        Locked = value;
        UpdateSprite();
    }

    void ToggleState()
    {
        Locked = !Locked;
        UpdateSprite();
    }

    void UpdateSprite()
    {
        if (Locked)
        {
            LockedImage.enabled = true;
            OpenImage.enabled = false;
        }
        else
        {
            LockedImage.enabled = false;
            OpenImage.enabled = true;
        }
    }

    public void OnPointerClick(PointerEventData ped)
    {
        ToggleState();
        LockToggleEvent(Locked);
    }
}
