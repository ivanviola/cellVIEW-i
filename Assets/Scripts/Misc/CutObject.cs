using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum CutType
{
    Plane = 0,
    Sphere = 1,
    Cube = 2,
    Cylinder = 3,
    Cone = 4,
    None = 5
};

[System.Serializable]
public class CutItem
{
    public string Name;
    public bool State;
}

[System.Serializable]
public class CutItemRanges //i will maybe use this later?
{
    public string Name;
    public int r0;
    public int r1;
    public int r2;

    public float d0; // delta 0
    public float d1; // delta 1
}

public enum LockState
{
    Unlocked = 0,
    Locked = 1,
    Consumed = 2,
    Restore = 3
};

public class CutParameters
{
    public int Id;
    public bool IsFocus;
    public bool IsHighlighted;
    public float Aperture;

    public float range0;
    public float range1;

    public int countAll; //parameters of the multi-range slider
    public int count0;
    public int count1;

    public float value1; //other parameters
    public float value2;
    public float fuzziness;
    public float fuzzinessDistance;
    public float fuzzinessCurve;
}


[ExecuteInEditMode]
public class CutObject : MonoBehaviour
{
    public static int UniqueId;
    public static float CutObjectAlphaStatic = 0.4f;

    [HideInInspector]
    public int Id;

    [HideInInspector]
    public int LastNodeFocusId = -1;

    [HideInInspector]
    public LockState CurrentLockState = LockState.Unlocked;
    
    public bool Hidden;
    public bool HardCut;
    public bool Inverse;
    public CutType CutType;

    private float _cutObjectAlpha;

    [Range(0,1)]
    public float CutObjectAlpha;
    
    //[Range(0, 1)]
    //public float Value1;

    //[Range(0, 1)]
    //public float Value2;

    //[Range(0, 1)]
    //public float Fuzziness;

    //[Range(0, 1)]
    //public float FuzzinessDistance;

    //[Range(0.01f, 3)]
    //public float FuzzinessCurve;
    
    [HideInInspector]
    public CutType PreviousCutType;

    [HideInInspector]
    public List<CutItem> ProteinCutFilters = new List<CutItem>();

    [HideInInspector]
    public List<CutItem> HistogramProteinTypes = new List<CutItem>();

    [HideInInspector]
    public List<CutItemRanges> HistogramRanges = new List<CutItemRanges>(); //i will maybe use this later?

    [HideInInspector]
    public List<CutParameters> IngredientCutParameters = new List<CutParameters>(); //this structure stores the cutaway parameters per protein type

	//private TreeViewControlEditor _tree;
	//private RecipeTreeUI _tree_ui;

    [HideInInspector]
    public float[] RangeValues = new float[2] { 0.2f, 0.3f };
    public List<float[]> TreeRangeValues = new List<float[]>();

    [HideInInspector]
    public bool initOptimizing = true;
    [HideInInspector]
    public bool distanceOptimized = false;
    [HideInInspector]
    public float findDistanceFrom = 0.0f;
    [HideInInspector]
    public float findDistanceTo = 1.0f;
    [HideInInspector]
    public float initialRange0 = 0.0f;
    [HideInInspector]
    public float initialRange1 = 0.0f;

    [NonSerialized] public int SelectedProteinType = -1;

    public void InitCutParameters()
    {
        var ingredientsCount = SceneManager.Get.NumAllIngredients;
        IngredientCutParameters.Clear();

        for (int i = 0; i < ingredientsCount; i++)
        {
            IngredientCutParameters.Add(
                new CutParameters()
                {
                    Id = i,
                    IsFocus = false,
                    IsHighlighted = false,
                    Aperture = 0.0f,
                    range0 = 0.0f,
                    range1 = 0.0f,

                    countAll = 0,
                    count0 = 0,
                    count1 = 0,

                    value1 = 0.5f,
                    value2 = 1.0f,
                    fuzziness = 0.0f,
                    fuzzinessDistance = 1.0f,
                    fuzzinessCurve = 1.0f
                }
                );   
        }
    }

    public CutParameters GetCutParametersFor(int ingredientId)
    {
        if (IngredientCutParameters.Count == 0 || IngredientCutParameters.Count <= ingredientId)
        {
            InitCutParameters();
        }

        return IngredientCutParameters[ingredientId];
    }

    public void SetCutParametersFor(int ingredientId, CutParameters cutParameters)
    {
        if (IngredientCutParameters.Count == 0 || IngredientCutParameters.Count <= ingredientId)
        {
            InitCutParameters();
        }

        IngredientCutParameters[ingredientId] = cutParameters;
    }

    public void SetFocusFor(int ingredientId, bool value)
    {
        IngredientCutParameters[ingredientId].IsFocus = value;
    }

    public void SetValue1For(int ingredientId, float value1)
    {
        IngredientCutParameters[ingredientId].value1 = value1;
    }

    public void SetValue2For(int ingredientId, float value2)
    {
        IngredientCutParameters[ingredientId].value2 = value2;
    }
    
