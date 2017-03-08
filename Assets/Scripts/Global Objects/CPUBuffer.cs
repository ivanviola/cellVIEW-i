using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[ExecuteInEditMode]
public class CPUBuffers : MonoBehaviour
{
    // Declare the buffer manager as a singleton
    private static CPUBuffers _instance = null;

    public static CPUBuffers Get
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<CPUBuffers>();
                if (_instance == null)
                {
                    var go = GameObject.Find("_CPUBuffers");
                    if (go != null)
                        DestroyImmediate(go);

                    go = new GameObject("_CPUBuffers") { hideFlags = HideFlags.HideInInspector };
                    _instance = go.AddComponent<CPUBuffers>();
                }
            }
            return _instance;
        }
    }

    //*** Ingredients ****//

    [HideInInspector]
    public List<float> IngredientEdgeOpacity = new List<float>();

    [HideInInspector]
    public List<Vector4> LipidAtomPositions = new List<Vector4>();
    [HideInInspector]
    public List<Vector4> LipidInstanceInfos = new List<Vector4>();
    [HideInInspector]
    public List<Vector4> LipidInstanceInfos2 = new List<Vector4>();
    [HideInInspector]
    public List<Vector4> LipidInstancePositions = new List<Vector4>();

    // Protein ingredients data
    [HideInInspector]
    public List<Vector4> ProteinInstanceInfos = new List<Vector4>();
    [HideInInspector]
    public List<Vector4> ProteinInstancePositions = new List<Vector4>();
    [HideInInspector]
    public List<Vector4> ProteinInstanceRotations = new List<Vector4>();

    [HideInInspector]
    public List<uint> ProteinInstanceRotations_c = new List<uint>();


    [HideInInspector]
    public List<int> ProteinAtomCount = new List<int>();
    [HideInInspector]
    public List<int> ProteinAtomStart = new List<int>();
    [HideInInspector]
    public List<int> ProteinToggleFlags = new List<int>();
    [HideInInspector]
    public List<Vector4> ProteinAtoms = new List<Vector4>();
    [HideInInspector]
    public List<Vector4> ProteinAtomInfo = new List<Vector4>();    
    [HideInInspector]
    public List<Vector4> ProteinAtomInfo2 = new List<Vector4>();
    [HideInInspector]
    public List<float> ProteinIngredientsRadii = new List<float>();
    [HideInInspector]
    public List<Vector4> ProteinAtomClusters = new List<Vector4>();
    [HideInInspector]
    public List<int> ProteinAtomClusterCount = new List<int>();
    [HideInInspector]
    public List<int> ProteinAtomClusterStart = new List<int>();

    // Curve ingredients data
    [HideInInspector]
    public List<int> CurveIngredientsAtomStart = new List<int>();
    [HideInInspector]
    public List<int> CurveIngredientsAtomCount = new List<int>();
    [HideInInspector]
    public List<int> CurveIngredientToggleFlags = new List<int>();

    [HideInInspector]
    public List<Vector4> CurveIngredientsAtoms = new List<Vector4>();
    [HideInInspector]
    public List<Vector4> CurveIngredientsInfos = new List<Vector4>();
    [HideInInspector]
    public List<Vector4> CurveIngredientsColors = new List<Vector4>();

    [HideInInspector]
    public List<Vector4> CurveControlPointsInfos = new List<Vector4>();
    [HideInInspector]
    public List<Vector4> CurveControlPointsNormals = new List<Vector4>();
    [HideInInspector]
    public List<Vector4> CurveControlPointsPositions = new List<Vector4>();

    // Color

    [HideInInspector]
    public List<DisplayInfo> IngredientsDisplayInfo = new List<DisplayInfo>();

    [HideInInspector]
    public List<DisplayInfo> IngredientGroupsDisplayInfo = new List<DisplayInfo>();




    [HideInInspector]
    public List<Vector4> ProteinIngredientsChainColors = new List<Vector4>();

    [HideInInspector]
    public List<Vector4> IngredientsColors = new List<Vector4>();
    [HideInInspector]
    public List<Vector4> IngredientsInfo = new List<Vector4>();
    [HideInInspector]
    public List<Color> IngredientGroupsColor = new List<Color>();







    //*******//

    [HideInInspector]
    public List<float> IngredientGroupsLerpFactors = new List<float>();
    [HideInInspector]
    public List<Vector4> IngredientGroupsColorRanges = new List<Vector4>();
    [HideInInspector]
    public List<Vector4> IngredientGroupsColorValues = new List<Vector4>();
    [HideInInspector]
    public List<Vector4> ProteinIngredientsRandomValues = new List<Vector4>();

    //Cut objects

    [NonSerialized]
    public List<HistStruct> HistogramData = new List<HistStruct>();
    [NonSerialized]
    public List<int> IngredientToNodeLookup = new List<int>();
    [NonSerialized]
    public List<int> NodeToIngredientLookup = new List<int>();

    
    //**********************************************//


    // Clear all the CPU buffers
    public void Clear()
    {
        // Clear all lists
        foreach (var field in GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            if (field.FieldType.FullName.Contains("System.Collections.Generic.List"))
            {
                var v = field.GetValue(this) as IList;
                if (v == null) continue;
                v.Clear();
            }
        }
    }

    void InitHistogramLookups()
    {
        // Init histogram GPU buffer
        CPUBuffers.Get.HistogramData.Clear();
        foreach (var path in SceneManager.Get.SceneHierarchy)
        {
            var hist = new HistStruct
            {
                parent = -1,
                all = 0,
                cutaway = 0,
                occluding = 0,
                visible = 0
            };

            if (MyUtility.IsPathRoot(path))
            {
                hist.parent = -1;
            }
            else
            {
                var parentPath = MyUtility.GetParentUrlPath(path);
                if (!SceneManager.Get.SceneHierarchy.Contains(parentPath)) throw new Exception("Hierarchy corrupted");
                hist.parent = SceneManager.Get.SceneHierarchy.IndexOf(parentPath);
            }

            CPUBuffers.Get.HistogramData.Add(hist);
        }

        //*******************************//

        CPUBuffers.Get.IngredientToNodeLookup.Clear();

        foreach (var ingredientName in SceneManager.Get.AllIngredientNames)
        {
            if (SceneManager.Get.SceneHierarchy.Contains(ingredientName))
            {
                CPUBuffers.Get.IngredientToNodeLookup.Add(SceneManager.Get.SceneHierarchy.IndexOf(ingredientName));
            }
        }

        //*******************************//

        CPUBuffers.Get.NodeToIngredientLookup.Clear();

        foreach (var path in SceneManager.Get.SceneHierarchy)
        {
            if (SceneManager.Get.AllIngredientNames.Contains(path))
            {
                CPUBuffers.Get.NodeToIngredientLookup.Add(SceneManager.Get.AllIngredientNames.IndexOf(path));
            }
            else
            {
                CPUBuffers.Get.NodeToIngredientLookup.Add(-1);
            }
        }
    }

    public void CopyDataToGPU()
    {
        CutObjectManager.Get.UpdateCutObjectParams();

        InitHistogramLookups();
        
        //CheckBufferSizes();

        GPUBuffers.Get.InitBuffers();

        var initArgs = new[] {0, 1, 0, 0};
        GPUBuffers.Get.ArgBuffer.SetData(initArgs);

        for (var i = 0; i < AtomHelper.AtomColors.Length; i++)
        {
           var c = AtomHelper.AtomColors[i];
           AtomHelper.AtomColors[i] = new Color(c.r, c.g, c.b, AtomHelper.AtomRadii[i]);
        }

        GPUBuffers.Get.AtomColors.SetData(AtomHelper.AtomColors);
        GPUBuffers.Get.AminoAcidColors.SetData(AtomHelper.ResidueColors);
        GPUBuffers.Get.IngredientsColors.SetData(CPUBuffers.Get.IngredientsColors.ToArray());
        GPUBuffers.Get.IngredientsChainColors.SetData(CPUBuffers.Get.ProteinIngredientsChainColors.ToArray());
        GPUBuffers.Get.IngredientsInfo.SetData(CPUBuffers.Get.IngredientsInfo.ToArray());
        GPUBuffers.Get.IngredientGroupsColor.SetData(CPUBuffers.Get.IngredientGroupsColor.ToArray());

        //*****//

        GPUBuffers.Get.IngredientGroupsLerpFactors.SetData(CPUBuffers.Get.IngredientGroupsLerpFactors.ToArray());
        GPUBuffers.Get.IngredientGroupsColorRanges.SetData(CPUBuffers.Get.IngredientGroupsColorRanges.ToArray());
        GPUBuffers.Get.IngredientGroupsColorValues.SetData(CPUBuffers.Get.IngredientGroupsColorValues.ToArray());
        GPUBuffers.Get.ProteinIngredientsRandomValues.SetData(CPUBuffers.Get.ProteinIngredientsRandomValues.ToArray());


        // Upload histogram info
        GPUBuffers.Get.Histograms.SetData(CPUBuffers.Get.HistogramData.ToArray());
        GPUBuffers.Get.HistogramsLookup.SetData(CPUBuffers.Get.IngredientToNodeLookup.ToArray());

        // Upload Lod levels info
        GPUBuffers.Get.LodInfo.SetData(GlobalProperties.Get.LodLevels);

        // Upload ingredient data
        GPUBuffers.Get.ProteinRadii.SetData(CPUBuffers.Get.ProteinIngredientsRadii.ToArray());
        GPUBuffers.Get.IngredientsColors.SetData(CPUBuffers.Get.IngredientsColors.ToArray());
        GPUBuffers.Get.IngredientMaskParams.SetData(CPUBuffers.Get.ProteinToggleFlags.ToArray());

        GPUBuffers.Get.ProteinAtoms.SetData(CPUBuffers.Get.ProteinAtoms.ToArray());
        GPUBuffers.Get.ProteinAtomInfo.SetData(CPUBuffers.Get.ProteinAtomInfo.ToArray());
        GPUBuffers.Get.ProteinAtomInfo2.SetData(CPUBuffers.Get.ProteinAtomInfo2.ToArray());
        GPUBuffers.Get.ProteinAtomCount.SetData(CPUBuffers.Get.ProteinAtomCount.ToArray());
        GPUBuffers.Get.ProteinAtomStart.SetData(CPUBuffers.Get.ProteinAtomStart.ToArray());

        GPUBuffers.Get.ProteinAtomClusters.SetData(CPUBuffers.Get.ProteinAtomClusters.ToArray());
        GPUBuffers.Get.ProteinAtomClusterCount.SetData(CPUBuffers.Get.ProteinAtomClusterCount.ToArray());
        GPUBuffers.Get.ProteinAtomClusterStart.SetData(CPUBuffers.Get.ProteinAtomClusterStart.ToArray());

        GPUBuffers.Get.ProteinInstancesInfo.SetData(CPUBuffers.Get.ProteinInstanceInfos.ToArray());
        GPUBuffers.Get.ProteinInstancePositions.SetData(CPUBuffers.Get.ProteinInstancePositions.ToArray());
        GPUBuffers.Get.ProteinInstanceRotations.SetData(CPUBuffers.Get.ProteinInstanceRotations.ToArray());

        // Upload curve ingredient data
        GPUBuffers.Get.CurveIngredientsAtoms.SetData(CPUBuffers.Get.CurveIngredientsAtoms.ToArray());
        GPUBuffers.Get.CurveIngredientsAtomCount.SetData(CPUBuffers.Get.CurveIngredientsAtomCount.ToArray());
        GPUBuffers.Get.CurveIngredientsAtomStart.SetData(CPUBuffers.Get.CurveIngredientsAtomStart.ToArray());

        GPUBuffers.Get.CurveIngredientsInfo.SetData(CPUBuffers.Get.CurveIngredientsInfos.ToArray());
        GPUBuffers.Get.CurveIngredientsColors.SetData(CPUBuffers.Get.CurveIngredientsColors.ToArray());
        GPUBuffers.Get.CurveIngredientsToggleFlags.SetData(CPUBuffers.Get.CurveIngredientToggleFlags.ToArray());

        GPUBuffers.Get.CurveControlPointsInfo.SetData(CPUBuffers.Get.CurveControlPointsInfos.ToArray());
        GPUBuffers.Get.CurveControlPointsNormals.SetData(CPUBuffers.Get.CurveControlPointsNormals.ToArray());
        GPUBuffers.Get.CurveControlPointsPositions.SetData(CPUBuffers.Get.CurveControlPointsPositions.ToArray());

        // Upload lipid data
        GPUBuffers.Get.LipidAtomPositions.SetData(CPUBuffers.Get.LipidAtomPositions.ToArray());
        GPUBuffers.Get.LipidInstancesInfo.SetData(CPUBuffers.Get.LipidInstanceInfos.ToArray());
        GPUBuffers.Get.LipidInstancesInfo2.SetData(CPUBuffers.Get.LipidInstanceInfos2.ToArray());
        GPUBuffers.Get.LipidInstancePositions.SetData(CPUBuffers.Get.LipidInstancePositions.ToArray());
    }
    
}
