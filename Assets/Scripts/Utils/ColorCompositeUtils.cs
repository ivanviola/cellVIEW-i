using System;
using UnityEngine;
using System.Collections;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

[Serializable]
public struct DisplayInfo
{
    public int a;
    public int b;
    public int c;
    public int d;

    public DisplayInfo(int _a, int _b, int _c, int _d)
    {
        a = _a;
        b = _b;
        c = _c;
        d = _d;
    }

    public string ToString()
    {
        return "a: " + a + " - b: " + b + " - c: " + c + " - d: " + d;
    }
}



public static class ColorCompositeUtils
{
    public static void ComputeCoverage(RenderTexture instanceIdBuffer)
    {
        ComputeShaderManager.Get.ComputeColorInfo.SetBuffer(3, "_ClearBuffer", GPUBuffers.Get.LipidInstanceVisibilityFlags);
        ComputeShaderManager.Get.ComputeColorInfo.Dispatch(3, Mathf.CeilToInt(SceneManager.Get.NumLipidInstances / 64.0f), 1, 1);

        ComputeShaderManager.Get.ComputeColorInfo.SetBuffer(3, "_ClearBuffer", GPUBuffers.Get.ProteinInstanceVisibilityFlags);
        ComputeShaderManager.Get.ComputeColorInfo.Dispatch(3, Mathf.CeilToInt(SceneManager.Get.NumProteinInstances / 64.0f), 1, 1);
        
        //**************************************************//

        for (var i = 0; i < CPUBuffers.Get.IngredientsDisplayInfo.Count; i++)
        {
            CPUBuffers.Get.IngredientsDisplayInfo[i] = new DisplayInfo(CPUBuffers.Get.IngredientsDisplayInfo[i].a, 0, 0, 0);
        }

        for (var j = 0; j < CPUBuffers.Get.IngredientGroupsDisplayInfo.Count; j++)
        {
            CPUBuffers.Get.IngredientGroupsDisplayInfo[j] = new DisplayInfo(CPUBuffers.Get.IngredientGroupsDisplayInfo[j].a, 0, 0, 0);
        }

        //*************************************************//

        GPUBuffers.Get.IngredientsColorInfo.SetData(CPUBuffers.Get.IngredientsDisplayInfo.ToArray());
        GPUBuffers.Get.IngredientGroupsColorInfo.SetData(CPUBuffers.Get.IngredientGroupsDisplayInfo.ToArray());

        ComputeShaderManager.Get.ComputeColorInfo.SetInt("_Width", instanceIdBuffer.width);
        ComputeShaderManager.Get.ComputeColorInfo.SetInt("_Height", instanceIdBuffer.height);

        ComputeShaderManager.Get.ComputeColorInfo.SetTexture(0, "_InstanceIdBuffer", instanceIdBuffer);
        
        ComputeShaderManager.Get.ComputeColorInfo.SetBuffer(0, "_IngredientsInfo", GPUBuffers.Get.IngredientsInfo);
        ComputeShaderManager.Get.ComputeColorInfo.SetBuffer(0, "_LipidInstancesInfo", GPUBuffers.Get.LipidInstancesInfo);
        ComputeShaderManager.Get.ComputeColorInfo.SetBuffer(0, "_ProteinInstancesInfo", GPUBuffers.Get.ProteinInstancesInfo);

        ComputeShaderManager.Get.ComputeColorInfo.SetBuffer(0, "_RWIngredientsDisplayInfo", GPUBuffers.Get.IngredientsColorInfo);
        ComputeShaderManager.Get.ComputeColorInfo.SetBuffer(0, "_RWIngredientGroupsDisplayInfo", GPUBuffers.Get.IngredientGroupsColorInfo);
        ComputeShaderManager.Get.ComputeColorInfo.SetBuffer(0, "_RWLipidInstancesVisibilityFlags", GPUBuffers.Get.LipidInstanceVisibilityFlags);
        ComputeShaderManager.Get.ComputeColorInfo.SetBuffer(0, "_RWProteinInstancesVisibilityFlags", GPUBuffers.Get.ProteinInstanceVisibilityFlags);

        ComputeShaderManager.Get.ComputeColorInfo.Dispatch(0, Mathf.CeilToInt(instanceIdBuffer.width / 8.0f), Mathf.CeilToInt(instanceIdBuffer.height / 8.0f), 1);
    }

