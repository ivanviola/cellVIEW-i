using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

class OptionsWindow : EditorWindow
{
    private readonly string[] _contourOptionsLabels = { "Show Contour", "Hide Contour", "Contour Only" };

    void OnGUI()
    {
        //GUIStyle style_1 = new GUIStyle();
        //style_1.margin = new RectOffset(5, 5, 5, 5);
        
        EditorGUILayout.LabelField("Base Settings", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        GlobalProperties.Get.Scale = EditorGUILayout.Slider("Global scale", GlobalProperties.Get.Scale,
            0.0001f, 1);
        GlobalProperties.Get.ContourStrength = EditorGUILayout.Slider("Contour strength",
            GlobalProperties.Get.ContourStrength, 0, 1);
        GlobalProperties.Get.ContourOptions = EditorGUILayout.Popup("Contours Options",
            GlobalProperties.Get.ContourOptions, _contourOptionsLabels);
        //DisplaySettings.Get.EnableOcclusionCulling = EditorGUILayout.Toggle("Enable Culling", DisplaySettings.Get.EnableOcclusionCulling);
        GlobalProperties.Get.DebugObjectCulling = EditorGUILayout.Toggle("Debug Culling",
            GlobalProperties.Get.DebugObjectCulling);
        EditorGUI.indentLevel--;
        EditorGUILayout.Space();

        GlobalProperties.Get.EnableLod = EditorGUILayout.BeginToggleGroup("Level of Detail",
            GlobalProperties.Get.EnableLod);
        {
            GlobalProperties.Get.FirstLevelOffset = EditorGUILayout.FloatField("First Level Being Range",
                GlobalProperties.Get.FirstLevelOffset);

            EditorGUI.indentLevel++;
            for (int i = 0; i <= SceneManager.Get.NumLodLevels; i++)
            {
                EditorGUILayout.LabelField("Level " + i, EditorStyles.boldLabel);
                EditorGUI.indentLevel++;

                var x = EditorGUILayout.FloatField("End Range", GlobalProperties.Get.LodLevels[i].x);
                var y = EditorGUILayout.FloatField("Min Radius", GlobalProperties.Get.LodLevels[i].y);
                var z = EditorGUILayout.FloatField("Max Radius", GlobalProperties.Get.LodLevels[i].z);

                var lodInfo = new Vector4(x, y, z, 1);

                if (GlobalProperties.Get.LodLevels[i] != lodInfo)
                {
                    GlobalProperties.Get.LodLevels[i] = lodInfo;
                    GPUBuffers.Get.LodInfo.SetData(GlobalProperties.Get.LodLevels);
                }

                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndToggleGroup();

        // Make all scene dirty to get changes to save
        if (GUI.changed)
        {
            EditorUtility.SetDirty(SceneManager.Get);
            EditorUtility.SetDirty(GlobalProperties.Get);
            EditorSceneManager.MarkAllScenesDirty();
        }
    }
}