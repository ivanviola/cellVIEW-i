using System;
using UnityEngine;
using System.Collections;

public static class RenderUtils
{
    public static void DrawProteins(Material renderProteinsMaterial, Camera camera, RenderBuffer colorBuffer, RenderBuffer depthBuffer)
    {
        // Protein params
        renderProteinsMaterial.SetInt("_EnableLod", Convert.ToInt32(GlobalProperties.Get.EnableLod));
        renderProteinsMaterial.SetFloat("_Scale", GlobalProperties.Get.Scale);
        renderProteinsMaterial.SetFloat("_FirstLevelBeingRange", GlobalProperties.Get.FirstLevelOffset);
        renderProteinsMaterial.SetVector("_CameraForward", camera.transform.forward);

        renderProteinsMaterial.SetBuffer("_LodLevelsInfos", GPUBuffers.Get.LodInfo);
        renderProteinsMaterial.SetBuffer("_ProteinInstanceInfo", GPUBuffers.Get.ProteinInstancesInfo);
        renderProteinsMaterial.SetBuffer("_ProteinInstancePositions", GPUBuffers.Get.ProteinInstancePositions);
        renderProteinsMaterial.SetBuffer("_ProteinInstanceRotations", GPUBuffers.Get.ProteinInstanceRotations);

        renderProteinsMaterial.SetBuffer("_ProteinColors", GPUBuffers.Get.IngredientsColors);
        renderProteinsMaterial.SetBuffer("_ProteinAtomPositions", GPUBuffers.Get.ProteinAtoms);
        renderProteinsMaterial.SetBuffer("_ProteinClusterPositions", GPUBuffers.Get.ProteinAtomClusters);
        renderProteinsMaterial.SetBuffer("_ProteinSphereBatchInfos", GPUBuffers.Get.SphereBatches);

        Graphics.SetRenderTarget(colorBuffer, depthBuffer);
        renderProteinsMaterial.SetPass(0);
        Graphics.DrawProceduralIndirect(MeshTopology.Points, GPUBuffers.Get.ArgBuffer);
    }

    public static void DrawClippedProteins(Material renderProteinsMaterial, Camera camera, RenderBuffer colorBuffer, RenderBuffer depthBuffer)
    {
        // Protein params
        renderProteinsMaterial.SetInt("_EnableLod", Convert.ToInt32(GlobalProperties.Get.EnableLod));
        renderProteinsMaterial.SetFloat("_Scale", GlobalProperties.Get.Scale);
        renderProteinsMaterial.SetFloat("_FirstLevelBeingRange", GlobalProperties.Get.FirstLevelOffset);
        renderProteinsMaterial.SetVector("_CameraForward", camera.transform.forward);

        renderProteinsMaterial.SetBuffer("_LodLevelsInfos", GPUBuffers.Get.LodInfo);
        renderProteinsMaterial.SetBuffer("_ProteinInstanceInfo", GPUBuffers.Get.ProteinInstancesInfo);
        renderProteinsMaterial.SetBuffer("_ProteinInstancePositions", GPUBuffers.Get.ProteinInstancePositions);
        renderProteinsMaterial.SetBuffer("_ProteinInstanceRotations", GPUBuffers.Get.ProteinInstanceRotations);

        renderProteinsMaterial.SetBuffer("_ProteinColors", GPUBuffers.Get.IngredientsColors);
        renderProteinsMaterial.SetBuffer("_ProteinAtomPositions", GPUBuffers.Get.ProteinAtoms);
        renderProteinsMaterial.SetBuffer("_ProteinClusterPositions", GPUBuffers.Get.ProteinAtomClusters);
        renderProteinsMaterial.SetBuffer("_ProteinSphereBatchInfos", GPUBuffers.Get.SphereBatches);

        Graphics.SetRenderTarget(colorBuffer, depthBuffer);
        renderProteinsMaterial.SetPass(3);
        Graphics.DrawProceduralIndirect(MeshTopology.Points, GPUBuffers.Get.ArgBuffer);
    }