    public static void CountInstances()
    {

        ComputeShaderManager.Get.ComputeColorInfo.SetInt("_NumInstances", SceneManager.Get.NumProteinInstances);
        ComputeShaderManager.Get.ComputeColorInfo.SetBuffer(1, "_IngredientsInfo", GPUBuffers.Get.IngredientsInfo);
        ComputeShaderManager.Get.ComputeColorInfo.SetBuffer(1, "_ProteinInstancesInfo", GPUBuffers.Get.ProteinInstancesInfo);
        
        ComputeShaderManager.Get.ComputeColorInfo.SetBuffer(1, "_RWIngredientsDisplayInfo", GPUBuffers.Get.IngredientsColorInfo);
        ComputeShaderManager.Get.ComputeColorInfo.SetBuffer(1, "_RWIngredientGroupsDisplayInfo", GPUBuffers.Get.IngredientGroupsColorInfo);
        ComputeShaderManager.Get.ComputeColorInfo.SetBuffer(1, "_RWProteinInstancesVisibilityFlags", GPUBuffers.Get.ProteinInstanceVisibilityFlags);

        ComputeShaderManager.Get.ComputeColorInfo.Dispatch(1, Mathf.CeilToInt(SceneManager.Get.NumProteinInstances / 64.0f), 1, 1);

        //***********************//

        ComputeShaderManager.Get.ComputeColorInfo.SetInt("_NumInstances", SceneManager.Get.NumLipidInstances);
        ComputeShaderManager.Get.ComputeColorInfo.SetBuffer(2, "_IngredientsInfo", GPUBuffers.Get.IngredientsInfo);
        ComputeShaderManager.Get.ComputeColorInfo.SetBuffer(2, "_LipidInstancesInfo", GPUBuffers.Get.LipidInstancesInfo);

        ComputeShaderManager.Get.ComputeColorInfo.SetBuffer(2, "_RWIngredientsDisplayInfo", GPUBuffers.Get.IngredientsColorInfo);
        ComputeShaderManager.Get.ComputeColorInfo.SetBuffer(2, "_RWIngredientGroupsDisplayInfo", GPUBuffers.Get.IngredientGroupsColorInfo);
        ComputeShaderManager.Get.ComputeColorInfo.SetBuffer(2, "_RWLipidInstancesVisibilityFlags", GPUBuffers.Get.LipidInstanceVisibilityFlags);

        ComputeShaderManager.Get.ComputeColorInfo.Dispatch(2, Mathf.CeilToInt(SceneManager.Get.NumLipidInstances / 64.0f), 1, 1);
    }

    private static Vector4 currentSelectionSphere = new Vector4();

