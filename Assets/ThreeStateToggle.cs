using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum ThreeStateToggleState
{
    Plus = 0,
    Minus = 1,
    Zero = 2,
}

public class ThreeStateToggle : MonoBehaviour, IPointerClickHandler
{
    private ThreeStateToggleState _state = ThreeStateToggleState.Zero;

    //public Image Plus;
   // public Image Minus;
   // public Image Zero;

    public Text VisibilityText;

    public delegate void ThreeStateToggleClick(ThreeStateToggleState state);
    public event ThreeStateToggleClick ThreeStateToggleClickEvent;

    private static ThreeStateToggleState _defaultNextState = ThreeStateToggleState.Plus;
    private ThreeStateToggleState _nextState = _defaultNextState;

    // Use this for initialization
    void Start()
    {
        UpdateSprite();
    }

    public ThreeStateToggleState GetState()
    {
        return _state;
    }

    public void SetState(ThreeStateToggleState value)
    {
        _state = value;
        UpdateSprite();
    }

    public void SetState(float value)
    {
        if (value == 0.5f)
        {
            SetState(ThreeStateToggleState.Zero);
            _nextState = _defaultNextState;
        }
        else if(value < 0.5f)
        {
            SetState(ThreeStateToggleState.Minus);
            _nextState = ThreeStateToggleState.Zero;
        }
        else
        {
            SetState(ThreeStateToggleState.Plus);
            _nextState = ThreeStateToggleState.Zero;
        }
    }

    void ToggleState()
    {
        if (_state == ThreeStateToggleState.Minus)
        {
            _state = ThreeStateToggleState.Zero;
            _nextState = ThreeStateToggleState.Plus;
        }
        else if(_state == ThreeStateToggleState.Plus)
        {
            _state = ThreeStateToggleState.Zero;
            _nextState = ThreeStateToggleState.Minus;
        }
        else
        {
            _state = _nextState;
        }


        UpdateSprite();
    }

    void UpdateSprite()
    {
        if (_state == ThreeStateToggleState.Minus)
        {
            //Minus.enabled = true;
            //Plus.enabled = false;
            //Zero.enabled = false;
            VisibilityText.text = "HIDE ALL";
        }
        else if (_state == ThreeStateToggleState.Plus)
        {
            //Minus.enabled = false;
            //Plus.enabled = true;
            //Zero.enabled = false;
            VisibilityText.text = "SHOW ALL";
        }
        else
        {
            //Minus.enabled = false;
            //Plus.enabled = false;
            //Zero.enabled = true;
            VisibilityText.text = "SHOW CUT";
        }
    }

    public void OnPointerClick(PointerEventData ped)
    {
        ToggleState();
        ThreeStateToggleClickEvent(_state);
    }
}