    public static void DrawProteinsAtoms(Material renderProteinsMaterial, Camera camera, RenderBuffer instanceId, RenderBuffer atomId, RenderBuffer depthBuffer, int pass)
    {
        // Protein params
        renderProteinsMaterial.SetInt("_EnableLod", Convert.ToInt32(GlobalProperties.Get.EnableLod));
        renderProteinsMaterial.SetFloat("_Scale", GlobalProperties.Get.Scale);
        renderProteinsMaterial.SetFloat("_FirstLevelBeingRange", GlobalProperties.Get.FirstLevelOffset);
        renderProteinsMaterial.SetVector("_CameraForward", camera.transform.forward);

        renderProteinsMaterial.SetBuffer("_LodLevelsInfos", GPUBuffers.Get.LodInfo);
        renderProteinsMaterial.SetBuffer("_ProteinInstanceInfo", GPUBuffers.Get.ProteinInstancesInfo);
        renderProteinsMaterial.SetBuffer("_ProteinInstancePositions", GPUBuffers.Get.ProteinInstancePositions);
        renderProteinsMaterial.SetBuffer("_ProteinInstanceRotations", GPUBuffers.Get.ProteinInstanceRotations);

        renderProteinsMaterial.SetBuffer("_ProteinColors", GPUBuffers.Get.IngredientsColors);
        renderProteinsMaterial.SetBuffer("_ProteinAtomInfo", GPUBuffers.Get.ProteinAtomInfo);
        renderProteinsMaterial.SetBuffer("_ProteinAtomPositions", GPUBuffers.Get.ProteinAtoms);
        renderProteinsMaterial.SetBuffer("_ProteinClusterPositions", GPUBuffers.Get.ProteinAtomClusters);
        renderProteinsMaterial.SetBuffer("_ProteinSphereBatchInfos", GPUBuffers.Get.SphereBatches);

        /****/
        renderProteinsMaterial.SetInt("_NumCutObjects", SceneManager.Get.NumCutObjects);
        renderProteinsMaterial.SetInt("_NumIngredientTypes", SceneManager.Get.NumAllIngredients);
        

        renderProteinsMaterial.SetBuffer("_CutInfos", GPUBuffers.Get.CutInfo);
        renderProteinsMaterial.SetBuffer("_CutScales", GPUBuffers.Get.CutScales);
        renderProteinsMaterial.SetBuffer("_CutPositions", GPUBuffers.Get.CutPositions);
        renderProteinsMaterial.SetBuffer("_CutRotations", GPUBuffers.Get.CutRotations);
        /****/

        Graphics.SetRenderTarget(new[] { instanceId, atomId }, depthBuffer);
        renderProteinsMaterial.SetPass(1);
        Graphics.DrawProceduralIndirect(MeshTopology.Points, GPUBuffers.Get.ArgBuffer);
    }

    public static void DrawProteinsShadowMap(Material renderProteinsMaterial, Camera camera, RenderBuffer eyeDepthBuffer, RenderBuffer depthBuffer, int pass)
    {
        // Protein params
        renderProteinsMaterial.SetInt("_EnableLod", Convert.ToInt32(GlobalProperties.Get.EnableLod));
        renderProteinsMaterial.SetFloat("_Scale", GlobalProperties.Get.Scale);
        renderProteinsMaterial.SetFloat("_FirstLevelBeingRange", GlobalProperties.Get.FirstLevelOffset);
        renderProteinsMaterial.SetVector("_CameraForward", camera.transform.forward);

        renderProteinsMaterial.SetBuffer("_LodLevelsInfos", GPUBuffers.Get.LodInfo);
        renderProteinsMaterial.SetBuffer("_ProteinInstanceInfo", GPUBuffers.Get.ProteinInstancesInfo);
        renderProteinsMaterial.SetBuffer("_ProteinInstancePositions", GPUBuffers.Get.ProteinInstancePositions);
        renderProteinsMaterial.SetBuffer("_ProteinInstanceRotations", GPUBuffers.Get.ProteinInstanceRotations);

        renderProteinsMaterial.SetBuffer("_ProteinColors", GPUBuffers.Get.IngredientsColors);
        renderProteinsMaterial.SetBuffer("_ProteinAtomPositions", GPUBuffers.Get.ProteinAtoms);
        renderProteinsMaterial.SetBuffer("_ProteinClusterPositions", GPUBuffers.Get.ProteinAtomClusters);
        renderProteinsMaterial.SetBuffer("_ProteinSphereBatchInfos", GPUBuffers.Get.SphereBatches);

        Graphics.SetRenderTarget(eyeDepthBuffer, depthBuffer);
        renderProteinsMaterial.SetPass(2);
        Graphics.DrawProceduralIndirect(MeshTopology.Points, GPUBuffers.Get.ArgBuffer);
    }

    //public static void DrawProteinSphereBatches(Material renderProteinsMaterial, MainCamera camera, RenderBuffer instanceId, RenderBuffer atomId, RenderBuffer depthBuffer, int pass)
    //{
    //    // Protein params
    //    renderProteinsMaterial.SetInt("_EnableLod", Convert.ToInt32(GlobalProperties.Get.EnableLod));
    //    renderProteinsMaterial.SetFloat("_Scale", GlobalProperties.Get.Scale);
    //    renderProteinsMaterial.SetFloat("_FirstLevelBeingRange", GlobalProperties.Get.FirstLevelOffset);
    //    renderProteinsMaterial.SetVector("_CameraForward", camera.transform.forward);

