using System;
using System.Reflection;
using UnityEngine;

[ExecuteInEditMode]
public class GPUBuffers : MonoBehaviour
{
    // Declare the buffer manager as a singleton
    private static GPUBuffers _instance = null;
    public static GPUBuffers Get
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GPUBuffers>();
                if (_instance == null)
                {
                    var go = GameObject.Find("_GPUBuffers");
                    if (go != null)
                        DestroyImmediate(go);

                    go = new GameObject("_GPUBuffers") { hideFlags = HideFlags.HideInInspector };
                    _instance = go.AddComponent<GPUBuffers>();
                }
            }
            return _instance;
        }
    }

    //----------------------------------------------

    public static int NumIngredientMax = 100;
    public static void CheckNumIngredientMax(int value)
    {
        if (value >= NumIngredientMax) throw new Exception("GPU buffer overflow");
    }

    public static int NumLipidAtomMax = 10000000;
    public static void CheckNumLipidAtomMax(int value)
    {
        if (value >= NumLipidAtomMax) throw new Exception("GPU buffer overflow");
    }

    public static int NumLipidInstancesMax = 1000000;
    public static void CheckNumLipidInstancesMax(int value)
    {
        if (value >= NumLipidInstancesMax) throw new Exception("GPU buffer overflow");
    }
    
    public static int NumProteinTypeMax = 100;
    public static void CheckNumProteinTypeMax(int value)
    {
        if (value >= NumProteinTypeMax) throw new Exception("GPU buffer overflow");
    }
    
    public static int NumProteinAtomMax = 3000000;
    public static void CheckNumProteinAtomMax(int value)
    {
        if (value >= NumProteinAtomMax) throw new Exception("GPU buffer overflow");
    }
    
    public static int NumProteinAtomClusterMax = 1000000;
    public static void CheckNumProteinAtomClusterMax(int value)
    {
        if (value >= NumProteinAtomClusterMax) throw new Exception("GPU buffer overflow");
    }
    
    public static int NumProteinInstancesMax = 100000;
    public static void CheckNumProteinInstancesMax(int value)
    {
        if (value >= NumProteinInstancesMax) throw new Exception("GPU buffer overflow");
    }

    public static int NumCurveIngredientMax = 100;
    public static void CheckNumCurveIngredientMax(int value)
    {
        if (value >= NumCurveIngredientMax) throw new Exception("GPU buffer overflow");
    }

    public static int NumCurveControlPointsMax = 1000000;
    public static void CheckNumCurveControlPointsMax(int value)
    {
        if (value >= NumCurveControlPointsMax) throw new Exception("GPU buffer overflow");
    }

    public static int NumCurveIngredientAtomsMax = 10000;
    public static void CheckNumCurveIngredientAtomsMax(int value)
    {
        if (value >= NumCurveIngredientAtomsMax) throw new Exception("GPU buffer overflow");
    }
    
    public static int NumLodMax = 10;
    public static int NumCutsMax = 100;
    public static int NumSceneHierarchyNodes = 200;
    public static int NumProteinSphereBatchesMax = 2500000;

    /********************************/

    // Colors

    public ComputeBuffer ProteinFocusScore;

    public ComputeBuffer IngredientsColorInfo;
    public ComputeBuffer IngredientGroupsColorInfo;

    public ComputeBuffer IngredientsInfo;
    public ComputeBuffer IngredientGroupsColor;
    public ComputeBuffer IngredientsColors;
    public ComputeBuffer IngredientsChainColors;
    public ComputeBuffer AminoAcidColors;
    public ComputeBuffer AtomColors;

    public ComputeBuffer IngredientGroupsColorRanges;
    public ComputeBuffer IngredientGroupsColorValues;
    public ComputeBuffer IngredientGroupsLerpFactors;
    public ComputeBuffer ProteinIngredientsRandomValues;

    //******//

    public ComputeBuffer ArgBuffer;
    public ComputeBuffer LodInfo;
    public ComputeBuffer SphereBatches;
    
    public ComputeBuffer IngredientStates;
    public ComputeBuffer IngredientMaskParams;
    public ComputeBuffer IngredientEdgeOpacity;
    
    // Protein buffers
    public ComputeBuffer ProteinRadii;

    public ComputeBuffer ProteinAtoms;
    public ComputeBuffer ProteinAtomInfo;
    public ComputeBuffer ProteinAtomInfo2;
    public ComputeBuffer ProteinAtomCount;
    public ComputeBuffer ProteinAtomStart;

    public ComputeBuffer ProteinAtomClusters;
    public ComputeBuffer ProteinAtomClusterCount;
    public ComputeBuffer ProteinAtomClusterStart;

    public ComputeBuffer ProteinInstancesInfo;
    public ComputeBuffer ProteinInstancePositions;
    public ComputeBuffer ProteinInstanceRotations;
    public ComputeBuffer ProteinInstanceCullFlags;
    public ComputeBuffer ProteinInstanceOcclusionFlags;
    public ComputeBuffer ProteinInstanceVisibilityFlags;

    // lipid buffers
    public ComputeBuffer LipidAtomPositions;
    public ComputeBuffer LipidInstancesInfo;
    public ComputeBuffer LipidInstancesInfo2; // I need to add another Info buffer because all the components on the original one are used
                                              // I'm gonna make it float4 just in case I need to put another info there
    public ComputeBuffer LipidInstancePositions;
    public ComputeBuffer LipidInstanceCullFlags;
    public ComputeBuffer LipidInstanceOcclusionFlags;
    public ComputeBuffer LipidInstanceVisibilityFlags;

    // Curve ingredients buffers
    public ComputeBuffer CurveIngredientsInfo;
    public ComputeBuffer CurveIngredientsColors;
    public ComputeBuffer CurveIngredientsToggleFlags;

    public ComputeBuffer CurveIngredientsAtoms;
    public ComputeBuffer CurveIngredientsAtomCount;
    public ComputeBuffer CurveIngredientsAtomStart;

    public ComputeBuffer CurveControlPointsInfo;
    public ComputeBuffer CurveControlPointsNormals;
    public ComputeBuffer CurveControlPointsPositions;

    // Cut Objects
    public ComputeBuffer CutItems;
    public ComputeBuffer CutInfo;
    public ComputeBuffer CutScales;
    public ComputeBuffer CutPositions;
    public ComputeBuffer CutRotations;
    //public ComputeBuffer ProteinCutFilters;
    //public ComputeBuffer HistogramProteinTypes;
    //public ComputeBuffer HistogramStatistics;
    public ComputeBuffer HistogramsLookup;
    public ComputeBuffer Histograms;

    //*****//
    
    void OnEnable()
    {
        CheckBufferSizes();
        InitBuffers();
    }

    void OnDisable()
    {
        ReleaseBuffers();
    }

    private void CheckBufferSizes()
    {
        //GPUBuffers.CheckNumIngredientMax(AllIngredientNames.Count);
        //GPUBuffers.CheckNumLipidAtomMax(LipidAtomPositions.Count);
        //GPUBuffers.CheckNumLipidInstancesMax(LipidInstancePositions.Count);
        //GPUBuffers.CheckNumProteinTypeMax(ProteinIngredientNames.Count);
        //GPUBuffers.CheckNumProteinAtomMax(ProteinAtoms.Count);
        //GPUBuffers.CheckNumProteinAtomClusterMax(ProteinAtomClusters.Count);
        //GPUBuffers.CheckNumProteinInstancesMax(ProteinInstancePositions.Count);
        //GPUBuffers.CheckNumCurveIngredientMax(CurveIngredientNames.Count);
        //GPUBuffers.CheckNumCurveControlPointsMax(CurveControlPointsPositions.Count);
        //GPUBuffers.CheckNumCurveIngredientAtomsMax(CurveIngredientsAtoms.Count);

        //if (Get.ProteinAtomClusterCount.Count >= GPUBuffers.NumProteinTypeMax * GPUBuffers.NumLodMax) throw new Exception("GPU buffer overflow");
    }

    public void InitBuffers ()
    {

        if (ArgBuffer == null) ArgBuffer = new ComputeBuffer(4, sizeof(int), ComputeBufferType.DrawIndirect);


        if (AtomColors == null) AtomColors = new ComputeBuffer(16, 16);
        if (AminoAcidColors == null) AminoAcidColors = new ComputeBuffer(32, 16);
        if (IngredientsColors == null) IngredientsColors = new ComputeBuffer(NumProteinTypeMax, 16);
        if (IngredientGroupsColor == null) IngredientGroupsColor = new ComputeBuffer(16, 16);
        if (IngredientsInfo == null) IngredientsInfo = new ComputeBuffer(NumIngredientMax, 16);
        if (IngredientsChainColors == null) IngredientsChainColors = new ComputeBuffer(NumIngredientMax * 16, 16);

        if (IngredientGroupsColorInfo == null) IngredientGroupsColorInfo = new ComputeBuffer(16, 16);
        if (IngredientsColorInfo == null) IngredientsColorInfo = new ComputeBuffer(NumIngredientMax, 16);

        if (IngredientGroupsLerpFactors == null) IngredientGroupsLerpFactors = new ComputeBuffer(16, 4);
        if (IngredientGroupsColorRanges == null) IngredientGroupsColorRanges = new ComputeBuffer(16, 16);
        if (IngredientGroupsColorValues == null) IngredientGroupsColorValues = new ComputeBuffer(16, 16);
        if (ProteinIngredientsRandomValues == null) ProteinIngredientsRandomValues = new ComputeBuffer(NumIngredientMax, 16);

        //*****//

        if (LodInfo == null) LodInfo = new ComputeBuffer(8, 16);
        if (SphereBatches == null) SphereBatches = new ComputeBuffer(NumProteinSphereBatchesMax, 16, ComputeBufferType.Append);

        if (IngredientStates == null) IngredientStates = new ComputeBuffer(NumIngredientMax, 4);
        if (IngredientMaskParams == null) IngredientMaskParams = new ComputeBuffer(NumIngredientMax, 4);
        if (IngredientEdgeOpacity == null) IngredientEdgeOpacity = new ComputeBuffer(NumIngredientMax, 4);

        //*****//

        if (ProteinRadii == null) ProteinRadii = new ComputeBuffer(NumProteinTypeMax, 4);

        if (ProteinAtoms == null) ProteinAtoms = new ComputeBuffer(NumProteinAtomMax, 16);
        if (ProteinAtomInfo == null) ProteinAtomInfo = new ComputeBuffer(NumProteinAtomMax, 16);
        if (ProteinAtomInfo2 == null) ProteinAtomInfo2 = new ComputeBuffer(NumProteinAtomMax, 16);
        if (ProteinAtomClusters == null) ProteinAtomClusters = new ComputeBuffer(NumProteinAtomClusterMax, 16);

        if (ProteinAtomCount == null) ProteinAtomCount = new ComputeBuffer(NumProteinTypeMax, 4);
        if (ProteinAtomStart == null) ProteinAtomStart = new ComputeBuffer(NumProteinTypeMax, 4);
        if (ProteinAtomClusterCount == null) ProteinAtomClusterCount = new ComputeBuffer(NumProteinTypeMax * NumLodMax, 4);
        if (ProteinAtomClusterStart == null) ProteinAtomClusterStart = new ComputeBuffer(NumProteinTypeMax * NumLodMax, 4);

        if (ProteinInstancesInfo == null) ProteinInstancesInfo = new ComputeBuffer(NumProteinInstancesMax, 16);
        if (ProteinInstancePositions == null) ProteinInstancePositions = new ComputeBuffer(NumProteinInstancesMax, 16);
        if (ProteinInstanceRotations == null) ProteinInstanceRotations = new ComputeBuffer(NumProteinInstancesMax, 16);
        if (ProteinInstanceCullFlags == null) ProteinInstanceCullFlags = new ComputeBuffer(NumProteinInstancesMax, 4);

        if (ProteinFocusScore == null) ProteinFocusScore = new ComputeBuffer(NumProteinInstancesMax, 4);
        if (ProteinInstanceOcclusionFlags == null) ProteinInstanceOcclusionFlags = new ComputeBuffer(NumProteinInstancesMax, 4);
        if (ProteinInstanceVisibilityFlags == null) ProteinInstanceVisibilityFlags = new ComputeBuffer(NumProteinInstancesMax, 4);

        //*****//

        if (LipidAtomPositions == null) LipidAtomPositions = new ComputeBuffer(NumLipidAtomMax, 16);
        if (LipidInstancesInfo == null) LipidInstancesInfo = new ComputeBuffer(NumLipidInstancesMax, 16);
        if (LipidInstancesInfo2 == null) LipidInstancesInfo2 = new ComputeBuffer(NumLipidInstancesMax, 16);
        if (LipidInstancePositions == null) LipidInstancePositions = new ComputeBuffer(NumLipidInstancesMax, 16);
        if (LipidInstanceCullFlags == null) LipidInstanceCullFlags = new ComputeBuffer(NumLipidInstancesMax, 4);
        if (LipidInstanceOcclusionFlags == null) LipidInstanceOcclusionFlags = new ComputeBuffer(NumLipidInstancesMax, 4);
        if (LipidInstanceVisibilityFlags == null) LipidInstanceVisibilityFlags = new ComputeBuffer(NumLipidInstancesMax, 4);

        //*****//

        if (CurveIngredientsInfo == null) CurveIngredientsInfo = new ComputeBuffer(NumCurveIngredientMax, 16);
        if (CurveIngredientsColors == null) CurveIngredientsColors = new ComputeBuffer(NumCurveIngredientMax, 16);
        if (CurveIngredientsToggleFlags == null) CurveIngredientsToggleFlags = new ComputeBuffer(NumCurveIngredientMax, 4);

        if (CurveIngredientsAtomCount == null) CurveIngredientsAtomCount = new ComputeBuffer(NumCurveIngredientMax, 4);
        if (CurveIngredientsAtomStart == null) CurveIngredientsAtomStart = new ComputeBuffer(NumCurveIngredientMax, 4);
        if (CurveIngredientsAtoms == null) CurveIngredientsAtoms = new ComputeBuffer(NumCurveIngredientAtomsMax, 16);
        
        if (CurveControlPointsInfo == null) CurveControlPointsInfo = new ComputeBuffer(NumCurveControlPointsMax, 16);
        if (CurveControlPointsNormals == null) CurveControlPointsNormals = new ComputeBuffer(NumCurveControlPointsMax, 16);
        if (CurveControlPointsPositions == null) CurveControlPointsPositions = new ComputeBuffer(NumCurveControlPointsMax, 16);

        //*****//
        
        if (CutInfo == null) CutInfo = new ComputeBuffer(NumCutsMax * NumProteinTypeMax, 48);
        if (CutScales == null) CutScales = new ComputeBuffer(NumCutsMax, 16);
        if (CutPositions == null) CutPositions = new ComputeBuffer(NumCutsMax, 16);
        if (CutRotations == null) CutRotations = new ComputeBuffer(NumCutsMax, 16);
        //if (ProteinCutFilters == null) ProteinCutFilters = new ComputeBuffer(NumCutsMax * NumProteinMax, 4);
        //if (HistogramProteinTypes == null) HistogramProteinTypes = new ComputeBuffer(NumCutsMax * NumProteinMax, 4);
        //if (HistogramStatistics == null) HistogramStatistics = new ComputeBuffer(4, 4);
        if (HistogramsLookup == null) HistogramsLookup = new ComputeBuffer(NumProteinTypeMax, 4);
        if (Histograms == null) Histograms = new ComputeBuffer(NumSceneHierarchyNodes, 32);
    }
	
	// Flush buffers on exit
	void ReleaseBuffers ()
    {
        // Use reflection to release all the buffers in the scene
        foreach (var field in GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            if (field.FieldType.FullName.Contains("UnityEngine.ComputeBuffer"))
            {
                var v = field.GetValue(this) as ComputeBuffer;
                if (v != null)
                {
                    v.Release();
                    v = null;
                }
            }
        }
    }
}
