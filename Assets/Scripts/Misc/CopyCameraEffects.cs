using System.Collections.Generic;

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class CopyCameraEffects : MonoBehaviour
{
#if UNITY_EDITOR
            
    private Camera _sceneCamera;    
    public Camera SceneCamera
    {
        get
        {
            if (_sceneCamera == null) _sceneCamera = MyUtility.GetWindowDontShow<SceneView>().camera;
            return _sceneCamera;
        }
    }

    bool hasCopied = false;

    void OnEnable()
    {
        hasCopied = false;

        if (!EditorApplication.isPlaying)
        {
            EditorApplication.update += ForceCopyAtLaunch;            
        }
    }

    void OnDisable()
    {
        if(SceneCamera)
        ClearSceneCameraEffects();
    }

    public void Update()
    {
        ForceCopyAtLaunch();
    }    

    public void ForceCopyAtLaunch()
    {
        if (!hasCopied && this != null && GetComponent<Camera>() != null && SceneCamera != null)
        {
            hasCopied = true;
            
            CopyEffects();
            EditorApplication.update -= ForceCopyAtLaunch;
        }
    }

    public void CopyEffects()
    {
        Debug.Log("Copy camera effects");

        ClearSceneCameraEffects();
        CopyGameCameraEffects();

        // Now the effects have been copies force camera to render to avoid bad glitches
        SceneCamera.GetComponent<Camera>().Render();
        GetComponent<Camera>().GetComponent<Camera>().Render();
    }

    private static List<Component> GetCameraEffects(Camera camera)
    {
        var components = camera.GetComponents<Component>();
        var results = new List<Component>();

        foreach (var component in components)
        {
            //if (component is SSAOPro || component is SceneRenderer)
            if (component is SceneRenderer)
            {
                results.Add(component);
            }                
        }

        return results;
    }    

    public void ClearSceneCameraEffects()
    {
        foreach (var component in GetCameraEffects(SceneCamera))
        {
            DestroyImmediate(component);
        }
    }

    public void CopyGameCameraEffects()
    {
        foreach (var component in GetCameraEffects(GetComponent<Camera>()))
        {
            var copy = SceneCamera.gameObject.AddComponent(component.GetType());
            EditorUtility.CopySerialized(component, copy);
        }
    }  
#endif
}