    //    renderProteinsMaterial.SetBuffer("_LodLevelsInfos", GPUBuffers.Get.LodInfo);
    //    renderProteinsMaterial.SetBuffer("_ProteinInstanceInfo", GPUBuffers.Get.ProteinInstancesInfo);
    //    renderProteinsMaterial.SetBuffer("_ProteinInstancePositions", GPUBuffers.Get.ProteinInstancePositions);
    //    renderProteinsMaterial.SetBuffer("_ProteinInstanceRotations", GPUBuffers.Get.ProteinInstanceRotations);

    //    renderProteinsMaterial.SetBuffer("_ProteinColors", GPUBuffers.Get.IngredientsColors);
    //    renderProteinsMaterial.SetBuffer("_ProteinAtomInfo", GPUBuffers.Get.ProteinAtomInfo);
    //    renderProteinsMaterial.SetBuffer("_ProteinAtomPositions", GPUBuffers.Get.ProteinAtoms);
    //    renderProteinsMaterial.SetBuffer("_ProteinClusterPositions", GPUBuffers.Get.ProteinAtomClusters);
    //    renderProteinsMaterial.SetBuffer("_ProteinSphereBatchInfos", GPUBuffers.Get.SphereBatches);

    //    Graphics.SetRenderTarget(new []{ instanceId , atomId } , depthBuffer);
    //    renderProteinsMaterial.SetPass(pass);
    //    Graphics.DrawProceduralIndirect(MeshTopology.Points, GPUBuffers.Get.ArgBuffer);
    //}

    public static void ComputeSphereBatches(Camera camera)
    {
        if (SceneManager.Get.NumProteinInstances <= 0) return;

        // Always clear append buffer before usage
        GPUBuffers.Get.SphereBatches.ClearAppendBuffer();

        ComputeShaderManager.Get.SphereBatchCS.SetFloat("_Scale", GlobalProperties.Get.Scale);
        ComputeShaderManager.Get.SphereBatchCS.SetInt("_NumLevels", SceneManager.Get.NumLodLevels);
        ComputeShaderManager.Get.SphereBatchCS.SetInt("_NumInstances", SceneManager.Get.NumProteinInstances);
        ComputeShaderManager.Get.SphereBatchCS.SetInt("_EnableLod", Convert.ToInt32(GlobalProperties.Get.EnableLod));
        ComputeShaderManager.Get.SphereBatchCS.SetVector("_CameraForward", camera.transform.forward);
        ComputeShaderManager.Get.SphereBatchCS.SetVector("_CameraPosition", camera.transform.position);
        ComputeShaderManager.Get.SphereBatchCS.SetFloats("_FrustrumPlanes", MyUtility.FrustrumPlanesAsFloats(camera));

        ComputeShaderManager.Get.SphereBatchCS.SetBuffer(0, "_ProteinRadii", GPUBuffers.Get.ProteinRadii);
        ComputeShaderManager.Get.SphereBatchCS.SetBuffer(0, "_ProteinAtomCount", GPUBuffers.Get.ProteinAtomCount);
        ComputeShaderManager.Get.SphereBatchCS.SetBuffer(0, "_ProteinAtomStart", GPUBuffers.Get.ProteinAtomStart);
        ComputeShaderManager.Get.SphereBatchCS.SetBuffer(0, "_ProteinClusterCount", GPUBuffers.Get.ProteinAtomClusterCount);
        ComputeShaderManager.Get.SphereBatchCS.SetBuffer(0, "_ProteinClusterStart", GPUBuffers.Get.ProteinAtomClusterStart);
        ComputeShaderManager.Get.SphereBatchCS.SetBuffer(0, "_LodLevelsInfos", GPUBuffers.Get.LodInfo);
        ComputeShaderManager.Get.SphereBatchCS.SetBuffer(0, "_ProteinInstanceInfo", GPUBuffers.Get.ProteinInstancesInfo);
        ComputeShaderManager.Get.SphereBatchCS.SetBuffer(0, "_ProteinInstancePositions", GPUBuffers.Get.ProteinInstancePositions);
        ComputeShaderManager.Get.SphereBatchCS.SetBuffer(0, "_ProteinInstanceCullFlags", GPUBuffers.Get.ProteinInstanceCullFlags);
        ComputeShaderManager.Get.SphereBatchCS.SetBuffer(0, "_ProteinInstanceVisibilityFlags", GPUBuffers.Get.ProteinInstanceVisibilityFlags);
        ComputeShaderManager.Get.SphereBatchCS.SetBuffer(0, "_ProteinInstanceOcclusionFlags", GPUBuffers.Get.ProteinInstanceOcclusionFlags);
        ComputeShaderManager.Get.SphereBatchCS.SetBuffer(0, "_ProteinSphereBatchInfos", GPUBuffers.Get.SphereBatches);

        ComputeShaderManager.Get.SphereBatchCS.Dispatch(0, Mathf.CeilToInt(SceneManager.Get.NumProteinInstances / 64.0f), 1, 1);
        ComputeBuffer.CopyCount(GPUBuffers.Get.SphereBatches, GPUBuffers.Get.ArgBuffer, 0);
    }

