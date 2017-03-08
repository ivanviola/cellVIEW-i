using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(ColorInspectorProxy))]
public class CustomProxyInspector : Editor
{
    public bool[] b;

    public override void OnInspectorGUI()
    {
        ////DrawDefaultInspector();
        //var sceneManager = (SceneManager)target;

        if (b == null || b.Length != CPUBuffers.Get.IngredientGroupsColorRanges.Count)
        {
            b = new bool[CPUBuffers.Get.IngredientGroupsColorRanges.Count];
        }

        EditorGUILayout.Space();
        
        for (int i = 0; i < CPUBuffers.Get.IngredientGroupsColorRanges.Count; i++) 
        {
            b[i] = EditorGUILayout.Foldout(b[i], "Group " + i);
            if (b[i])
            {
                var hclColor = new Vector3();
                var hclRange = new Vector3();

                CPUBuffers.Get.IngredientGroupsLerpFactors[i] = EditorGUILayout.Slider("Lerp factor", CPUBuffers.Get.IngredientGroupsLerpFactors[i], 0, 1);
                
                EditorGUILayout.Separator();

                hclColor.x = EditorGUILayout.Slider("Hue Centroid", CPUBuffers.Get.IngredientGroupsColorValues[i].x, 0, 360);
                hclColor.y = EditorGUILayout.Slider("Chroma Centroid", CPUBuffers.Get.IngredientGroupsColorValues[i].y, 0, 140);
                hclColor.z = EditorGUILayout.Slider("Luminance", CPUBuffers.Get.IngredientGroupsColorValues[i].z, 0, 100);

                EditorGUILayout.Separator();

                hclRange.x = EditorGUILayout.Slider("Hue Offset", CPUBuffers.Get.IngredientGroupsColorRanges[i].x, 0, 360);
                hclRange.y = EditorGUILayout.Slider("Chroma Offset", CPUBuffers.Get.IngredientGroupsColorRanges[i].y, 0, 140);
                hclRange.z = EditorGUILayout.Slider("Luminance Offset", CPUBuffers.Get.IngredientGroupsColorRanges[i].z, 0, 100);

                CPUBuffers.Get.IngredientGroupsColorValues[i] = hclColor;
                CPUBuffers.Get.IngredientGroupsColorRanges[i] = hclRange;
            }
        }

        // Make all scene dirty to get changes to save
        if (GUI.changed)
        {
            EditorSceneManager.MarkAllScenesDirty();

            GPUBuffers.Get.IngredientGroupsColorValues.SetData(CPUBuffers.Get.IngredientGroupsColorValues.ToArray());
            GPUBuffers.Get.IngredientGroupsColorRanges.SetData(CPUBuffers.Get.IngredientGroupsColorRanges.ToArray());
            GPUBuffers.Get.IngredientGroupsLerpFactors.SetData(CPUBuffers.Get.IngredientGroupsLerpFactors.ToArray());
        }
    }
}