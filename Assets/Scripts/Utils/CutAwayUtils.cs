using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class CutAwayUtils
{
    public static void ComputeVisibility(RenderTexture itemBuffer)
    {
        //int _Width;
        //int _Height;

        //// Clear Buffer
        ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(0, "_FlagBuffer", GPUBuffers.Get.ProteinInstanceVisibilityFlags);
        ComputeShaderManager.Get.ComputeVisibilityCS.Dispatch(0, Mathf.CeilToInt(SceneManager.Get.NumProteinInstances / 1), 1, 1);

        ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(0, "_FlagBuffer", GPUBuffers.Get.LipidInstanceVisibilityFlags);
        ComputeShaderManager.Get.ComputeVisibilityCS.Dispatch(0, Mathf.CeilToInt(SceneManager.Get.NumLipidInstances / 1), 1, 1);

        // Compute item visibility
        ComputeShaderManager.Get.ComputeVisibilityCS.SetTexture(1, "_ItemBuffer", itemBuffer);
        ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(1, "_ProteinInstanceVisibilityFlags", GPUBuffers.Get.ProteinInstanceVisibilityFlags);
        ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(1, "_LipidInstanceVisibilityFlags", GPUBuffers.Get.LipidInstanceVisibilityFlags);
        ComputeShaderManager.Get.ComputeVisibilityCS.Dispatch(1, Mathf.CeilToInt(itemBuffer.width / 8.0f), Mathf.CeilToInt(itemBuffer.height / 8.0f), 1);
    }

    public static void FetchHistogramValues()
    {
        // Fetch histograms from GPU
        var histograms = new HistStruct[SceneManager.Get.SceneHierarchy.Count];
        GPUBuffers.Get.Histograms.GetData(histograms);

        // Clear histograms
        ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(2, "_Histograms", GPUBuffers.Get.Histograms);
        ComputeShaderManager.Get.ComputeVisibilityCS.Dispatch(2, Mathf.CeilToInt(CPUBuffers.Get.HistogramData.Count / 64.0f), 1, 1);

        foreach (var histogram in histograms)
        {
            int addWhere = histogram.parent;
            while (addWhere >= 0)
            {
                histograms[addWhere].all += histogram.all;
                histograms[addWhere].cutaway += histogram.cutaway;
                histograms[addWhere].visible += histogram.visible;
                addWhere = histograms[addWhere].parent;
            }
        }

        CPUBuffers.Get.HistogramData = histograms.ToList();
    }

    public static void ComputeProteinObjectSpaceCutAways(Camera _camera, int WeightThreshold)
    {
        if (SceneManager.Get.NumProteinInstances <= 0) return;

        // Cutaways params
        ComputeShaderManager.Get.ObjectSpaceCutAwaysCS.SetInt("_WeightThreshold", WeightThreshold);
        ComputeShaderManager.Get.ObjectSpaceCutAwaysCS.SetBuffer(0, "_IngredientProperties", GPUBuffers.Get.IngredientsInfo);


        ComputeShaderManager.Get.ObjectSpaceCutAwaysCS.SetInt("_NumCutObjects", SceneManager.Get.NumCutObjects);
        ComputeShaderManager.Get.ObjectSpaceCutAwaysCS.SetInt("_NumIngredientTypes", SceneManager.Get.NumAllIngredients);
        ComputeShaderManager.Get.ObjectSpaceCutAwaysCS.SetBuffer(0, "_CutInfos", GPUBuffers.Get.CutInfo);
        ComputeShaderManager.Get.ObjectSpaceCutAwaysCS.SetBuffer(0, "_CutScales", GPUBuffers.Get.CutScales);
        ComputeShaderManager.Get.ObjectSpaceCutAwaysCS.SetBuffer(0, "_CutPositions", GPUBuffers.Get.CutPositions);
        ComputeShaderManager.Get.ObjectSpaceCutAwaysCS.SetBuffer(0, "_CutRotations", GPUBuffers.Get.CutRotations);
        ComputeShaderManager.Get.ObjectSpaceCutAwaysCS.SetBuffer(0, "_Histograms", GPUBuffers.Get.Histograms);
        ComputeShaderManager.Get.ObjectSpaceCutAwaysCS.SetBuffer(0, "_HistogramsLookup", GPUBuffers.Get.HistogramsLookup);

        ComputeShaderManager.Get.ObjectSpaceCutAwaysCS.SetVector("_CameraForward", _camera.transform.forward);

        //ComputeShaderManager.Get.ObjectSpaceCutAwaysCS.SetTexture(0, "noiseTexture", noiseTexture);
        //ComputeShaderManager.Get.ObjectSpaceCutAwaysCS.SetFloat("noiseTextureW", noiseTexture.width);
        //ComputeShaderManager.Get.ObjectSpaceCutAwaysCS.SetFloat("noiseTextureH", noiseTexture.height);

        //ComputeShaderManager.Get.ObjectSpaceCutAwaysCS.SetBuffer(0, "_ProteinCutFilters", GPUBuffer.Get.ProteinCutFilters);
        //ComputeShaderManager.Get.ObjectSpaceCutAwaysCS.SetBuffer(0, "_HistogramStatistics", GPUBuffer.Get.HistogramStatistics);
        //ComputeShaderManager.Get.ObjectSpaceCutAwaysCS.SetBuffer(0, "_HistogramProteinTypes", GPUBuffer.Get.HistogramProteinTypes);

        // Other params
        ComputeShaderManager.Get.ObjectSpaceCutAwaysCS.SetUniform("_Scale", GlobalProperties.Get.Scale);
        ComputeShaderManager.Get.ObjectSpaceCutAwaysCS.SetUniform("_NumInstances", SceneManager.Get.NumProteinInstances);

        ComputeShaderManager.Get.ObjectSpaceCutAwaysCS.SetBuffer(0, "_ProteinRadii", GPUBuffers.Get.ProteinRadii);
        ComputeShaderManager.Get.ObjectSpaceCutAwaysCS.SetBuffer(0, "_ProteinInstanceInfo", GPUBuffers.Get.ProteinInstancesInfo);
        ComputeShaderManager.Get.ObjectSpaceCutAwaysCS.SetBuffer(0, "_ProteinInstancePositions", GPUBuffers.Get.ProteinInstancePositions);

        ComputeShaderManager.Get.ObjectSpaceCutAwaysCS.SetBuffer(0, "_ProteinInstanceCullFlags", GPUBuffers.Get.ProteinInstanceCullFlags);
        ComputeShaderManager.Get.ObjectSpaceCutAwaysCS.SetBuffer(0, "_ProteinInstanceOcclusionFlags", GPUBuffers.Get.ProteinInstanceOcclusionFlags);
        ComputeShaderManager.Get.ObjectSpaceCutAwaysCS.SetBuffer(0, "_ProteinInstanceVisibilityFlags", GPUBuffers.Get.ProteinInstanceVisibilityFlags);

        ComputeShaderManager.Get.ObjectSpaceCutAwaysCS.Dispatch(0, Mathf.CeilToInt(SceneManager.Get.NumProteinInstances / 64.0f), 1, 1);
    }

    public static void ComputeLipidObjectSpaceCutAways()
    {
        if (SceneManager.Get.NumProteinInstances <= 0) return;

        // Cutaways params
        ComputeShaderManager.Get.ObjectSpaceCutAwaysCS.SetInt("_NumCutObjects", SceneManager.Get.NumCutObjects);
        ComputeShaderManager.Get.ObjectSpaceCutAwaysCS.SetInt("_NumIngredientTypes", SceneManager.Get.NumAllIngredients);
        ComputeShaderManager.Get.ObjectSpaceCutAwaysCS.SetBuffer(1, "_CutInfos", GPUBuffers.Get.CutInfo);
        ComputeShaderManager.Get.ObjectSpaceCutAwaysCS.SetBuffer(1, "_CutScales", GPUBuffers.Get.CutScales);
        ComputeShaderManager.Get.ObjectSpaceCutAwaysCS.SetBuffer(1, "_CutPositions", GPUBuffers.Get.CutPositions);
        ComputeShaderManager.Get.ObjectSpaceCutAwaysCS.SetBuffer(1, "_CutRotations", GPUBuffers.Get.CutRotations);
        ComputeShaderManager.Get.ObjectSpaceCutAwaysCS.SetBuffer(1, "_Histograms", GPUBuffers.Get.Histograms);
        ComputeShaderManager.Get.ObjectSpaceCutAwaysCS.SetBuffer(1, "_HistogramsLookup", GPUBuffers.Get.HistogramsLookup);

        //ComputeShaderManager.Get.ObjectSpaceCutAwaysCS.SetTexture(0, "noiseTexture", noiseTexture);
        //ComputeShaderManager.Get.ObjectSpaceCutAwaysCS.SetFloat("noiseTextureW", noiseTexture.width);
        //ComputeShaderManager.Get.ObjectSpaceCutAwaysCS.SetFloat("noiseTextureH", noiseTexture.height);

        //ComputeShaderManager.Get.ObjectSpaceCutAwaysCS.SetBuffer(0, "_ProteinCutFilters", GPUBuffer.Get.ProteinCutFilters);
        //ComputeShaderManager.Get.ObjectSpaceCutAwaysCS.SetBuffer(0, "_HistogramStatistics", GPUBuffer.Get.HistogramStatistics);
        //ComputeShaderManager.Get.ObjectSpaceCutAwaysCS.SetBuffer(0, "_HistogramProteinTypes", GPUBuffer.Get.HistogramProteinTypes);

        // Other params
        ComputeShaderManager.Get.ObjectSpaceCutAwaysCS.SetUniform("_Scale", GlobalProperties.Get.Scale);
        ComputeShaderManager.Get.ObjectSpaceCutAwaysCS.SetUniform("_TypeId", SceneManager.Get.NumProteinInstances);
        ComputeShaderManager.Get.ObjectSpaceCutAwaysCS.SetUniform("_NumInstances", SceneManager.Get.NumLipidInstances);
        ComputeShaderManager.Get.ObjectSpaceCutAwaysCS.SetBuffer(1, "_LipidInstanceInfo", GPUBuffers.Get.LipidInstancesInfo);
        ComputeShaderManager.Get.ObjectSpaceCutAwaysCS.SetBuffer(1, "_LipidInstancePositions", GPUBuffers.Get.LipidInstancePositions);
        ComputeShaderManager.Get.ObjectSpaceCutAwaysCS.SetBuffer(1, "_LipidInstanceCullFlags", GPUBuffers.Get.LipidInstanceCullFlags);
        ComputeShaderManager.Get.ObjectSpaceCutAwaysCS.SetBuffer(1, "_LipidInstanceVisibilityFlags", GPUBuffers.Get.LipidInstanceVisibilityFlags);

        ComputeShaderManager.Get.ObjectSpaceCutAwaysCS.Dispatch(1, Mathf.CeilToInt(SceneManager.Get.NumLipidInstances / 64.0f), 1, 1);
    }

//    static void ComputeDistanceTransform(RenderTexture inputTexture, ref RenderTexture _floodFillTexturePing, ref RenderTexture _floodFillTexturePong)
//    {
//        var tempBuffer = RenderTexture.GetTemporary(512, 512, 32, RenderTextureFormat.ARGB32);
//        Graphics.SetRenderTarget(tempBuffer);
//        Graphics.Blit(inputTexture, tempBuffer);

       

//        float widthScale = inputTexture.width / 512.0f;
//        float heightScale = inputTexture.height / 512.0f;

//        ComputeShaderManager.Get.FloodFillCS.SetInt("_StepSize", 512 / 2);
//        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Mask", tempBuffer);
//        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Ping", _floodFillTexturePing);
//        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Pong", _floodFillTexturePong);

//        ComputeShaderManager.Get.FloodFillCS.SetFloat("_WidthScale", widthScale);
//        ComputeShaderManager.Get.FloodFillCS.SetFloat("_HeightScale", heightScale);
//        ComputeShaderManager.Get.FloodFillCS.Dispatch(0, Mathf.CeilToInt(tempBuffer.width / 8.0f), Mathf.CeilToInt(tempBuffer.height / 8.0f), 1);

//        ComputeShaderManager.Get.FloodFillCS.SetInt("_StepSize", 512 / 4);
//        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Mask", tempBuffer);
//        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Ping", _floodFillTexturePong);
//        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Pong", _floodFillTexturePing);
//        ComputeShaderManager.Get.FloodFillCS.SetFloat("_WidthScale", widthScale);
//        ComputeShaderManager.Get.FloodFillCS.SetFloat("_HeightScale", heightScale);
//        ComputeShaderManager.Get.FloodFillCS.Dispatch(0, Mathf.CeilToInt(tempBuffer.width / 8.0f), Mathf.CeilToInt(tempBuffer.height / 8.0f), 1);

//        ComputeShaderManager.Get.FloodFillCS.SetInt("_StepSize", 512 / 8);
//        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Mask", tempBuffer);
//        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Ping", _floodFillTexturePing);
//        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Pong", _floodFillTexturePong);
//        ComputeShaderManager.Get.FloodFillCS.SetFloat("_WidthScale", widthScale);
//        ComputeShaderManager.Get.FloodFillCS.SetFloat("_HeightScale", heightScale);
//        ComputeShaderManager.Get.FloodFillCS.Dispatch(0, Mathf.CeilToInt(tempBuffer.width / 8.0f), Mathf.CeilToInt(tempBuffer.height / 8.0f), 1);

//        ComputeShaderManager.Get.FloodFillCS.SetInt("_StepSize", 512 / 16);
//        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Mask", tempBuffer);
//        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Ping", _floodFillTexturePong);
//        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Pong", _floodFillTexturePing);
//        ComputeShaderManager.Get.FloodFillCS.SetFloat("_WidthScale", widthScale);
//        ComputeShaderManager.Get.FloodFillCS.SetFloat("_HeightScale", heightScale);
//        ComputeShaderManager.Get.FloodFillCS.Dispatch(0, Mathf.CeilToInt(tempBuffer.width / 8.0f), Mathf.CeilToInt(tempBuffer.height / 8.0f), 1);

//        ComputeShaderManager.Get.FloodFillCS.SetInt("_StepSize", 512 / 32);
//        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Mask", tempBuffer);
//        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Ping", _floodFillTexturePing);
//        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Pong", _floodFillTexturePong);
//        ComputeShaderManager.Get.FloodFillCS.SetFloat("_WidthScale", widthScale);
//        ComputeShaderManager.Get.FloodFillCS.SetFloat("_HeightScale", heightScale);
//        ComputeShaderManager.Get.FloodFillCS.Dispatch(0, Mathf.CeilToInt(tempBuffer.width / 8.0f), Mathf.CeilToInt(tempBuffer.height / 8.0f), 1);

//        ComputeShaderManager.Get.FloodFillCS.SetInt("_StepSize", 512 / 64);
//        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Mask", tempBuffer);
//        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Ping", _floodFillTexturePong);
//        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Pong", _floodFillTexturePing);
//        ComputeShaderManager.Get.FloodFillCS.SetFloat("_WidthScale", widthScale);
//        ComputeShaderManager.Get.FloodFillCS.SetFloat("_HeightScale", heightScale);
//        ComputeShaderManager.Get.FloodFillCS.Dispatch(0, Mathf.CeilToInt(tempBuffer.width / 8.0f), Mathf.CeilToInt(tempBuffer.height / 8.0f), 1);

//        ComputeShaderManager.Get.FloodFillCS.SetInt("_StepSize", 512 / 128);
//        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Mask", tempBuffer);
//        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Ping", _floodFillTexturePing);
//        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Pong", _floodFillTexturePong);
//        ComputeShaderManager.Get.FloodFillCS.SetFloat("_WidthScale", widthScale);
//        ComputeShaderManager.Get.FloodFillCS.SetFloat("_HeightScale", heightScale);
//        ComputeShaderManager.Get.FloodFillCS.Dispatch(0, Mathf.CeilToInt(tempBuffer.width / 8.0f), Mathf.CeilToInt(tempBuffer.height / 8.0f), 1);

//        ComputeShaderManager.Get.FloodFillCS.SetInt("_StepSize", 512 / 256);
//        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Mask", tempBuffer);
//        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Ping", _floodFillTexturePong);
//        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Pong", _floodFillTexturePing);
//        ComputeShaderManager.Get.FloodFillCS.SetFloat("_WidthScale", widthScale);
//        ComputeShaderManager.Get.FloodFillCS.SetFloat("_HeightScale", heightScale);
//        ComputeShaderManager.Get.FloodFillCS.Dispatch(0, Mathf.CeilToInt(tempBuffer.width / 8.0f), Mathf.CeilToInt(tempBuffer.height / 8.0f), 1);

//        ComputeShaderManager.Get.FloodFillCS.SetInt("_StepSize", 512 / 512);
//        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Mask", tempBuffer);
//        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Ping", _floodFillTexturePing);
//        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Pong", _floodFillTexturePong);
//        ComputeShaderManager.Get.FloodFillCS.SetFloat("_WidthScale", widthScale);
//        ComputeShaderManager.Get.FloodFillCS.SetFloat("_HeightScale", heightScale);
//        ComputeShaderManager.Get.FloodFillCS.Dispatch(0, Mathf.CeilToInt(tempBuffer.width / 8.0f), Mathf.CeilToInt(tempBuffer.height / 8.0f), 1);

//        RenderTexture.ReleaseTemporary(tempBuffer);
//    }

//    static void ComputeOcclusionMaskLEqual(Material OcclusionQueriesMaterial, RenderTexture tempBuffer, bool maskProtein, bool maskLipid)
//    {
//        // First clear mask buffer
//        Graphics.SetRenderTarget(tempBuffer);
//        GL.Clear(true, true, Color.blue);

//        //***** Compute Protein Mask *****//
//        if (maskProtein)
//        {
//            // Always clear append buffer before usage
//            GPUBuffers.Get.SphereBatches.ClearAppendBuffer();

//            //Fill the buffer with occludees
//            ComputeShaderManager.Get.SphereBatchCS.SetUniform("_NumInstances", SceneManager.Get.NumProteinInstances);
//            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(1, "_ProteinInstanceInfo", GPUBuffers.Get.ProteinInstancesInfo);
//            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(1, "_ProteinInstanceCullFlags", GPUBuffers.Get.ProteinInstanceCullFlags);

//            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(1, "_IngredientMaskParams", GPUBuffers.Get.IngredientMaskParams);
//            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(1, "_OccludeeSphereBatches", GPUBuffers.Get.SphereBatches);
//            ComputeShaderManager.Get.SphereBatchCS.Dispatch(1, Mathf.CeilToInt(SceneManager.Get.NumProteinInstances / 64.0f), 1, 1);

//            // Count occludees instances
//            ComputeBuffer.CopyCount(GPUBuffers.Get.SphereBatches, GPUBuffers.Get.ArgBuffer, 0);

//            // Prepare draw call
//            OcclusionQueriesMaterial.SetFloat("_Scale", GlobalProperties.Get.Scale);
//            OcclusionQueriesMaterial.SetBuffer("_ProteinRadii", GPUBuffers.Get.ProteinRadii);
//            OcclusionQueriesMaterial.SetBuffer("_ProteinInstanceInfo", GPUBuffers.Get.ProteinInstancesInfo);
//            OcclusionQueriesMaterial.SetBuffer("_ProteinInstancePositions", GPUBuffers.Get.ProteinInstancePositions);
//            OcclusionQueriesMaterial.SetBuffer("_OccludeeSphereBatches", GPUBuffers.Get.SphereBatches);
//            OcclusionQueriesMaterial.SetPass(0);

//            // Draw occludees - bounding sphere only - write to depth and stencil buffer
//            Graphics.SetRenderTarget(tempBuffer);
//            Graphics.DrawProceduralIndirect(MeshTopology.Points, GPUBuffers.Get.ArgBuffer);
//        }

//        //***** Compute Lipid Mask *****//
//        if (maskLipid)
//        {
//            // Always clear append buffer before usage
//            GPUBuffers.Get.SphereBatches.ClearAppendBuffer();

//            //Fill the buffer with occludees
//            ComputeShaderManager.Get.SphereBatchCS.SetUniform("_NumInstances", SceneManager.Get.NumLipidInstances);
//            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(3, "_LipidInstanceInfo", GPUBuffers.Get.LipidInstancesInfo);
//            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(3, "_LipidInstanceCullFlags", GPUBuffers.Get.LipidInstanceCullFlags);

//            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(3, "_IngredientMaskParams", GPUBuffers.Get.IngredientMaskParams);
//            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(3, "_OccludeeSphereBatches", GPUBuffers.Get.SphereBatches);
//            ComputeShaderManager.Get.SphereBatchCS.Dispatch(3, Mathf.CeilToInt(SceneManager.Get.NumLipidInstances / 64.0f), 1, 1);

//            // Count occludees instances
//            ComputeBuffer.CopyCount(GPUBuffers.Get.SphereBatches, GPUBuffers.Get.ArgBuffer, 0);

//            //DebugSphereBatchCount();

//            // Prepare draw call
//            OcclusionQueriesMaterial.SetFloat("_Scale", GlobalProperties.Get.Scale);
//            OcclusionQueriesMaterial.SetBuffer("_LipidInstanceInfo", GPUBuffers.Get.LipidInstancesInfo);
//            OcclusionQueriesMaterial.SetBuffer("_LipidInstancePositions", GPUBuffers.Get.LipidInstancePositions);
//            OcclusionQueriesMaterial.SetBuffer("_OccludeeSphereBatches", GPUBuffers.Get.SphereBatches);
//            OcclusionQueriesMaterial.SetPass(2);

//            // Draw occludees - bounding sphere only - write to depth and stencil buffer
//            Graphics.SetRenderTarget(tempBuffer);
//            Graphics.DrawProceduralIndirect(MeshTopology.Points, GPUBuffers.Get.ArgBuffer);
//        }
//    }

//    //ComputeShaderManager.Get.ObjectSpaceCutAwaysCS.SetBuffer(0, "_CutInfos", GPUBuffers.Get.CutInfo);

//    public static void ComputeOcclusionMaskGEqual(Material OcclusionQueriesMaterial, RenderTexture tempBuffer, bool maskProtein, bool maskLipid)
//    {
//        // First clear mask buffer
//        Graphics.SetRenderTarget(tempBuffer);
//        GL.Clear(true, true, Color.blue, 0);

//        //***** Compute Protein Mask *****//
//        if (maskProtein)
//        {
//            // Always clear append buffer before usage
//            GPUBuffers.Get.SphereBatches.ClearAppendBuffer();

//            //Fill the buffer with occludees
//            ComputeShaderManager.Get.SphereBatchCS.SetUniform("_NumInstances", SceneManager.Get.NumProteinInstances);
//            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(1, "_ProteinInstanceInfo", GPUBuffers.Get.ProteinInstancesInfo);
//            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(1, "_ProteinInstanceCullFlags", GPUBuffers.Get.ProteinInstanceCullFlags);

//            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(1, "_IngredientMaskParams", GPUBuffers.Get.IngredientMaskParams);
//            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(1, "_OccludeeSphereBatches", GPUBuffers.Get.SphereBatches);
//            ComputeShaderManager.Get.SphereBatchCS.Dispatch(1, Mathf.CeilToInt(SceneManager.Get.NumProteinInstances / 64.0f), 1, 1);

//            // Count occludees instances
//            ComputeBuffer.CopyCount(GPUBuffers.Get.SphereBatches, GPUBuffers.Get.ArgBuffer, 0);

//            // Prepare draw call
//            OcclusionQueriesMaterial.SetFloat("_Scale", GlobalProperties.Get.Scale);
//            OcclusionQueriesMaterial.SetBuffer("_ProteinRadii", GPUBuffers.Get.ProteinRadii);
//            OcclusionQueriesMaterial.SetBuffer("_ProteinInstanceInfo", GPUBuffers.Get.ProteinInstancesInfo);
//            OcclusionQueriesMaterial.SetBuffer("_ProteinInstancePositions", GPUBuffers.Get.ProteinInstancePositions);
//            OcclusionQueriesMaterial.SetBuffer("_OccludeeSphereBatches", GPUBuffers.Get.SphereBatches);
//            OcclusionQueriesMaterial.SetPass(4);

//            // Draw occludees - bounding sphere only - write to depth and stencil buffer
//            Graphics.SetRenderTarget(tempBuffer);
//            Graphics.DrawProceduralIndirect(MeshTopology.Points, GPUBuffers.Get.ArgBuffer);
//        }

//        //***** Compute Lipid Mask *****//
//        if (maskLipid)
//        {
//            // Always clear append buffer before usage
//            GPUBuffers.Get.SphereBatches.ClearAppendBuffer();

//            //Fill the buffer with occludees
//            ComputeShaderManager.Get.SphereBatchCS.SetUniform("_NumInstances", SceneManager.Get.NumLipidInstances);
//            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(3, "_LipidInstanceInfo", GPUBuffers.Get.LipidInstancesInfo);
//            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(3, "_LipidInstanceCullFlags", GPUBuffers.Get.LipidInstanceCullFlags);

//            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(3, "_IngredientMaskParams", GPUBuffers.Get.IngredientMaskParams);
//            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(3, "_OccludeeSphereBatches", GPUBuffers.Get.SphereBatches);
//            ComputeShaderManager.Get.SphereBatchCS.Dispatch(3, Mathf.CeilToInt(SceneManager.Get.NumLipidInstances / 64.0f), 1, 1);

//            // Count occludees instances
//            ComputeBuffer.CopyCount(GPUBuffers.Get.SphereBatches, GPUBuffers.Get.ArgBuffer, 0);

//            //DebugSphereBatchCount();

//            // Prepare draw call
//            OcclusionQueriesMaterial.SetFloat("_Scale", GlobalProperties.Get.Scale);
//            OcclusionQueriesMaterial.SetBuffer("_LipidInstanceInfo", GPUBuffers.Get.LipidInstancesInfo);
//            OcclusionQueriesMaterial.SetBuffer("_LipidInstancePositions", GPUBuffers.Get.LipidInstancePositions);
//            OcclusionQueriesMaterial.SetBuffer("_OccludeeSphereBatches", GPUBuffers.Get.SphereBatches);
//            OcclusionQueriesMaterial.SetPass(5);

//            // Draw occludees - bounding sphere only - write to depth and stencil buffer
//            Graphics.SetRenderTarget(tempBuffer);
//            Graphics.DrawProceduralIndirect(MeshTopology.Points, GPUBuffers.Get.ArgBuffer);
//        }
//    }

//    public static void ComputeOcclusionQueries(RenderTexture _floodFillTexturePong, Material OcclusionQueriesMaterial, RenderTexture tempBuffer, CutObject cutObject, int cutObjectIndex, int internalState, bool cullProtein, bool cullLipid)
//    {
//        if (cullProtein)
//        {
//            // Always clear append buffer before usage
//            GPUBuffers.Get.SphereBatches.ClearAppendBuffer();

//            //Fill the buffer with occluders
//            ComputeShaderManager.Get.SphereBatchCS.SetUniform("_NumInstances", SceneManager.Get.NumProteinInstances);
//            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(1, "_ProteinInstanceInfo", GPUBuffers.Get.ProteinInstancesInfo);
//            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(1, "_ProteinInstanceCullFlags", GPUBuffers.Get.ProteinInstanceCullFlags);

//            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(1, "_IngredientMaskParams", GPUBuffers.Get.IngredientMaskParams);
//            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(1, "_OccludeeSphereBatches", GPUBuffers.Get.SphereBatches);
//            ComputeShaderManager.Get.SphereBatchCS.Dispatch(1, Mathf.CeilToInt(SceneManager.Get.NumProteinInstances / 64.0f), 1, 1);

//            // Count occluder instances
//            ComputeBuffer.CopyCount(GPUBuffers.Get.SphereBatches, GPUBuffers.Get.ArgBuffer, 0);

//            //DebugSphereBatchCount();

//            // Clear protein occlusion buffer 
//            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(0, "_FlagBuffer", GPUBuffers.Get.ProteinInstanceOcclusionFlags);
//            ComputeShaderManager.Get.ComputeVisibilityCS.Dispatch(0, Mathf.CeilToInt(SceneManager.Get.NumProteinInstances / 64.0f), 1, 1);

//            // Bind the read/write occlusion buffer to the shader
//            // After this draw call the occlusion buffer will be filled with ones if an instance occluded and occludee, zero otherwise
//            Graphics.SetRandomWriteTarget(1, GPUBuffers.Get.ProteinInstanceOcclusionFlags);
//            MyUtility.DummyBlit();   // Dunny why yet, but without this I cannot write to the buffer from the shader, go figure

//            // Set the render target
//            Graphics.SetRenderTarget(tempBuffer);

//            OcclusionQueriesMaterial.SetInt("_CutObjectIndex", cutObjectIndex);
//            OcclusionQueriesMaterial.SetInt("_NumIngredients", SceneManager.Get.NumAllIngredients);
//            OcclusionQueriesMaterial.SetBuffer("_CutInfo", GPUBuffers.Get.CutInfo);
//            OcclusionQueriesMaterial.SetTexture("_DistanceField", _floodFillTexturePong);

//            OcclusionQueriesMaterial.SetFloat("_Scale", GlobalProperties.Get.Scale);
//            OcclusionQueriesMaterial.SetBuffer("_ProteinRadii", GPUBuffers.Get.ProteinRadii);
//            OcclusionQueriesMaterial.SetBuffer("_ProteinInstanceInfo", GPUBuffers.Get.ProteinInstancesInfo);
//            OcclusionQueriesMaterial.SetBuffer("_ProteinInstancePositions", GPUBuffers.Get.ProteinInstancePositions);
//            OcclusionQueriesMaterial.SetBuffer("_OccludeeSphereBatches", GPUBuffers.Get.SphereBatches);
//            OcclusionQueriesMaterial.SetPass(1);

//            // Issue draw call for occluders - bounding quads only - depth/stencil test enabled - no write to color/depth/stencil
//            Graphics.DrawProceduralIndirect(MeshTopology.Points, GPUBuffers.Get.ArgBuffer);
//            Graphics.ClearRandomWriteTargets();

//            ComputeShaderManager.Get.ComputeVisibilityCS.SetUniform("_CutObjectIndex", cutObjectIndex);
//            ComputeShaderManager.Get.ComputeVisibilityCS.SetUniform("_NumIngredients", SceneManager.Get.NumAllIngredients);
//            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(3, "_CutInfo", GPUBuffers.Get.CutInfo);

//            //// Discard occluding instances according to value2
//            ComputeShaderManager.Get.ComputeVisibilityCS.SetUniform("_CutObjectId", cutObject.Id);
//            ComputeShaderManager.Get.ComputeVisibilityCS.SetUniform("_ConsumeRestoreState", internalState);
//            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(3, "_Histograms", GPUBuffers.Get.Histograms);
//            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(3, "_HistogramsLookup", GPUBuffers.Get.HistogramsLookup);
//            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(3, "_ProteinInstanceInfo", GPUBuffers.Get.ProteinInstancesInfo);
//            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(3, "_ProteinInstanceCullFlags", GPUBuffers.Get.ProteinInstanceCullFlags);
//            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(3, "_ProteinInstanceOcclusionFlags", GPUBuffers.Get.ProteinInstanceOcclusionFlags);
//            ComputeShaderManager.Get.ComputeVisibilityCS.Dispatch(3, Mathf.CeilToInt(SceneManager.Get.NumProteinInstances / 64.0f), 1, 1);
//        }

//        if (cullLipid)
//        {
//            // Always clear append buffer before usage
//            GPUBuffers.Get.SphereBatches.ClearAppendBuffer();

//            //Fill the buffer with occluders
//            ComputeShaderManager.Get.SphereBatchCS.SetUniform("_NumInstances", SceneManager.Get.NumLipidInstances);
//            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(3, "_LipidInstanceInfo", GPUBuffers.Get.LipidInstancesInfo);
//            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(3, "_LipidInstanceCullFlags", GPUBuffers.Get.LipidInstanceCullFlags);

//            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(3, "_IngredientMaskParams", GPUBuffers.Get.IngredientMaskParams);
//            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(3, "_OccludeeSphereBatches", GPUBuffers.Get.SphereBatches);
//            ComputeShaderManager.Get.SphereBatchCS.Dispatch(3, Mathf.CeilToInt(SceneManager.Get.NumLipidInstances / 64.0f), 1, 1);

//            // Count occluder instances
//            ComputeBuffer.CopyCount(GPUBuffers.Get.SphereBatches, GPUBuffers.Get.ArgBuffer, 0);

//            // Clear lipid occlusion buffer 
//            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(0, "_FlagBuffer", GPUBuffers.Get.LipidInstanceOcclusionFlags);
//            ComputeShaderManager.Get.ComputeVisibilityCS.Dispatch(0, Mathf.CeilToInt(SceneManager.Get.NumLipidInstances / 64.0f), 1, 1);

//            // Bind the read/write occlusion buffer to the shader
//            // After this draw call the occlusion buffer will be filled with ones if an instance occluded and occludee, zero otherwise
//            Graphics.SetRandomWriteTarget(1, GPUBuffers.Get.LipidInstanceOcclusionFlags);
//            MyUtility.DummyBlit();   // Dunny why yet, but without this I cannot write to the buffer from the shader, go figure

//            // Set the render target
//            Graphics.SetRenderTarget(tempBuffer);

//            OcclusionQueriesMaterial.SetInt("_CutObjectIndex", cutObjectIndex);
//            OcclusionQueriesMaterial.SetInt("_NumIngredients", SceneManager.Get.NumAllIngredients);
//            OcclusionQueriesMaterial.SetBuffer("_CutInfo", GPUBuffers.Get.CutInfo);
//            OcclusionQueriesMaterial.SetTexture("_DistanceField", _floodFillTexturePong);

//            OcclusionQueriesMaterial.SetFloat("_Scale", GlobalProperties.Get.Scale);
//            OcclusionQueriesMaterial.SetBuffer("_LipidInstanceInfo", GPUBuffers.Get.LipidInstancesInfo);
//            OcclusionQueriesMaterial.SetBuffer("_LipidInstancePositions", GPUBuffers.Get.LipidInstancePositions);
//            OcclusionQueriesMaterial.SetBuffer("_OccludeeSphereBatches", GPUBuffers.Get.SphereBatches);
//            OcclusionQueriesMaterial.SetPass(3);

//            // Issue draw call for occluders - bounding quads only - depth/stencil test enabled - no write to color/depth/stencil
//            Graphics.DrawProceduralIndirect(MeshTopology.Points, GPUBuffers.Get.ArgBuffer);
//            Graphics.ClearRandomWriteTargets();

//            ComputeShaderManager.Get.ComputeVisibilityCS.SetUniform("_CutObjectIndex", cutObjectIndex);
//            ComputeShaderManager.Get.ComputeVisibilityCS.SetUniform("_NumIngredients", SceneManager.Get.NumAllIngredients);
//            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(4, "_CutInfo", GPUBuffers.Get.CutInfo);

//            //// Discard occluding instances according to value2
//            ComputeShaderManager.Get.ComputeVisibilityCS.SetUniform("_CutObjectId", cutObject.Id);
//            ComputeShaderManager.Get.ComputeVisibilityCS.SetUniform("_ConsumeRestoreState", internalState);
//            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(4, "_Histograms", GPUBuffers.Get.Histograms);
//            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(4, "_HistogramsLookup", GPUBuffers.Get.HistogramsLookup);
//            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(4, "_LipidInstanceInfo", GPUBuffers.Get.LipidInstancesInfo);
//            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(4, "_LipidInstanceCullFlags", GPUBuffers.Get.LipidInstanceCullFlags);
//            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(4, "_LipidInstanceOcclusionFlags", GPUBuffers.Get.LipidInstanceOcclusionFlags);
//            ComputeShaderManager.Get.ComputeVisibilityCS.Dispatch(4, Mathf.CeilToInt(SceneManager.Get.NumLipidInstances / 64.0f), 1, 1);
//        }
//    }

//    //[ImageEffectOpaque]
//    //void OnRenderImage(RenderTexture src, RenderTexture dst)
//    public static void ComputeViewSpaceCutAways(Camera camera, Material occlusionQueryMaterial)
//    {
//        //ComputeProteinObjectSpaceCutAways();
//        //ComputeLipidObjectSpaceCutAways();

//        // Prepare and set the render target
//        var tempBuffer = RenderTexture.GetTemporary(camera.pixelWidth, camera.pixelHeight, 32, RenderTextureFormat.ARGB32);
//        //var tempBuffer = RenderTexture.GetTemporary(512, 512, 32, RenderTextureFormat.ARGB32);

//        var resetCutSnapshot = CutObjectManager.Get.ResetCutSnapshot;
//        CutObjectManager.Get.ResetCutSnapshot = -1;

//        if (resetCutSnapshot > 0)
//        {
//            // Discard occluding instances according to value2
//            ComputeShaderManager.Get.ComputeVisibilityCS.SetUniform("_CutObjectId", resetCutSnapshot);
//            ComputeShaderManager.Get.ComputeVisibilityCS.SetUniform("_ConsumeRestoreState", 2);
//            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(3, "_Histograms", GPUBuffers.Get.Histograms);
//            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(3, "_HistogramsLookup", GPUBuffers.Get.HistogramsLookup);
//            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(3, "_IngredientMaskParams", GPUBuffers.Get.IngredientMaskParams);
//            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(3, "_ProteinInstanceInfo", GPUBuffers.Get.ProteinInstancesInfo);
//            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(3, "_ProteinInstanceCullFlags", GPUBuffers.Get.ProteinInstanceCullFlags);
//            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(3, "_ProteinInstanceOcclusionFlags", GPUBuffers.Get.ProteinInstanceOcclusionFlags);
//            ComputeShaderManager.Get.ComputeVisibilityCS.Dispatch(3, Mathf.CeilToInt(SceneManager.Get.NumProteinInstances / 64.0f), 1, 1);

//            //// Discard occluding instances according to value2
//            ComputeShaderManager.Get.ComputeVisibilityCS.SetUniform("_CutObjectId", resetCutSnapshot);
//            ComputeShaderManager.Get.ComputeVisibilityCS.SetUniform("_ConsumeRestoreState", 2);
//            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(4, "_Histograms", GPUBuffers.Get.Histograms);
//            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(4, "_HistogramsLookup", GPUBuffers.Get.HistogramsLookup);
//            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(4, "_IngredientMaskParams", GPUBuffers.Get.IngredientMaskParams);
//            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(4, "_LipidInstanceInfo", GPUBuffers.Get.LipidInstancesInfo);
//            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(4, "_LipidInstanceCullFlags", GPUBuffers.Get.LipidInstanceCullFlags);
//            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(4, "_LipidInstanceOcclusionFlags", GPUBuffers.Get.LipidInstanceOcclusionFlags);
//            ComputeShaderManager.Get.ComputeVisibilityCS.Dispatch(4, Mathf.CeilToInt(SceneManager.Get.NumLipidInstances / 64.0f), 1, 1);
//        }

//        var cutObjectId = -1;

//        var _floodFillTexturePing = new RenderTexture(tempBuffer.width, tempBuffer.height, 32, RenderTextureFormat.ARGBFloat);
//        _floodFillTexturePing.enableRandomWrite = true;
//        _floodFillTexturePing.filterMode = FilterMode.Point;

//        var _floodFillTexturePong = new RenderTexture(tempBuffer.width, tempBuffer.height, 32, RenderTextureFormat.ARGBFloat);
//        _floodFillTexturePong.enableRandomWrite = true;
//        _floodFillTexturePong.filterMode = FilterMode.Point;

//        foreach (var cutObject in CutObjectManager.Get.CutObjects)
//        {
//            cutObjectId++;

//            //********************************************************//

//            var internalState = 0;
//            //Debug.Log(cutObject.CurrentLockState.ToString());

//            if (cutObject.CurrentLockState == LockState.Restore)
//            {
//                Debug.Log("We restore");
//                internalState = 2;
//                cutObject.CurrentLockState = LockState.Unlocked;
//            }

//            if (cutObject.CurrentLockState == LockState.Consumed)
//            {
//                continue;
//            }

//            if (cutObject.CurrentLockState == LockState.Locked)
//            {
//                Debug.Log("We consume");
//                internalState = 1;
//                cutObject.CurrentLockState = LockState.Consumed;
//            }

//            //********************************************************//

//            var maskLipid = false;
//            var cullLipid = false;

//            var maskProtein = false;
//            var cullProtein = false;

//            //Fill the buffer with occludees mask falgs
//            var maskFlags = new List<int>();
//            var cullFlags = new List<int>();

//            foreach (var cutParam in cutObject.IngredientCutParameters)
//            {
//                var isMask = cutParam.IsFocus;
//                var isCulled = !cutParam.IsFocus && (cutParam.Aperture > 0 || cutParam.value2 < 1);

//                if (cutParam.IsFocus)
//                {
//                    if (cutParam.Id < SceneManager.Get.NumProteinIngredients) maskProtein = true;
//                    else maskLipid = true;
//                }

//                if (isCulled)
//                {
//                    if (cutParam.Id < SceneManager.Get.NumProteinIngredients) cullProtein = true;
//                    else cullLipid = true;
//                }

//                maskFlags.Add(isMask ? 1 : 0);
//                cullFlags.Add(isCulled ? 1 : 0);
//            }

//            //if (!cullProtein && !cullLipid) continue;

//            //********************************************************//

//            //***** Compute Depth-Stencil mask *****//

//            // Upload Occludees flags to GPU
//            GPUBuffers.Get.IngredientMaskParams.SetData(maskFlags.ToArray());
//            ComputeOcclusionMaskLEqual(occlusionQueryMaterial, tempBuffer, maskProtein, maskLipid);
//            //ComputeOcclusionMaskGEqual(tempBuffer, maskProtein, maskLipid);
//            //Graphics.Blit(tempBuffer, dst);
//            //break;
                        
//            Graphics.SetRenderTarget(_floodFillTexturePing);
//            GL.Clear(true, true, new Color(-1, -1, -1, -1));
            
//            Graphics.SetRenderTarget(_floodFillTexturePong);
//            GL.Clear(true, true, new Color(-1, -1, -1, -1));

//            ComputeDistanceTransform(tempBuffer, ref _floodFillTexturePing, ref _floodFillTexturePong);

//            //Graphics.Blit(_floodFillTexturePong, dst, CompositeMaterial, 4);
//            //break;

//            /////**** Compute Queries ***//

//            // Upload Occluders flags to GPU
//            GPUBuffers.Get.IngredientMaskParams.SetData(cullFlags.ToArray());
//            ComputeOcclusionQueries(_floodFillTexturePong, occlusionQueryMaterial, tempBuffer, cutObject, cutObjectId, internalState, cullProtein, cullLipid);
//        }

//        // Release render target
//        _floodFillTexturePing.Release();
//        _floodFillTexturePong.Release();
//        RenderTexture.ReleaseTemporary(tempBuffer);

//    }
}