    public static void ComputeSphereBatchesClipped(Camera camera)
    {
        if (SceneManager.Get.NumProteinInstances <= 0) return;

        // Always clear append buffer before usage
        GPUBuffers.Get.SphereBatches.ClearAppendBuffer();

        ComputeShaderManager.Get.SphereBatchCS.SetFloat("_Scale", GlobalProperties.Get.Scale);
        ComputeShaderManager.Get.SphereBatchCS.SetInt("_NumLevels", SceneManager.Get.NumLodLevels);
        ComputeShaderManager.Get.SphereBatchCS.SetInt("_NumInstances", SceneManager.Get.NumProteinInstances);
        ComputeShaderManager.Get.SphereBatchCS.SetInt("_EnableLod", Convert.ToInt32(GlobalProperties.Get.EnableLod));
        ComputeShaderManager.Get.SphereBatchCS.SetVector("_CameraForward", camera.transform.forward);
        ComputeShaderManager.Get.SphereBatchCS.SetVector("_CameraPosition", camera.transform.position);
        ComputeShaderManager.Get.SphereBatchCS.SetFloats("_FrustrumPlanes", MyUtility.FrustrumPlanesAsFloats(camera));

        ComputeShaderManager.Get.SphereBatchCS.SetBuffer(4, "_ProteinRadii", GPUBuffers.Get.ProteinRadii);
        ComputeShaderManager.Get.SphereBatchCS.SetBuffer(4, "_ProteinAtomCount", GPUBuffers.Get.ProteinAtomCount);
        ComputeShaderManager.Get.SphereBatchCS.SetBuffer(4, "_ProteinAtomStart", GPUBuffers.Get.ProteinAtomStart);
        ComputeShaderManager.Get.SphereBatchCS.SetBuffer(4, "_ProteinClusterCount", GPUBuffers.Get.ProteinAtomClusterCount);
        ComputeShaderManager.Get.SphereBatchCS.SetBuffer(4, "_ProteinClusterStart", GPUBuffers.Get.ProteinAtomClusterStart);
        ComputeShaderManager.Get.SphereBatchCS.SetBuffer(4, "_LodLevelsInfos", GPUBuffers.Get.LodInfo);
        ComputeShaderManager.Get.SphereBatchCS.SetBuffer(4, "_ProteinInstanceInfo", GPUBuffers.Get.ProteinInstancesInfo);
        ComputeShaderManager.Get.SphereBatchCS.SetBuffer(4, "_ProteinInstancePositions", GPUBuffers.Get.ProteinInstancePositions);
        ComputeShaderManager.Get.SphereBatchCS.SetBuffer(4, "_ProteinInstanceCullFlags", GPUBuffers.Get.ProteinInstanceCullFlags);
        ComputeShaderManager.Get.SphereBatchCS.SetBuffer(4, "_ProteinInstanceVisibilityFlags", GPUBuffers.Get.ProteinInstanceVisibilityFlags);
        ComputeShaderManager.Get.SphereBatchCS.SetBuffer(4, "_ProteinInstanceOcclusionFlags", GPUBuffers.Get.ProteinInstanceOcclusionFlags);
        ComputeShaderManager.Get.SphereBatchCS.SetBuffer(4, "_ProteinSphereBatchInfos", GPUBuffers.Get.SphereBatches);

        ComputeShaderManager.Get.SphereBatchCS.Dispatch(4, Mathf.CeilToInt(SceneManager.Get.NumProteinInstances / 64.0f), 1, 1);
        ComputeBuffer.CopyCount(GPUBuffers.Get.SphereBatches, GPUBuffers.Get.ArgBuffer, 0);
    }



