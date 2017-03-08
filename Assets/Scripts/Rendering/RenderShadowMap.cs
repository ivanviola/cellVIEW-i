using System;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class RenderShadowMap : MonoBehaviour
{
    public Material RenderCurveMaterial;
    public Material RenderLipidMaterial;
    public Material RenderProteinMaterial;

    public RenderTexture ShadowMap;
    public RenderTexture ShadowMap2;

    //public float Downscale = 1;

    /*****/

    private Camera lightCamera;

    /*****/

    void OnEnable()
    {
        lightCamera = GetComponent<Camera>();
        lightCamera.depthTextureMode |= DepthTextureMode.Depth;
        lightCamera.depthTextureMode |= DepthTextureMode.DepthNormals;
        lightCamera.targetTexture = ShadowMap;
    }

    void OnDisable()
    {
        if (ShadowMap2 != null)
        {
            ShadowMap2.DiscardContents();
            DestroyImmediate(ShadowMap2);
        }
    }

    void OnPostRender()
    {
        //if (!MainCamera.main.GetComponent<SceneRenderer>().EnableLight) return;

        //if (ShadowMap2 != null)
        //{
        //    if (ShadowMap2.width != (int)(Camera.main.pixelWidth) || ShadowMap2.height != (int)(Camera.main.pixelHeight))
        //    {
        //        RenderTexture.active = null;
        //        GetComponent<Camera>().targetTexture = ShadowMap;
        //        ShadowMap2.DiscardContents();
        //        DestroyImmediate(ShadowMap2);
        //        ShadowMap2 = null;
        //    }
        //}

        //if (ShadowMap2 == null)
        //{
        //    ShadowMap2 = new RenderTexture((int)(Camera.main.pixelWidth), (int)(Camera.main.pixelHeight), 24, RenderTextureFormat.RFloat);
        //    GetComponent<Camera>().targetTexture = ShadowMap2;
        //}

        Graphics.SetRenderTarget(ShadowMap2);
        GL.Clear(true, true, Color.black);

        // Draw proteins
        if (SceneManager.Get.NumProteinInstances > 0)
        {
            RenderUtils.ComputeSphereBatches(lightCamera);
            RenderUtils.DrawProteinsShadowMap(RenderProteinMaterial, lightCamera, ShadowMap2.colorBuffer, ShadowMap2.depthBuffer, 1);
        }

        // Draw Lipids
        if (SceneManager.Get.NumLipidInstances > 0)
        {
            RenderUtils.ComputeLipidSphereBatches(lightCamera);
            RenderUtils.DrawLipidShadows(RenderLipidMaterial, ShadowMap2, ShadowMap2);
        }
    }
}