using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ToggleFieldItem : MonoBehaviour, IItemInterface {

	public Transform ToggleObj;
	public Transform TextObj;

	private Toggle _ts;
	private Text _tx;
	private RectTransform _tor;
	private RectTransform _txr;

	public object[] Parameters 
	{
		get 
		{
			return GetParam();
		} 
		set
		{ 
			SetParam(value);
		}
	}

	void Awake()
	{
		_ts = ToggleObj.gameObject.GetComponent<Toggle>();
		_tx = TextObj.gameObject.GetComponent<Text>();
		_tor = ToggleObj.GetComponent<RectTransform>();
		_txr = TextObj.GetComponent<RectTransform>();
	}

    public void SetTextFontSize(int fontSize)
    {
        _tx.fontSize = fontSize;
    }

    public void SetContentAlpha(float alpha)
    {

    }

    public bool GetLockState()
    {
        return false;
    }

    public bool GetSlowDownState()
    {
        return false;
    }

    public void BoolChanged()
	{
		//if(ParameterChange != null)
		//{
		//	ParameterChange(Parameters);
		//}
	}

	private object[] GetParam()
	{
		return new object[] { _ts.isOn, _tx.text, _ts.enabled, _tx.color, _tx.fontSize, _tx.fontStyle, _tx.font };
	}

	private void SetParam(object[] Vals)
	{
		if(Vals.Length <= 7)
		{
			bool good = true;
			for(int i = 0; i < Vals.Length; i++)
			{
				switch(i)
				{
				case 0:
					if(!((Vals[i] is bool) || (Vals[i] == null)))
					{
						good = false;
					}
					break;
				case 1:
					if(!((Vals[i] is string) || (Vals[i] == null)) )
					{
						good = false;
					}
					break;
				case 2:
					if(!((Vals[i] is bool) || (Vals[i] == null)) )
					{
						good = false;
					}
					break;
				case 3:
					if(!((Vals[i] is Color) || (Vals[i] == null)) )
					{
						good = false;
					}
					break;
				case 4:
					if(!((Vals[i] is int) || (Vals[i] == null)) )
					{
						good = false;
					}
					break;
				case 5:
					if(!((Vals[i] is FontStyle) || (Vals[i] == null)) )
					{
						good = false;
					}
					break;
				case 6:
					if(!((Vals[i] is Font) || (Vals[i] == null)) )
					{
						good = false;
					}
					break;
				default:
					break;
				}
			}
			if(good)
			{
				for(int i = 0; i < Vals.Length; i++)
				{
					switch(i)
					{
					case 0:
						if(!(Vals[i] == null))
						{
							_ts.isOn = (bool)Vals[i];
						}
						break;
					case 1:
						if(!(Vals[i] == null))
						{
							_tx.text = (string)Vals[i];
						}
						break;
					case 2:
						if(!(Vals[i] == null))
						{
							_ts.enabled = (bool)Vals[i];
						}
						break;
					case 3:
						if(!(Vals[i] == null))
						{
							_tx.color = (Color)Vals[i];
						}
						break;
					case 4:
						if(!(Vals[i] == null))
						{
							_tx.fontSize = (int)Vals[i];
						}
						break;
					case 5:
						if(!(Vals[i] == null))
						{
							_tx.fontStyle = (FontStyle)Vals[i];
						}
						break;
					case 6:
						if(!(Vals[i] == null))
						{
							_tx.font = (Font)Vals[i];	
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