    public void SetFuzzinessParametersFor(int ingredientId, float value1, float value2, float value3)
    {
        IngredientCutParameters[ingredientId].fuzziness = value1;
        IngredientCutParameters[ingredientId].fuzzinessDistance = value2;
        IngredientCutParameters[ingredientId].fuzzinessCurve = value3;
    }

    public float[] GetRangeValues(int ingredientId)
    {
        //return RangeValues;
        int count = TreeRangeValues.Count;
        if (count <= ingredientId)
        {
            if (ingredientId >= TreeRangeValues.Capacity)
                TreeRangeValues.Capacity = ingredientId + 1;
            TreeRangeValues.AddRange(Enumerable.Repeat(new float[2] {0.5f, 0.1f} , ingredientId + 1 - count));
        }

        return TreeRangeValues[ingredientId];
    }

    public void SetRangeValues(int ingredientId, float[] rangeValues)
    {
        RangeValues = rangeValues;
        int count = TreeRangeValues.Count;
        if (count <= ingredientId)
        {
            if (ingredientId >= TreeRangeValues.Capacity)
                TreeRangeValues.Capacity = ingredientId + 1;
            TreeRangeValues.AddRange(Enumerable.Repeat(new float[2] { 0.3f, 0.1f }, ingredientId + 1 - count));
        }
        TreeRangeValues[ingredientId] = rangeValues;
    }


    public void SetCutItems(List<string> names)
    {
        foreach(var name in names)
        {
            /*ProteinCutFilters.Add(new CutItem() { Name = name, State = true });
            HistogramProteinTypes.Add(new CutItem() { Name = name, State = true });
            HistogramRanges.Add(new CutItemRanges() { Name = name, r0 = 0, r1 = 0, r2 = 0, d0 = 0.0f, d1 = 0.0f });*/
            AddCutItem(name);
        }
    }

	public void RemoveCutItem (string name)
    {
		CutItem toRemove=null;
		foreach(CutItem cu in ProteinCutFilters){
			if (string.Equals(cu.Name,name)){
				toRemove = cu;
				break;
			}
		}
		if (toRemove != null)ProteinCutFilters.Remove(toRemove);

        toRemove = null;
        foreach (CutItem cu in HistogramProteinTypes)
        {
            if (string.Equals(cu.Name, name))
            {
                toRemove = cu;
                break;
            }
        }
        if (toRemove != null) HistogramProteinTypes.Remove(toRemove);

        CutItemRanges toRemoveR = null;
        foreach (CutItemRanges cu in HistogramRanges)
        {
            if (string.Equals(cu.Name, name))
            {
                toRemoveR = cu;
                break;
            }
        }
        if (toRemoveR != null) HistogramRanges.Remove(toRemoveR);
    }

	public  void AddCutItem (string name)
    {
		ProteinCutFilters.Add(new CutItem() { Name = name, State = true });
        HistogramProteinTypes.Add(new CutItem() { Name = name, State = true });
        HistogramRanges.Add(new CutItemRanges() { Name = name, r0 = 0, r1 = 0, r2 = 0, d0 = 0.0f, d1 = 0.0f });
	}

	public void ToggleCutItem (string name, bool toggle)
    {
		foreach(CutItem cu in ProteinCutFilters){
			if (string.Equals(cu.Name,name)){

                Debug.Log("toggling " + name);

				cu.State = toggle;
				break;
			}
		}
    }

	public void ToggleAllCutItem (bool toggle)
    {
		foreach(CutItem cu in ProteinCutFilters)
        {
			cu.State = toggle;
		}
    }


    public void ToggleAllHistogramCutItems(bool toggle)
    {
        foreach (CutItem cu in HistogramProteinTypes)
        {
            cu.State = toggle;
        }
    }
    public void ToggleHistogramCutItem(string name, bool toggle)
    {
        foreach (CutItem cu in HistogramProteinTypes)
        {
            if (string.Equals(cu.Name, name))
            {
                cu.State = toggle;
                break;
            }
        }
    }

    
    private bool previousHiddenValue;

    public void Update ()
    {
        if (CutObjectAlpha != _cutObjectAlpha)
        {
            _cutObjectAlpha = CutObjectAlpha;
            CutObjectAlphaStatic = CutObjectAlpha;
        }

        if (CutObjectAlphaStatic != CutObjectAlpha)
        {
            _cutObjectAlpha = CutObjectAlphaStatic;
            CutObjectAlpha = CutObjectAlphaStatic;
        }


        if (CutType != PreviousCutType || gameObject.GetComponent<MeshFilter>().sharedMesh == null)
        {
            SetMesh();
            PreviousCutType = CutType;
        }

        if (previousHiddenValue != Hidden)
        {
            SetHidden(Hidden);
            previousHiddenValue = Hidden;

        }

        //SetHidden(Hidden);
        //if (CutObjectAlpha == 0) SetHidden(true);

        GetComponent<MeshRenderer>().sharedMaterial.SetFloat("_CutObjectAlpha", CutObjectAlphaStatic);
    }

