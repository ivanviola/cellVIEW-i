using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(ColorManager))]
public class ColorManagerInspector : Editor
{
    public bool foldout;
    public bool[] foldouts;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var colorManager = (ColorManager)target;

        if (foldouts == null || foldouts.Length != CPUBuffers.Get.IngredientGroupsColorRanges.Count)
        {
            foldouts = new bool[CPUBuffers.Get.IngredientGroupsColorRanges.Count];
        }

        for (int i = 0; i < CPUBuffers.Get.IngredientGroupsColorRanges.Count; i++)
        {
            foldouts[i] = EditorGUILayout.Foldout(foldouts[i], "Group " + i);
            if (foldouts[i])
            {
                var hclColor = new Vector3();
                var hclRange = new Vector3();

                //CPUBuffers.Get.IngredientGroupsLerpFactors[i] = EditorGUILayout.Slider("Lerp factor", CPUBuffers.Get.IngredientGroupsLerpFactors[i], 0, 1);

                EditorGUILayout.Separator();

                hclColor.x = EditorGUILayout.Slider("Hue", CPUBuffers.Get.IngredientGroupsColorValues[i].x, 0, 360);
                hclColor.y = EditorGUILayout.Slider("Chroma", CPUBuffers.Get.IngredientGroupsColorValues[i].y, 0, 140);
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
            

            GPUBuffers.Get.IngredientGroupsColorValues.SetData(CPUBuffers.Get.IngredientGroupsColorValues.ToArray());
            GPUBuffers.Get.IngredientGroupsColorRanges.SetData(CPUBuffers.Get.IngredientGroupsColorRanges.ToArray());
            GPUBuffers.Get.IngredientGroupsLerpFactors.SetData(CPUBuffers.Get.IngredientGroupsLerpFactors.ToArray());

            if(!Application.isPlaying)
            {
                EditorUtility.SetDirty(target);
                EditorSceneManager.MarkAllScenesDirty();
            }
        }
    }
}