    public static void ComputeLipidSphereBatches(Camera camera)
    {
        if (SceneManager.Get.NumLipidInstances <= 0) return;

        // Always clear append buffer before usage
        GPUBuffers.Get.SphereBatches.ClearAppendBuffer();

        ComputeShaderManager.Get.SphereBatchCS.SetFloat("_Scale", GlobalProperties.Get.Scale);
        ComputeShaderManager.Get.SphereBatchCS.SetInt("_NumLevels", SceneManager.Get.NumLodLevels);
        ComputeShaderManager.Get.SphereBatchCS.SetInt("_NumInstances", SceneManager.Get.NumLipidInstances);
        ComputeShaderManager.Get.SphereBatchCS.SetInt("_EnableLod", Convert.ToInt32(GlobalProperties.Get.EnableLod));
        ComputeShaderManager.Get.SphereBatchCS.SetVector("_CameraForward", camera.transform.forward);
        ComputeShaderManager.Get.SphereBatchCS.SetVector("_CameraPosition", camera.transform.position);
        ComputeShaderManager.Get.SphereBatchCS.SetFloats("_FrustrumPlanes", MyUtility.FrustrumPlanesAsFloats(camera));

        ComputeShaderManager.Get.SphereBatchCS.SetBuffer(2, "_LipidInstanceInfo", GPUBuffers.Get.LipidInstancesInfo);
        ComputeShaderManager.Get.SphereBatchCS.SetBuffer(2, "_LipidInstancePositions", GPUBuffers.Get.LipidInstancePositions);
        ComputeShaderManager.Get.SphereBatchCS.SetBuffer(2, "_LipidInstanceCullFlags", GPUBuffers.Get.LipidInstanceCullFlags);
        ComputeShaderManager.Get.SphereBatchCS.SetBuffer(2, "_LipidInstanceOcclusionFlags", GPUBuffers.Get.LipidInstanceOcclusionFlags);
        ComputeShaderManager.Get.SphereBatchCS.SetBuffer(2, "_LipidSphereBatches", GPUBuffers.Get.SphereBatches);
        ComputeShaderManager.Get.SphereBatchCS.Dispatch(2, Mathf.CeilToInt(SceneManager.Get.NumLipidInstances / 64.0f), 1, 1);
        ComputeBuffer.CopyCount(GPUBuffers.Get.SphereBatches, GPUBuffers.Get.ArgBuffer, 0);

    }

    public static void DrawLipidSphereBatches(Material RenderLipidsMaterial, RenderTexture colorBuffer, RenderTexture depthBuffer)
    {
        RenderLipidsMaterial.SetFloat("_Scale", GlobalProperties.Get.Scale);
        RenderLipidsMaterial.SetBuffer("_LipidSphereBatches", GPUBuffers.Get.SphereBatches);
        RenderLipidsMaterial.SetBuffer("_LipidAtomPositions", GPUBuffers.Get.LipidAtomPositions);
        //RenderLipidsMaterial.SetBuffer("_LipidInstanceInfos", GPUBuffer.Get.LipidInstanceInfos);
        //RenderLipidsMaterial.SetBuffer("_LipidInstancesInfo2", GPUBuffers.Get.LipidInstancesInfo2);
        RenderLipidsMaterial.SetBuffer("_LipidInstancePositions", GPUBuffers.Get.LipidInstancePositions);
        RenderLipidsMaterial.SetPass(0);

        Graphics.SetRenderTarget(colorBuffer.colorBuffer, depthBuffer.depthBuffer);
        Graphics.DrawProceduralIndirect(MeshTopology.Points, GPUBuffers.Get.ArgBuffer);
    }

    public static void DrawLipidShadows(Material RenderLipidsMaterial, RenderTexture colorBuffer, RenderTexture depthBuffer)
    {
        RenderLipidsMaterial.SetFloat("_Scale", GlobalProperties.Get.Scale);
        RenderLipidsMaterial.SetBuffer("_LipidSphereBatches", GPUBuffers.Get.SphereBatches);
        RenderLipidsMaterial.SetBuffer("_LipidAtomPositions", GPUBuffers.Get.LipidAtomPositions);
        //RenderLipidsMaterial.SetBuffer("_LipidInstanceInfos", GPUBuffer.Get.LipidInstanceInfos);
        RenderLipidsMaterial.SetBuffer("_LipidInstancePositions", GPUBuffers.Get.LipidInstancePositions);
        RenderLipidsMaterial.SetPass(1);

        Graphics.SetRenderTarget(colorBuffer.colorBuffer, depthBuffer.depthBuffer);
        Graphics.DrawProceduralIndirect(MeshTopology.Points, GPUBuffers.Get.ArgBuffer);
    }
}
