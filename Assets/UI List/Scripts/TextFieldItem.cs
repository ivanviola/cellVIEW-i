using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TextFieldItem : MonoBehaviour, IItemInterface
{
    private Text MyObj;
    

	/// <summary>
	/// Parameters = new object[]{ string DisplayText }   OR
	/// Parameters = new object[]{ string DisplayText, Color FontColor }  OR
	/// Parameters = new object[]{ string DisplayText, Color FontColor, int FontSize }  OR
	/// Parameters = new object[]{ string DisplayText, Color FontColor, int FontSize, FontStyle fontstyle }  OR
	/// Parameters = new object[]{ string DisplayText, Color FontColor, int FontSize, FontStyle fontstyle, Font font } 
	/// </summary>
	/// <value>The parameters.</value>
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

	void Awake()
	{
		MyObj = this.GetComponent<Text>();
	}

    public void SetTextFontSize(int fontSize)
    {
        MyObj.fontSize = fontSize;
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

    private object[] GetVals()
	{
		return new object[]{ MyObj.text, MyObj.color, MyObj.fontSize, MyObj.fontStyle, MyObj.font };
	}

	private void SetVals(object[] Vals)
	{
		if(Vals.Length <= 5)
		{
			bool good = true;
			for(int i = 0; i < Vals.Length; i++)
			{
				switch(i)
				{
				case 0:
					if(!(Vals[i] is string))
					{
						good = false;
					}
					break;
				case 1:
					if(!((Vals[i] is Color) || (Vals[i] == null)) )
					{
						good = false;
					}
					break;
				case 2:
					if(!((Vals[i] is int) || (Vals[i] == null)) )
					{
						good = false;
					}
					break;
				case 3:
					if(!((Vals[i] is FontStyle) || (Vals[i] == null)) )
					{
						good = false;
					}
					break;
				case 4:
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
						MyObj.text = (string)Vals[i];
						break;
					case 1:
						if(Vals[i] != null)
						{
							MyObj.color = (Color)Vals[i];
						}
						break;
					case 2:
						if(Vals[i] != null)
						{
							MyObj.fontSize = (int)Vals[i];	
						}
						break;
					case 3:
						if(Vals[i] != null)
						{
							MyObj.fontStyle = (FontStyle)Vals[i];	
						}
						break;
					case 4:
						if(Vals[i] != null)
						{
							MyObj.font = (Font)Vals[i];
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