    public static void ComputeColorComposition(Camera camera, Material colorCompositeMaterial, RenderTexture dst, RenderTexture instanceIdBuffer, RenderTexture atomIdBuffer, RenderTexture depthBuffer)
    {
        //var temp1 = new DisplayInfo[CPUBuffers.Get.IngredientsDisplayInfo.Count];
        //var temp2 = new DisplayInfo[CPUBuffers.Get.IngredientGroupsDisplayInfo.Count];

        //GPUBuffers.Get.IngredientsColorInfo.GetData(temp1);
        //GPUBuffers.Get.IngredientGroupsColorInfo.GetData(temp2);

        //CPUBuffers.Get.IngredientsDisplayInfo = temp1.ToList();
        //CPUBuffers.Get.IngredientGroupsDisplayInfo = temp2.ToList();

        //Debug.Log(temp2[5].ToString());

        /**************/

        //colorCompositeMaterial.SetFloat("_depth", ColorManager.Get.depthSlider);
        colorCompositeMaterial.SetFloat("_UseHCL", Convert.ToInt32(ColorManager.Get.UseHCL));
        colorCompositeMaterial.SetFloat("_ShowAtoms", Convert.ToInt32(ColorManager.Get.ShowAtoms));
        colorCompositeMaterial.SetFloat("_ShowChains", Convert.ToInt32(ColorManager.Get.ShowChains));
        colorCompositeMaterial.SetFloat("_ShowResidues", Convert.ToInt32(ColorManager.Get.ShowResidues));
        //colorCompositeMaterial.SetFloat("_ShowSecondaryStructures", Convert.ToInt32(ColorManager.Get.ShowSecondaryStructures));
        colorCompositeMaterial.SetFloat("_ShowSecondaryStructures", Convert.ToInt32(false));


        colorCompositeMaterial.SetFloat("_AtomDistance", ColorManager.Get.AtomDistance);
        colorCompositeMaterial.SetFloat("_ChainDistance", GlobalProperties.Get.LodLevels[0].x);
        colorCompositeMaterial.SetFloat("_ResidueDistance", ColorManager.Get.ResidueDistance);
        colorCompositeMaterial.SetFloat("_SecondaryStructureDistance", ColorManager.Get.SecondaryStructureDistance);

        //****************//

        var selectionSphere = (Vector4)SelectionManager.Instance.SelectionGameObject.transform.position;
        selectionSphere.w = SelectionManager.Instance.SelectionGameObject.GetComponent<SphereCollider>().radius;


        currentSelectionSphere = Vector4.Lerp(currentSelectionSphere, selectionSphere, 0.05f);

        colorCompositeMaterial.SetVector("_FocusSphere", currentSelectionSphere);
        colorCompositeMaterial.SetMatrix("_ProjectionMatrix", camera.projectionMatrix);
        colorCompositeMaterial.SetMatrix("_InverseViewMatrix", camera.cameraToWorldMatrix);

        //****************//

        // LOD infos
        var rangeValues = Matrix4x4.zero;
        
        int distAcc = 0;
        for (int i = 0; i < ColorManager.Get.LevelRanges.Length; i++)
        {
            distAcc += (int)(ColorManager.Get.LevelRanges[i] * ColorManager.Get.DistanceMax);
            rangeValues[i] = distAcc;
        }
        rangeValues[ColorManager.Get.LevelRanges.Length] = ColorManager.Get.DistanceMax;

        colorCompositeMaterial.SetInt("_UseDistanceLevels", Convert.ToInt32(ColorManager.Get.UseDistanceLevels));
        
        colorCompositeMaterial.SetInt("_NumLevelMax", ColorManager.Get.NumLevelMax);
        colorCompositeMaterial.SetInt("_DistanceMax", ColorManager.Get.DistanceMax);
        colorCompositeMaterial.SetMatrix("_LevelRanges", rangeValues);

        //*****//
        colorCompositeMaterial.SetFloat("_LevelLerpFactor", ColorManager.Get.LevelLerpFactor);
        colorCompositeMaterial.SetInt("_NumPixels", instanceIdBuffer.width * instanceIdBuffer.height);

        //*****//
        colorCompositeMaterial.SetTexture("_DepthBuffer", depthBuffer);
        colorCompositeMaterial.SetTexture("_AtomIdBuffer", atomIdBuffer);
        colorCompositeMaterial.SetTexture("_InstanceIdBuffer", instanceIdBuffer);

        // Properties
        colorCompositeMaterial.SetBuffer("_ProteinAtomInfos", GPUBuffers.Get.ProteinAtomInfo);
        colorCompositeMaterial.SetBuffer("_ProteinAtomInfos2", GPUBuffers.Get.ProteinAtomInfo2);
        colorCompositeMaterial.SetBuffer("_ProteinInstanceInfo", GPUBuffers.Get.ProteinInstancesInfo);
        colorCompositeMaterial.SetBuffer("_LipidAtomInfos", GPUBuffers.Get.LipidAtomPositions);
        colorCompositeMaterial.SetBuffer("_LipidInstancesInfo", GPUBuffers.Get.LipidInstancesInfo);
        colorCompositeMaterial.SetBuffer("_LipidInstancesInfo2", GPUBuffers.Get.LipidInstancesInfo2);

        colorCompositeMaterial.SetBuffer("_IngredientsInfo", GPUBuffers.Get.IngredientsInfo);
        colorCompositeMaterial.SetBuffer("_IngredientGroupsColorInfo", GPUBuffers.Get.IngredientGroupsColorInfo);
        colorCompositeMaterial.SetBuffer("_ProteinIngredientsColorInfo", GPUBuffers.Get.IngredientsColorInfo);

        colorCompositeMaterial.SetFloat("_RnaSelected", NewSelectionManager.Instance.RnaSelected ? 1.0f : 0.0f);

        // Predifined colors 
        colorCompositeMaterial.SetBuffer("_AtomColors", GPUBuffers.Get.AtomColors);
        colorCompositeMaterial.SetBuffer("_AminoAcidColors", GPUBuffers.Get.AminoAcidColors);
        colorCompositeMaterial.SetBuffer("_IngredientsColors", GPUBuffers.Get.IngredientsColors);
        colorCompositeMaterial.SetBuffer("_IngredientsChainColors", GPUBuffers.Get.IngredientsChainColors);
        colorCompositeMaterial.SetBuffer("_IngredientGroupsColor", GPUBuffers.Get.IngredientGroupsColor);

        // Values for color generation on the fly 
        colorCompositeMaterial.SetBuffer("_IngredientGroupsLerpFactors", GPUBuffers.Get.IngredientGroupsLerpFactors);
        colorCompositeMaterial.SetBuffer("_IngredientGroupsColorValues", GPUBuffers.Get.IngredientGroupsColorValues);
        colorCompositeMaterial.SetBuffer("_IngredientGroupsColorRanges", GPUBuffers.Get.IngredientGroupsColorRanges);
        colorCompositeMaterial.SetBuffer("_ProteinIngredientsRandomValues", GPUBuffers.Get.ProteinIngredientsRandomValues);

        // is something selected? if so, I need to dim down everything that isn't is Highlighted state
        int selected = NewSelectionManager.Instance.SomethingIsSelected ? 1 : 0;
        colorCompositeMaterial.SetInt("_SomethingIsSelected", selected);

        Graphics.Blit(null, dst, colorCompositeMaterial, 0);
    }
}
