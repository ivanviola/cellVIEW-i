using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

public static class TreeUtility
{
    

    public static GameObject InstantiateNodeField(string type)
    {
        // Instantiate node
        if (type == "Text")
        {
            return GameObject.Instantiate(Resources.Load("TextField", typeof(GameObject))) as GameObject;
        }
        else if(type == "Toggle")
        {
            return GameObject.Instantiate(Resources.Load("ToggleField", typeof(GameObject))) as GameObject;
        }
        else if (type == "Range")
        {
            return GameObject.Instantiate(Resources.Load("RangeFieldItem", typeof(GameObject))) as GameObject;
        }

        return null;
    }
}
