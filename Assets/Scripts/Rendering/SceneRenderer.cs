using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class SceneRenderer : MonoBehaviour
{
    public Camera LightCamera;
    public RenderShadowMap RenderShadowMap;
    public LightCameraController LightCameraController;

    public Material ContourMaterial;
    public Material CompositeMaterial;
    public Material ColorCompositeMaterial;
    public Material OcclusionQueriesMaterial;

    public Material RenderLipidsMaterial;
    public Material RenderProteinsMaterial;
    public Material RenderCurveIngredientsMaterial;

    public Material RecBorderMaterial;

    /*****/

    private Camera _camera;
    private RenderTexture _floodFillTexturePing;
    private RenderTexture _floodFillTexturePong;

    /*****/

    public int WeightThreshold;

    public bool EnableGhosts;

    [Range(-1, 1)]
    public float GhostContours;

    [Range(0, 10)]
    public float myslider = 0;

    /*****/

    void OnEnable()
    {
        _camera = GetComponent<Camera>();
        _camera.depthTextureMode |= DepthTextureMode.Depth;
        _camera.depthTextureMode |= DepthTextureMode.DepthNormals;
    }

    void OnDisable()
    {
        if (_floodFillTexturePing != null)
        {
            _floodFillTexturePing.DiscardContents();
            DestroyImmediate(_floodFillTexturePing);
        }

        if (_floodFillTexturePong != null)
        {
            _floodFillTexturePong.DiscardContents();
            DestroyImmediate(_floodFillTexturePong);
        }
    }

 

    void SetContourShaderParams()
    {
        // Contour params
        ContourMaterial.SetInt("_ContourOptions", GlobalProperties.Get.ContourOptions);
        ContourMaterial.SetFloat("_ContourStrength", GlobalProperties.Get.ContourStrength);
    }

    void DebugSphereBatchCount()
    {
        var batchCount = new int[1];
        GPUBuffers.Get.ArgBuffer.GetData(batchCount);
        Debug.Log(batchCount[0]);
    }


    void DrawCurveIngredients(RenderTexture colorBuffer, RenderTexture idBuffer, RenderTexture depthBuffer)
    {
        RenderCurveIngredientsMaterial.SetInt("_IngredientIdOffset", SceneManager.Get.NumProteinIngredients + SceneManager.Get.NumLipidIngredients);
        RenderCurveIngredientsMaterial.SetInt("_NumCutObjects", SceneManager.Get.NumCutObjects);
        RenderCurveIngredientsMaterial.SetInt("_NumIngredientTypes", SceneManager.Get.NumAllIngredients);
        RenderCurveIngredientsMaterial.SetBuffer("_CutInfos", GPUBuffers.Get.CutInfo);
        RenderCurveIngredientsMaterial.SetBuffer("_CutScales", GPUBuffers.Get.CutScales);
        RenderCurveIngredientsMaterial.SetBuffer("_CutPositions", GPUBuffers.Get.CutPositions);
        RenderCurveIngredientsMaterial.SetBuffer("_CutRotations", GPUBuffers.Get.CutRotations);

        var planes = GeometryUtility.CalculateFrustumPlanes(GetComponent<Camera>());
        RenderCurveIngredientsMaterial.SetVector("_FrustrumPlane_0", MyUtility.PlaneToVector4(planes[0]));
        RenderCurveIngredientsMaterial.SetVector("_FrustrumPlane_1", MyUtility.PlaneToVector4(planes[1]));
        RenderCurveIngredientsMaterial.SetVector("_FrustrumPlane_2", MyUtility.PlaneToVector4(planes[2]));
        RenderCurveIngredientsMaterial.SetVector("_FrustrumPlane_3", MyUtility.PlaneToVector4(planes[3]));
        RenderCurveIngredientsMaterial.SetVector("_FrustrumPlane_4", MyUtility.PlaneToVector4(planes[4]));
        RenderCurveIngredientsMaterial.SetVector("_FrustrumPlane_5", MyUtility.PlaneToVector4(planes[5]));

        RenderCurveIngredientsMaterial.SetInt("_NumSegments", SceneManager.Get.NumDnaControlPoints);
        RenderCurveIngredientsMaterial.SetInt("_EnableTwist", 1);

        RenderCurveIngredientsMaterial.SetFloat("_Scale", GlobalProperties.Get.Scale);
        RenderCurveIngredientsMaterial.SetFloat("_SegmentLength", GlobalProperties.Get.DistanceContraint);
        //RenderCurveIngredientsMaterial.SetInt("_EnableCrossSection", Convert.ToInt32(GlobalProperties.Get.EnableCrossSection));
        //RenderCurveIngredientsMaterial.SetVector("_CrossSectionPlane", new Vector4(GlobalProperties.Get.CrossSectionPlaneNormal.x, GlobalProperties.Get.CrossSectionPlaneNormal.y, GlobalProperties.Get.CrossSectionPlaneNormal.z, GlobalProperties.Get.CrossSectionPlaneDistance));

        RenderCurveIngredientsMaterial.SetBuffer("_CurveIngredientsInfos", GPUBuffers.Get.CurveIngredientsInfo);
        RenderCurveIngredientsMaterial.SetBuffer("_CurveIngredientsColors", GPUBuffers.Get.CurveIngredientsColors);
        RenderCurveIngredientsMaterial.SetBuffer("_CurveIngredientsToggleFlags", GPUBuffers.Get.CurveIngredientsToggleFlags);
        RenderCurveIngredientsMaterial.SetBuffer("_CurveIngredientsAtoms", GPUBuffers.Get.CurveIngredientsAtoms);
        RenderCurveIngredientsMaterial.SetBuffer("_CurveIngredientsAtomCount", GPUBuffers.Get.CurveIngredientsAtomCount);
        RenderCurveIngredientsMaterial.SetBuffer("_CurveIngredientsAtomStart", GPUBuffers.Get.CurveIngredientsAtomStart);

        RenderCurveIngredientsMaterial.SetBuffer("_DnaControlPointsInfos", GPUBuffers.Get.CurveControlPointsInfo);
        RenderCurveIngredientsMaterial.SetBuffer("_DnaControlPointsNormals", GPUBuffers.Get.CurveControlPointsNormals);
        RenderCurveIngredientsMaterial.SetBuffer("_DnaControlPoints", GPUBuffers.Get.CurveControlPointsPositions);

        Graphics.SetRenderTarget(new[] { colorBuffer.colorBuffer, idBuffer.colorBuffer }, depthBuffer.depthBuffer);
        RenderCurveIngredientsMaterial.SetPass(0);
        Graphics.DrawProcedural(MeshTopology.Points, Mathf.Max(SceneManager.Get.NumDnaSegments - 2, 0)); // Do not draw first and last segments
    }

    void ComputeDistanceTransform(RenderTexture inputTexture)
    {
        var tempBuffer = RenderTexture.GetTemporary(512, 512, 32, RenderTextureFormat.ARGB32);
        Graphics.SetRenderTarget(tempBuffer);
        Graphics.Blit(inputTexture, tempBuffer);

        // Prepare and set the render target
        if (_floodFillTexturePing == null)
        {
            _floodFillTexturePing = new RenderTexture(tempBuffer.width, tempBuffer.height, 32, RenderTextureFormat.ARGBFloat);
            _floodFillTexturePing.enableRandomWrite = true;
            _floodFillTexturePing.filterMode = FilterMode.Point;
        }

        Graphics.SetRenderTarget(_floodFillTexturePing);
        GL.Clear(true, true, new Color(-1, -1, -1, -1));

        if (_floodFillTexturePong == null)
        {
            _floodFillTexturePong = new RenderTexture(tempBuffer.width, tempBuffer.height, 32, RenderTextureFormat.ARGBFloat);
            _floodFillTexturePong.enableRandomWrite = true;
            _floodFillTexturePong.filterMode = FilterMode.Point;
        }

        Graphics.SetRenderTarget(_floodFillTexturePong);
        GL.Clear(true, true, new Color(-1, -1, -1, -1));

        float widthScale = inputTexture.width / 512.0f;
        float heightScale = inputTexture.height / 512.0f;

        ComputeShaderManager.Get.FloodFillCS.SetInt("_StepSize", 512 / 2);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Mask", tempBuffer);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Ping", _floodFillTexturePing);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Pong", _floodFillTexturePong);

        ComputeShaderManager.Get.FloodFillCS.SetFloat("_WidthScale", widthScale);
        ComputeShaderManager.Get.FloodFillCS.SetFloat("_HeightScale", heightScale);
        ComputeShaderManager.Get.FloodFillCS.Dispatch(0, Mathf.CeilToInt(tempBuffer.width / 8.0f), Mathf.CeilToInt(tempBuffer.height / 8.0f), 1);

        ComputeShaderManager.Get.FloodFillCS.SetInt("_StepSize", 512 / 4);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Mask", tempBuffer);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Ping", _floodFillTexturePong);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Pong", _floodFillTexturePing);
        ComputeShaderManager.Get.FloodFillCS.SetFloat("_WidthScale", widthScale);
        ComputeShaderManager.Get.FloodFillCS.SetFloat("_HeightScale", heightScale);
        ComputeShaderManager.Get.FloodFillCS.Dispatch(0, Mathf.CeilToInt(tempBuffer.width / 8.0f), Mathf.CeilToInt(tempBuffer.height / 8.0f), 1);

        ComputeShaderManager.Get.FloodFillCS.SetInt("_StepSize", 512 / 8);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Mask", tempBuffer);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Ping", _floodFillTexturePing);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Pong", _floodFillTexturePong);
        ComputeShaderManager.Get.FloodFillCS.SetFloat("_WidthScale", widthScale);
        ComputeShaderManager.Get.FloodFillCS.SetFloat("_HeightScale", heightScale);
        ComputeShaderManager.Get.FloodFillCS.Dispatch(0, Mathf.CeilToInt(tempBuffer.width / 8.0f), Mathf.CeilToInt(tempBuffer.height / 8.0f), 1);

        ComputeShaderManager.Get.FloodFillCS.SetInt("_StepSize", 512 / 16);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Mask", tempBuffer);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Ping", _floodFillTexturePong);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Pong", _floodFillTexturePing);
        ComputeShaderManager.Get.FloodFillCS.SetFloat("_WidthScale", widthScale);
        ComputeShaderManager.Get.FloodFillCS.SetFloat("_HeightScale", heightScale);
        ComputeShaderManager.Get.FloodFillCS.Dispatch(0, Mathf.CeilToInt(tempBuffer.width / 8.0f), Mathf.CeilToInt(tempBuffer.height / 8.0f), 1);

        ComputeShaderManager.Get.FloodFillCS.SetInt("_StepSize", 512 / 32);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Mask", tempBuffer);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Ping", _floodFillTexturePing);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Pong", _floodFillTexturePong);
        ComputeShaderManager.Get.FloodFillCS.SetFloat("_WidthScale", widthScale);
        ComputeShaderManager.Get.FloodFillCS.SetFloat("_HeightScale", heightScale);
        ComputeShaderManager.Get.FloodFillCS.Dispatch(0, Mathf.CeilToInt(tempBuffer.width / 8.0f), Mathf.CeilToInt(tempBuffer.height / 8.0f), 1);

        ComputeShaderManager.Get.FloodFillCS.SetInt("_StepSize", 512 / 64);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Mask", tempBuffer);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Ping", _floodFillTexturePong);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Pong", _floodFillTexturePing);
        ComputeShaderManager.Get.FloodFillCS.SetFloat("_WidthScale", widthScale);
        ComputeShaderManager.Get.FloodFillCS.SetFloat("_HeightScale", heightScale);
        ComputeShaderManager.Get.FloodFillCS.Dispatch(0, Mathf.CeilToInt(tempBuffer.width / 8.0f), Mathf.CeilToInt(tempBuffer.height / 8.0f), 1);

        ComputeShaderManager.Get.FloodFillCS.SetInt("_StepSize", 512 / 128);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Mask", tempBuffer);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Ping", _floodFillTexturePing);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Pong", _floodFillTexturePong);
        ComputeShaderManager.Get.FloodFillCS.SetFloat("_WidthScale", widthScale);
        ComputeShaderManager.Get.FloodFillCS.SetFloat("_HeightScale", heightScale);
        ComputeShaderManager.Get.FloodFillCS.Dispatch(0, Mathf.CeilToInt(tempBuffer.width / 8.0f), Mathf.CeilToInt(tempBuffer.height / 8.0f), 1);

        ComputeShaderManager.Get.FloodFillCS.SetInt("_StepSize", 512 / 256);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Mask", tempBuffer);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Ping", _floodFillTexturePong);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Pong", _floodFillTexturePing);
        ComputeShaderManager.Get.FloodFillCS.SetFloat("_WidthScale", widthScale);
        ComputeShaderManager.Get.FloodFillCS.SetFloat("_HeightScale", heightScale);
        ComputeShaderManager.Get.FloodFillCS.Dispatch(0, Mathf.CeilToInt(tempBuffer.width / 8.0f), Mathf.CeilToInt(tempBuffer.height / 8.0f), 1);

        ComputeShaderManager.Get.FloodFillCS.SetInt("_StepSize", 512 / 512);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Mask", tempBuffer);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Ping", _floodFillTexturePing);
        ComputeShaderManager.Get.FloodFillCS.SetTexture(0, "_Pong", _floodFillTexturePong);
        ComputeShaderManager.Get.FloodFillCS.SetFloat("_WidthScale", widthScale);
        ComputeShaderManager.Get.FloodFillCS.SetFloat("_HeightScale", heightScale);
        ComputeShaderManager.Get.FloodFillCS.Dispatch(0, Mathf.CeilToInt(tempBuffer.width / 8.0f), Mathf.CeilToInt(tempBuffer.height / 8.0f), 1);

        RenderTexture.ReleaseTemporary(tempBuffer);
    }

    void ComputeOcclusionMaskLEqual(RenderTexture tempBuffer, bool maskProtein, bool maskLipid)
    {
        // First clear mask buffer
        Graphics.SetRenderTarget(tempBuffer);
        GL.Clear(true, true, Color.blue);

        //***** Compute Protein Mask *****//
        if (maskProtein)
        {
            // Always clear append buffer before usage
            GPUBuffers.Get.SphereBatches.ClearAppendBuffer();

            //Fill the buffer with occludees
            ComputeShaderManager.Get.SphereBatchCS.SetUniform("_NumInstances", SceneManager.Get.NumProteinInstances);
            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(1, "_ProteinInstanceInfo", GPUBuffers.Get.ProteinInstancesInfo);
            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(1, "_ProteinInstanceCullFlags", GPUBuffers.Get.ProteinInstanceCullFlags);

            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(1, "_IngredientMaskParams", GPUBuffers.Get.IngredientMaskParams);
            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(1, "_OccludeeSphereBatches", GPUBuffers.Get.SphereBatches);
            ComputeShaderManager.Get.SphereBatchCS.Dispatch(1, Mathf.CeilToInt(SceneManager.Get.NumProteinInstances / 64.0f), 1, 1);

            // Count occludees instances
            ComputeBuffer.CopyCount(GPUBuffers.Get.SphereBatches, GPUBuffers.Get.ArgBuffer, 0);

            // Prepare draw call
            OcclusionQueriesMaterial.SetFloat("_Scale", GlobalProperties.Get.Scale);
            OcclusionQueriesMaterial.SetBuffer("_ProteinRadii", GPUBuffers.Get.ProteinRadii);
            OcclusionQueriesMaterial.SetBuffer("_ProteinInstanceInfo", GPUBuffers.Get.ProteinInstancesInfo);
            OcclusionQueriesMaterial.SetBuffer("_ProteinInstancePositions", GPUBuffers.Get.ProteinInstancePositions);
            OcclusionQueriesMaterial.SetBuffer("_OccludeeSphereBatches", GPUBuffers.Get.SphereBatches);
            OcclusionQueriesMaterial.SetPass(0);

            // Draw occludees - bounding sphere only - write to depth and stencil buffer
            Graphics.SetRenderTarget(tempBuffer);
            Graphics.DrawProceduralIndirect(MeshTopology.Points, GPUBuffers.Get.ArgBuffer);
        }

        //***** Compute Lipid Mask *****//
        if (maskLipid)
        {
            // Always clear append buffer before usage
            GPUBuffers.Get.SphereBatches.ClearAppendBuffer();

            //Fill the buffer with occludees
            ComputeShaderManager.Get.SphereBatchCS.SetUniform("_NumInstances", SceneManager.Get.NumLipidInstances);
            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(3, "_LipidInstanceInfo", GPUBuffers.Get.LipidInstancesInfo);
            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(3, "_LipidInstanceCullFlags", GPUBuffers.Get.LipidInstanceCullFlags);

            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(3, "_IngredientMaskParams", GPUBuffers.Get.IngredientMaskParams);
            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(3, "_OccludeeSphereBatches", GPUBuffers.Get.SphereBatches);
            ComputeShaderManager.Get.SphereBatchCS.Dispatch(3, Mathf.CeilToInt(SceneManager.Get.NumLipidInstances / 64.0f), 1, 1);

            // Count occludees instances
            ComputeBuffer.CopyCount(GPUBuffers.Get.SphereBatches, GPUBuffers.Get.ArgBuffer, 0);

            //DebugSphereBatchCount();

            // Prepare draw call
            OcclusionQueriesMaterial.SetFloat("_Scale", GlobalProperties.Get.Scale);
            OcclusionQueriesMaterial.SetBuffer("_LipidInstanceInfo", GPUBuffers.Get.LipidInstancesInfo);
            OcclusionQueriesMaterial.SetBuffer("_LipidInstancePositions", GPUBuffers.Get.LipidInstancePositions);
            OcclusionQueriesMaterial.SetBuffer("_OccludeeSphereBatches", GPUBuffers.Get.SphereBatches);
            OcclusionQueriesMaterial.SetPass(2);

            // Draw occludees - bounding sphere only - write to depth and stencil buffer
            Graphics.SetRenderTarget(tempBuffer);
            Graphics.DrawProceduralIndirect(MeshTopology.Points, GPUBuffers.Get.ArgBuffer);
        }
    }



    void ComputeOcclusionMaskGEqual(RenderTexture tempBuffer, bool maskProtein, bool maskLipid)
    {
        // First clear mask buffer
        Graphics.SetRenderTarget(tempBuffer);
        GL.Clear(true, true, Color.blue, 0);

        //***** Compute Protein Mask *****//
        if (maskProtein)
        {
            // Always clear append buffer before usage
            GPUBuffers.Get.SphereBatches.ClearAppendBuffer();

            //Fill the buffer with occludees
            ComputeShaderManager.Get.SphereBatchCS.SetUniform("_NumInstances", SceneManager.Get.NumProteinInstances);
            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(1, "_ProteinInstanceInfo", GPUBuffers.Get.ProteinInstancesInfo);
            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(1, "_ProteinInstanceCullFlags", GPUBuffers.Get.ProteinInstanceCullFlags);

            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(1, "_IngredientMaskParams", GPUBuffers.Get.IngredientMaskParams);
            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(1, "_OccludeeSphereBatches", GPUBuffers.Get.SphereBatches);
            ComputeShaderManager.Get.SphereBatchCS.Dispatch(1, Mathf.CeilToInt(SceneManager.Get.NumProteinInstances / 64.0f), 1, 1);

            // Count occludees instances
            ComputeBuffer.CopyCount(GPUBuffers.Get.SphereBatches, GPUBuffers.Get.ArgBuffer, 0);

            // Prepare draw call
            OcclusionQueriesMaterial.SetFloat("_Scale", GlobalProperties.Get.Scale);
            OcclusionQueriesMaterial.SetBuffer("_ProteinRadii", GPUBuffers.Get.ProteinRadii);
            OcclusionQueriesMaterial.SetBuffer("_ProteinInstanceInfo", GPUBuffers.Get.ProteinInstancesInfo);
            OcclusionQueriesMaterial.SetBuffer("_ProteinInstancePositions", GPUBuffers.Get.ProteinInstancePositions);
            OcclusionQueriesMaterial.SetBuffer("_OccludeeSphereBatches", GPUBuffers.Get.SphereBatches);
            OcclusionQueriesMaterial.SetPass(4);

            // Draw occludees - bounding sphere only - write to depth and stencil buffer
            Graphics.SetRenderTarget(tempBuffer);
            Graphics.DrawProceduralIndirect(MeshTopology.Points, GPUBuffers.Get.ArgBuffer);
        }

        //***** Compute Lipid Mask *****//
        if (maskLipid)
        {
            // Always clear append buffer before usage
            GPUBuffers.Get.SphereBatches.ClearAppendBuffer();

            //Fill the buffer with occludees
            ComputeShaderManager.Get.SphereBatchCS.SetUniform("_NumInstances", SceneManager.Get.NumLipidInstances);
            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(3, "_LipidInstanceInfo", GPUBuffers.Get.LipidInstancesInfo);
            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(3, "_LipidInstanceCullFlags", GPUBuffers.Get.LipidInstanceCullFlags);

            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(3, "_IngredientMaskParams", GPUBuffers.Get.IngredientMaskParams);
            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(3, "_OccludeeSphereBatches", GPUBuffers.Get.SphereBatches);
            ComputeShaderManager.Get.SphereBatchCS.Dispatch(3, Mathf.CeilToInt(SceneManager.Get.NumLipidInstances / 64.0f), 1, 1);

            // Count occludees instances
            ComputeBuffer.CopyCount(GPUBuffers.Get.SphereBatches, GPUBuffers.Get.ArgBuffer, 0);

            //DebugSphereBatchCount();

            // Prepare draw call
            OcclusionQueriesMaterial.SetFloat("_Scale", GlobalProperties.Get.Scale);
            OcclusionQueriesMaterial.SetBuffer("_LipidInstanceInfo", GPUBuffers.Get.LipidInstancesInfo);
            OcclusionQueriesMaterial.SetBuffer("_LipidInstancePositions", GPUBuffers.Get.LipidInstancePositions);
            OcclusionQueriesMaterial.SetBuffer("_OccludeeSphereBatches", GPUBuffers.Get.SphereBatches);
            OcclusionQueriesMaterial.SetPass(5);

            // Draw occludees - bounding sphere only - write to depth and stencil buffer
            Graphics.SetRenderTarget(tempBuffer);
            Graphics.DrawProceduralIndirect(MeshTopology.Points, GPUBuffers.Get.ArgBuffer);
        }
    }

    void ComputeOcclusionQueries(RenderTexture tempBuffer, CutObject cutObject, int cutObjectIndex, int internalState, bool cullProtein, bool cullLipid)
    {
        if (cullProtein)
        {
            // Always clear append buffer before usage
            GPUBuffers.Get.SphereBatches.ClearAppendBuffer();

            //Fill the buffer with occluders
            ComputeShaderManager.Get.SphereBatchCS.SetUniform("_NumInstances", SceneManager.Get.NumProteinInstances);
            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(1, "_ProteinInstanceInfo", GPUBuffers.Get.ProteinInstancesInfo);
            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(1, "_ProteinInstanceCullFlags", GPUBuffers.Get.ProteinInstanceCullFlags);

            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(1, "_IngredientMaskParams", GPUBuffers.Get.IngredientMaskParams);
            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(1, "_OccludeeSphereBatches", GPUBuffers.Get.SphereBatches);
            ComputeShaderManager.Get.SphereBatchCS.Dispatch(1, Mathf.CeilToInt(SceneManager.Get.NumProteinInstances / 64.0f), 1, 1);

            // Count occluder instances
            ComputeBuffer.CopyCount(GPUBuffers.Get.SphereBatches, GPUBuffers.Get.ArgBuffer, 0);

            //DebugSphereBatchCount();

            // Clear protein occlusion buffer 
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(0, "_FlagBuffer", GPUBuffers.Get.ProteinInstanceOcclusionFlags);
            ComputeShaderManager.Get.ComputeVisibilityCS.Dispatch(0, Mathf.CeilToInt(SceneManager.Get.NumProteinInstances / 64.0f), 1, 1);

            // Bind the read/write occlusion buffer to the shader
            // After this draw call the occlusion buffer will be filled with ones if an instance occluded and occludee, zero otherwise
            Graphics.SetRandomWriteTarget(1, GPUBuffers.Get.ProteinInstanceOcclusionFlags);
            MyUtility.DummyBlit();   // Dunny why yet, but without this I cannot write to the buffer from the shader, go figure

            // Set the render target
            Graphics.SetRenderTarget(tempBuffer);

            OcclusionQueriesMaterial.SetInt("_CutObjectIndex", cutObjectIndex);
            OcclusionQueriesMaterial.SetInt("_NumIngredients", SceneManager.Get.NumAllIngredients);
            OcclusionQueriesMaterial.SetBuffer("_CutInfo", GPUBuffers.Get.CutInfo);
            OcclusionQueriesMaterial.SetTexture("_DistanceField", _floodFillTexturePong);

            OcclusionQueriesMaterial.SetFloat("_Scale", GlobalProperties.Get.Scale);
            OcclusionQueriesMaterial.SetBuffer("_ProteinRadii", GPUBuffers.Get.ProteinRadii);
            OcclusionQueriesMaterial.SetBuffer("_ProteinInstanceInfo", GPUBuffers.Get.ProteinInstancesInfo);
            OcclusionQueriesMaterial.SetBuffer("_ProteinInstancePositions", GPUBuffers.Get.ProteinInstancePositions);
            OcclusionQueriesMaterial.SetBuffer("_OccludeeSphereBatches", GPUBuffers.Get.SphereBatches);
            OcclusionQueriesMaterial.SetPass(1);

            // Issue draw call for occluders - bounding quads only - depth/stencil test enabled - no write to color/depth/stencil
            Graphics.DrawProceduralIndirect(MeshTopology.Points, GPUBuffers.Get.ArgBuffer);
            Graphics.ClearRandomWriteTargets();

            ComputeShaderManager.Get.ComputeVisibilityCS.SetUniform("_CutObjectIndex", cutObjectIndex);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetUniform("_NumIngredients", SceneManager.Get.NumAllIngredients);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(3, "_CutInfo", GPUBuffers.Get.CutInfo);

            //// Discard occluding instances according to value2
            ComputeShaderManager.Get.ComputeVisibilityCS.SetUniform("_CutObjectId", cutObject.Id);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetUniform("_ConsumeRestoreState", internalState);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(3, "_Histograms", GPUBuffers.Get.Histograms);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(3, "_HistogramsLookup", GPUBuffers.Get.HistogramsLookup);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(3, "_ProteinInstanceInfo", GPUBuffers.Get.ProteinInstancesInfo);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(3, "_ProteinInstanceCullFlags", GPUBuffers.Get.ProteinInstanceCullFlags);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(3, "_ProteinInstanceOcclusionFlags", GPUBuffers.Get.ProteinInstanceOcclusionFlags);
            ComputeShaderManager.Get.ComputeVisibilityCS.Dispatch(3, Mathf.CeilToInt(SceneManager.Get.NumProteinInstances / 64.0f), 1, 1);
        }

        if (cullLipid)
        {
            // Always clear append buffer before usage
            GPUBuffers.Get.SphereBatches.ClearAppendBuffer();

            //Fill the buffer with occluders
            ComputeShaderManager.Get.SphereBatchCS.SetUniform("_NumInstances", SceneManager.Get.NumLipidInstances);
            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(3, "_LipidInstanceInfo", GPUBuffers.Get.LipidInstancesInfo);
            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(3, "_LipidInstanceCullFlags", GPUBuffers.Get.LipidInstanceCullFlags);

            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(3, "_IngredientMaskParams", GPUBuffers.Get.IngredientMaskParams);
            ComputeShaderManager.Get.SphereBatchCS.SetBuffer(3, "_OccludeeSphereBatches", GPUBuffers.Get.SphereBatches);
            ComputeShaderManager.Get.SphereBatchCS.Dispatch(3, Mathf.CeilToInt(SceneManager.Get.NumLipidInstances / 64.0f), 1, 1);

            // Count occluder instances
            ComputeBuffer.CopyCount(GPUBuffers.Get.SphereBatches, GPUBuffers.Get.ArgBuffer, 0);

            // Clear lipid occlusion buffer 
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(0, "_FlagBuffer", GPUBuffers.Get.LipidInstanceOcclusionFlags);
            ComputeShaderManager.Get.ComputeVisibilityCS.Dispatch(0, Mathf.CeilToInt(SceneManager.Get.NumLipidInstances / 64.0f), 1, 1);

            // Bind the read/write occlusion buffer to the shader
            // After this draw call the occlusion buffer will be filled with ones if an instance occluded and occludee, zero otherwise
            Graphics.SetRandomWriteTarget(1, GPUBuffers.Get.LipidInstanceOcclusionFlags);
            MyUtility.DummyBlit();   // Dunny why yet, but without this I cannot write to the buffer from the shader, go figure

            // Set the render target
            Graphics.SetRenderTarget(tempBuffer);

            OcclusionQueriesMaterial.SetInt("_CutObjectIndex", cutObjectIndex);
            OcclusionQueriesMaterial.SetInt("_NumIngredients", SceneManager.Get.NumAllIngredients);
            OcclusionQueriesMaterial.SetBuffer("_CutInfo", GPUBuffers.Get.CutInfo);
            OcclusionQueriesMaterial.SetTexture("_DistanceField", _floodFillTexturePong);

            OcclusionQueriesMaterial.SetFloat("_Scale", GlobalProperties.Get.Scale);
            OcclusionQueriesMaterial.SetBuffer("_LipidInstanceInfo", GPUBuffers.Get.LipidInstancesInfo);
            OcclusionQueriesMaterial.SetBuffer("_LipidInstancePositions", GPUBuffers.Get.LipidInstancePositions);
            OcclusionQueriesMaterial.SetBuffer("_OccludeeSphereBatches", GPUBuffers.Get.SphereBatches);
            OcclusionQueriesMaterial.SetPass(3);

            // Issue draw call for occluders - bounding quads only - depth/stencil test enabled - no write to color/depth/stencil
            Graphics.DrawProceduralIndirect(MeshTopology.Points, GPUBuffers.Get.ArgBuffer);
            Graphics.ClearRandomWriteTargets();

            ComputeShaderManager.Get.ComputeVisibilityCS.SetUniform("_CutObjectIndex", cutObjectIndex);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetUniform("_NumIngredients", SceneManager.Get.NumAllIngredients);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(4, "_CutInfo", GPUBuffers.Get.CutInfo);

            //// Discard occluding instances according to value2
            ComputeShaderManager.Get.ComputeVisibilityCS.SetUniform("_CutObjectId", cutObject.Id);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetUniform("_ConsumeRestoreState", internalState);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(4, "_Histograms", GPUBuffers.Get.Histograms);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(4, "_HistogramsLookup", GPUBuffers.Get.HistogramsLookup);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(4, "_LipidInstanceInfo", GPUBuffers.Get.LipidInstancesInfo);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(4, "_LipidInstanceCullFlags", GPUBuffers.Get.LipidInstanceCullFlags);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(4, "_LipidInstanceOcclusionFlags", GPUBuffers.Get.LipidInstanceOcclusionFlags);
            ComputeShaderManager.Get.ComputeVisibilityCS.Dispatch(4, Mathf.CeilToInt(SceneManager.Get.NumLipidInstances / 64.0f), 1, 1);
        }
    }

    //[ImageEffectOpaque]
    //void OnRenderImage(RenderTexture src, RenderTexture dst)
    void ComputeViewSpaceCutAways()
    {
        //ComputeProteinObjectSpaceCutAways();
        //ComputeLipidObjectSpaceCutAways();

        // Prepare and set the render target
        var tempBuffer = RenderTexture.GetTemporary(GetComponent<Camera>().pixelWidth, GetComponent<Camera>().pixelHeight, 32, RenderTextureFormat.ARGB32);
        //var tempBuffer = RenderTexture.GetTemporary(512, 512, 32, RenderTextureFormat.ARGB32);

        var resetCutSnapshot = CutObjectManager.Get.ResetCutSnapshot;
        CutObjectManager.Get.ResetCutSnapshot = -1;

        if (resetCutSnapshot > 0)
        {
            // Discard occluding instances according to value2
            ComputeShaderManager.Get.ComputeVisibilityCS.SetUniform("_CutObjectId", resetCutSnapshot);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetUniform("_ConsumeRestoreState", 2);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(3, "_Histograms", GPUBuffers.Get.Histograms);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(3, "_HistogramsLookup", GPUBuffers.Get.HistogramsLookup);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(3, "_IngredientMaskParams", GPUBuffers.Get.IngredientMaskParams);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(3, "_ProteinInstanceInfo", GPUBuffers.Get.ProteinInstancesInfo);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(3, "_ProteinInstanceCullFlags", GPUBuffers.Get.ProteinInstanceCullFlags);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(3, "_ProteinInstanceOcclusionFlags", GPUBuffers.Get.ProteinInstanceOcclusionFlags);
            ComputeShaderManager.Get.ComputeVisibilityCS.Dispatch(3, Mathf.CeilToInt(SceneManager.Get.NumProteinInstances / 64.0f), 1, 1);

            //// Discard occluding instances according to value2
            ComputeShaderManager.Get.ComputeVisibilityCS.SetUniform("_CutObjectId", resetCutSnapshot);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetUniform("_ConsumeRestoreState", 2);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(4, "_Histograms", GPUBuffers.Get.Histograms);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(4, "_HistogramsLookup", GPUBuffers.Get.HistogramsLookup);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(4, "_IngredientMaskParams", GPUBuffers.Get.IngredientMaskParams);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(4, "_LipidInstanceInfo", GPUBuffers.Get.LipidInstancesInfo);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(4, "_LipidInstanceCullFlags", GPUBuffers.Get.LipidInstanceCullFlags);
            ComputeShaderManager.Get.ComputeVisibilityCS.SetBuffer(4, "_LipidInstanceOcclusionFlags", GPUBuffers.Get.LipidInstanceOcclusionFlags);
            ComputeShaderManager.Get.ComputeVisibilityCS.Dispatch(4, Mathf.CeilToInt(SceneManager.Get.NumLipidInstances / 64.0f), 1, 1);
        }

        var cutObjectId = -1;

        foreach (var cutObject in CutObjectManager.Get.CutObjects)
        {
            cutObjectId++;

            //********************************************************//

            var internalState = 0;
            //Debug.Log(cutObject.CurrentLockState.ToString());

            if (cutObject.CurrentLockState == LockState.Restore)
            {
                Debug.Log("We restore");
                internalState = 2;
                cutObject.CurrentLockState = LockState.Unlocked;
            }

            if (cutObject.CurrentLockState == LockState.Consumed)
            {
                continue;
            }

            if (cutObject.CurrentLockState == LockState.Locked)
            {
                Debug.Log("We consume");
                internalState = 1;
                cutObject.CurrentLockState = LockState.Consumed;
            }

            //********************************************************//

            var maskLipid = false;
            var cullLipid = false;

            var maskProtein = false;
            var cullProtein = false;

            //Fill the buffer with occludees mask falgs
            var maskFlags = new List<int>();
            var cullFlags = new List<int>();

            foreach (var cutParam in cutObject.IngredientCutParameters)
            {
                var isMask = cutParam.IsFocus;
                var isCulled = !cutParam.IsFocus && (cutParam.Aperture > 0 || cutParam.value2 < 1);

                if (cutParam.IsFocus)
                {
                    if (cutParam.Id < SceneManager.Get.NumProteinIngredients) maskProtein = true;
                    else maskLipid = true;
                }

                if (isCulled)
                {
                    if (cutParam.Id < SceneManager.Get.NumProteinIngredients) cullProtein = true;
                    else cullLipid = true;
                }

                maskFlags.Add(isMask ? 1 : 0);
                cullFlags.Add(isCulled ? 1 : 0);
            }

            //if (!cullProtein && !cullLipid) continue;

            //********************************************************//

            //***** Compute Depth-Stencil mask *****//

            // Upload Occludees flags to GPU
            GPUBuffers.Get.IngredientMaskParams.SetData(maskFlags.ToArray());
            ComputeOcclusionMaskLEqual(tempBuffer, maskProtein, maskLipid);
            //ComputeOcclusionMaskGEqual(tempBuffer, maskProtein, maskLipid);
            //Graphics.Blit(tempBuffer, dst);
            //break;

            ComputeDistanceTransform(tempBuffer);

            //Graphics.Blit(_floodFillTexturePong, dst, CompositeMaterial, 4);
            //break;

            /////**** Compute Queries ***//

            // Upload Occluders flags to GPU
            GPUBuffers.Get.IngredientMaskParams.SetData(cullFlags.ToArray());
            ComputeOcclusionQueries(tempBuffer, cutObject, cutObjectId, internalState, cullProtein, cullLipid);
        }

        // Release render target
        RenderTexture.ReleaseTemporary(tempBuffer);
    }

    // With edges
    [ImageEffectOpaque]
    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        //return;

        if (_camera.pixelWidth == 0 || _camera.pixelHeight == 0) return;

        if (SceneManager.Get.NumProteinInstances == 0 && SceneManager.Get.NumLipidInstances == 0)
        {
            Graphics.Blit(src, dst);
            return;
        }

        CutAwayUtils.ComputeProteinObjectSpaceCutAways(_camera, WeightThreshold);
        CutAwayUtils.ComputeLipidObjectSpaceCutAways();
        ComputeViewSpaceCutAways();

        ///**** Start rendering routine ***

        // Declare temp buffers
        var colorBuffer = RenderTexture.GetTemporary(_camera.pixelWidth, _camera.pixelHeight, 0, RenderTextureFormat.ARGB32);
        var depthBuffer = RenderTexture.GetTemporary(_camera.pixelWidth, _camera.pixelHeight, 32, RenderTextureFormat.Depth);

        var atomIdBuffer = RenderTexture.GetTemporary(_camera.pixelWidth, _camera.pixelHeight, 0, RenderTextureFormat.RInt);
        var instanceIdBuffer = RenderTexture.GetTemporary(_camera.pixelWidth, _camera.pixelHeight, 0, RenderTextureFormat.RInt);

        var compositeColorBuffer = RenderTexture.GetTemporary(_camera.pixelWidth, _camera.pixelHeight, 0, RenderTextureFormat.ARGB32);
        var compositeColorBuffer2 = RenderTexture.GetTemporary(_camera.pixelWidth, _camera.pixelHeight, 0, RenderTextureFormat.ARGB32);

        var compositeDepthBuffer = RenderTexture.GetTemporary(_camera.pixelWidth, _camera.pixelHeight, 32, RenderTextureFormat.Depth);
        var compositeDepthBuffer2 = RenderTexture.GetTemporary(_camera.pixelWidth, _camera.pixelHeight, 32, RenderTextureFormat.Depth);

        // Clear temp buffers
        Graphics.SetRenderTarget(instanceIdBuffer);
        GL.Clear(true, true, new Color(-1, 0, 0, 0));

        Graphics.SetRenderTarget(colorBuffer.colorBuffer, depthBuffer.depthBuffer);
        GL.Clear(true, true, Color.white);

        // Draw proteins
        if (SceneManager.Get.NumProteinInstances > 0)
        {
            RenderUtils.ComputeSphereBatches(_camera);
            //DebugSphereBatchCount();
            RenderUtils.DrawProteinsAtoms(RenderProteinsMaterial, _camera, instanceIdBuffer.colorBuffer, atomIdBuffer.colorBuffer, depthBuffer.depthBuffer, 0);
        }

        // Draw Lipids
        if (SceneManager.Get.NumLipidInstances > 0)
        {
            RenderUtils.ComputeLipidSphereBatches(_camera);
            RenderUtils.DrawLipidSphereBatches(RenderLipidsMaterial, instanceIdBuffer, depthBuffer);
        }

        // Draw curve ingredients
        if (SceneManager.Get.NumDnaSegments > 0)
        {
            DrawCurveIngredients(instanceIdBuffer, atomIdBuffer, depthBuffer);
            //DrawCurveIngredients(colorBuffer, instanceIdBuffer, depthBuffer);
        }

        CutAwayUtils.ComputeVisibility(instanceIdBuffer);
        CutAwayUtils.FetchHistogramValues();

        //// Fetch color
        //CompositeMaterial.SetTexture("_IdTexture", itemBuffer);
        //Graphics.Blit(null, colorBuffer, CompositeMaterial, 3);

        //Compute color composition
        //ColorCompositeUtils.ComputeCoverage(instanceIdBuffer);
        //ColorCompositeUtils.CountInstances();
        ColorCompositeUtils.ComputeColorComposition(_camera, ColorCompositeMaterial, colorBuffer, instanceIdBuffer, atomIdBuffer, depthBuffer);

        // Compute contours detection
        SetContourShaderParams();
        ContourMaterial.SetTexture("_IdTexture", instanceIdBuffer);
        Graphics.Blit(colorBuffer, compositeColorBuffer, ContourMaterial, 0);
        
        
        // Composite with scene color
        CompositeMaterial.SetTexture("_ColorTexture", compositeColorBuffer);
        CompositeMaterial.SetTexture("_DepthTexture", depthBuffer);
        Graphics.Blit(null, src, CompositeMaterial, 0);
        //Graphics.Blit(src, dst);

        // TODO: compute average of the depthBuffer texture so that I can set the speed of zooming, arcballing and panning
        //RenderTexture.active = depthBuffer;
        //Texture2D tex = new Texture2D(depthBuffer.width, depthBuffer.height);
        //tex.ReadPixels(new Rect(0, 0, depthBuffer.width, depthBuffer.height), 0, 0);
        //var pixels = tex.GetPixels(10);
        //Debug.Log("depthBuffer \"average\" value = " + pixels[0].r);

        Shader.SetGlobalTexture("_CameraDepthTexture", depthBuffer);

        /*** Object Picking ***/

        /*
         * Here I think I should just read the instance value on the mouse position no matter if mouse is clicked or not
         */

        //Debug.Log("HierarchySelectedPath = " + NewSelectionManager.Instance.HierarchySelectedItemPath);
        int mouseInstanceId = MyUtility.ReadPixelId(instanceIdBuffer, NewSelectionManager.Instance.MousePosition);
        NewSelectionManager.Instance.HoveredInstanceId = mouseInstanceId;
        if (NewSelectionManager.Instance.MouseClicked)
        {
            // Read value from instanceIdBuffer
            int id = MyUtility.ReadPixelId(instanceIdBuffer, NewSelectionManager.Instance.MousePosition);

            //Debug.Log("Clicked ID: " + id);

            if (id == -1)                                                   // Click into background
            {
                NewSelectionManager.Instance.SelectedLipidInstanceId = -1;
                NewSelectionManager.Instance.SelectedProteinInstanceId = -1;
                NewSelectionManager.Instance.RnaSelected = false;
                NewSelectionManager.Instance.HierarchySelectedItemPath = "";
            }
            else if (id == -123)                                            // Click on RNA
            {
                NewSelectionManager.Instance.SelectedLipidInstanceId = -1;
                NewSelectionManager.Instance.SelectedProteinInstanceId = -1;
                NewSelectionManager.Instance.RnaSelected = true;
            }
            else if (id > 100000)                                           // Click on LIPID
            { // lipid
                NewSelectionManager.Instance.SelectedProteinInstanceId = -1;
                NewSelectionManager.Instance.RnaSelected = false;
                NewSelectionManager.Instance.SelectedLipidInstanceId = id;
                //Debug.Log("LIPID selected");
            } 
            else                                                            // Click on PROTEIN
            { // protein
                NewSelectionManager.Instance.SelectedLipidInstanceId = -1;
                NewSelectionManager.Instance.RnaSelected = false;
                NewSelectionManager.Instance.SelectedProteinInstanceId = id;
                //Debug.Log("PROTEIN selected");
            }

            InfoTextController.Get.ShowInfoForInstance(id); // TODO: could be optimised because in this function I do the same checks as in the ifs before

        }

        // Interaction Recording - REC border drawing
        if (InteractionRecorder.Get.currentState == RecorderState.Recording)
        {
            Graphics.Blit(src, dst, RecBorderMaterial);
        } else
        {
            Graphics.Blit(src, dst);
        }

        // Release temp buffers
        RenderTexture.ReleaseTemporary(instanceIdBuffer);
        RenderTexture.ReleaseTemporary(atomIdBuffer);

        RenderTexture.ReleaseTemporary(colorBuffer);
        RenderTexture.ReleaseTemporary(depthBuffer);

        RenderTexture.ReleaseTemporary(compositeColorBuffer);
        RenderTexture.ReleaseTemporary(compositeColorBuffer2);

        RenderTexture.ReleaseTemporary(compositeDepthBuffer);
        RenderTexture.ReleaseTemporary(compositeDepthBuffer2);
    }
}