    void Awake()
    {
        _cutObjectAlpha = CutObjectAlpha;
        //CutObjectAlphaStatic = CutObjectAlpha;

        UniqueId++;
        Id = UniqueId;
    }

    void OnEnable()
    {
        InitCutParameters();

        // Register this object in the cut object cache
        if (!CutObjectManager.Get.CutObjects.Contains(this))
        {
            CutObjectManager.Get.CutObjects.Add(this);
        }

        Hidden = true;
        previousHiddenValue = !Hidden;

        SetMesh();
        
    }

    void OnDisable()
    {
        // De-register this object in the cut object cache
        if (SceneManager.CheckInstance() && CutObjectManager.Get.CutObjects.Contains(this))
        {
            if (CurrentLockState != LockState.Unlocked)
            {
                CutObjectManager.Get.ResetCutSnapshot = Id;
            }
            CutObjectManager.Get.CutObjects.Remove(this);
        }
    }

    public void SetHidden(bool value, bool highlight = false)
    {
        if (CutType == CutType.None)
        {
            gameObject.GetComponent<MeshRenderer>().enabled = false;
            gameObject.GetComponent<TransformHandle>().enabled = false;
        }
        else
        {
            gameObject.GetComponent<MeshRenderer>().enabled = !value;
            gameObject.GetComponent<TransformHandle>().enabled = !value;
            if (gameObject.GetComponent<Collider>() != null) gameObject.GetComponent<Collider>().enabled = !value;
            if (highlight) SelectionManager.Instance.SetHandleSelected(gameObject.GetComponent<TransformHandle>());
        }
    }

    public void SetMesh()
    {
        switch (CutType)
        {
            case CutType.Plane:
                gameObject.GetComponent<MeshFilter>().sharedMesh = Resources.Load("Meshes/Plane") as Mesh;
                DestroyImmediate(gameObject.GetComponent<Collider>());
                gameObject.AddComponent<BoxCollider>();
                gameObject.GetComponent<MeshRenderer>().enabled = true;
                gameObject.GetComponent<TransformHandle>().enabled = true;
                //gameObject.transform.localScale = new Vector3(10,10,10);
                break;

            case CutType.Sphere:
                gameObject.GetComponent<MeshFilter>().sharedMesh = Resources.Load("Meshes/Sphere") as Mesh;
                DestroyImmediate(gameObject.GetComponent<Collider>());
                gameObject.AddComponent<SphereCollider>();
                gameObject.GetComponent<MeshRenderer>().enabled = true;
                gameObject.GetComponent<TransformHandle>().enabled = true;
                //gameObject.transform.localScale = new Vector3(50, 50, 50);
                break;

            case CutType.Cube:
                gameObject.GetComponent<MeshFilter>().sharedMesh = Resources.Load("Meshes/Cube") as Mesh;
                DestroyImmediate(gameObject.GetComponent<Collider>());
                gameObject.AddComponent<MeshCollider>();
                gameObject.GetComponent<MeshRenderer>().enabled = true;
                gameObject.GetComponent<TransformHandle>().enabled = true;
                //gameObject.transform.localScale = new Vector3(50, 50, 50);
                break;

            case CutType.Cone:
                gameObject.GetComponent<MeshFilter>().sharedMesh = Resources.Load("Meshes/Cone") as Mesh;
                DestroyImmediate(gameObject.GetComponent<Collider>());
                gameObject.AddComponent<MeshCollider>();
                gameObject.GetComponent<MeshRenderer>().enabled = true;
                gameObject.GetComponent<TransformHandle>().enabled = true;
                break;

            case CutType.Cylinder:
                gameObject.GetComponent<MeshFilter>().sharedMesh = Resources.Load("Meshes/Cylinder") as Mesh;
                DestroyImmediate(gameObject.GetComponent<Collider>());
                gameObject.AddComponent<MeshCollider>();
                gameObject.GetComponent<MeshRenderer>().enabled = true;
                gameObject.GetComponent<TransformHandle>().enabled = true;
                break;

            case CutType.None:
                gameObject.GetComponent<MeshRenderer>().enabled = false;
                gameObject.GetComponent<TransformHandle>().enabled = false;
                break;
        }
    }

    public List<float> GetDistinctValue2()
    {
        var distincValue2List = new List<float>();
        foreach (var cutParam in IngredientCutParameters)
        {
            if (!distincValue2List.Contains(cutParam.value2) && cutParam.value2 != 0)
            {
                distincValue2List.Add(cutParam.value2);
            }
        }

        return distincValue2List;
    }

    public List<int> GetAllWithValue2(float value2)
    {
        var list = new List<int>();
        for(int i = 0; i < IngredientCutParameters.Count; i++)
        {

            if (IngredientCutParameters[i].value2 == value2)
            {
                list.Add(i);
            }
        }

        return list;
    }

    public List<int> GetAllNonIgnorePriorityDraw()
    {
        var list = new List<int>();
        for (int i = 0; i < IngredientCutParameters.Count; i++)
        {

            if (IngredientCutParameters[i].IsFocus)
            {
                list.Add(i);
            }
        }

        return list;
    }
}
