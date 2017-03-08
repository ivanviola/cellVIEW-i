//C# Example

using System;
using System.Collections.Generic;
using System.IO;
using Loaders;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class CellViewMenuCommands
{
    // Add menu item named "My Window" to the Window menu
    [MenuItem("cellVIEW/Load Color Palette")]
    public static void LoadColorPalette()
    {
        ColorManager.Get.LoadColorPalette();
        EditorUtility.SetDirty(SceneManager.Get);
        EditorSceneManager.MarkAllScenesDirty();
    }

    // Add menu item named "My Window" to the Window menu
    [MenuItem("cellVIEW/Reload Color Palette")]
    public static void ReloadColorPalette()
    {
        ColorManager.Get.LoadColorPalette(GlobalProperties.Get.LastColorPaletteLoaded);
        EditorUtility.SetDirty(SceneManager.Get);
        EditorSceneManager.MarkAllScenesDirty();
    }

    // Add menu item named "My Window" to the Window menu
    [MenuItem("cellVIEW/Save Color Palette")]
    public static void SaveColorPalette()
    {
        ColorManager.Get.SaveCurrentColorPalette();
        //EditorUtility.SetDirty(SceneManager.Get);
        //EditorSceneManager.MarkAllScenesDirty();
    }

    // Add menu item named "My Window" to the Window menu
    [MenuItem("cellVIEW/Reload Colors")]
    public static void ReloadColors()
    {
        ColorManager.Get.ReloadColors();
        EditorUtility.SetDirty(SceneManager.Get);
        EditorSceneManager.MarkAllScenesDirty();
    }

    [MenuItem("cellVIEW/Set Current Camera Position As Default")]
    public static void SetDefaultCameraPosition()
    {
        var go = GameObject.Find("Main Camera Controller");
        var cameraController = go.GetComponent<MainCameraController>();
        var currentPos = cameraController.transform.position;
        var currentRot = cameraController.transform.rotation;
        cameraController.CameraDefaultPosition = new Vector4(currentPos.x, currentPos.y, currentPos.z);
        cameraController.CameraDefaultRotation = new Quaternion(currentRot.x, currentRot.y, currentRot.z, currentRot.w);
    }

    // Add menu item named "My Window" to the Window menu
    [MenuItem("cellVIEW/Load Membrane")]
    public static void LoadMembrane()
    {
        CellPackLoader.LoadMembrane();
    }

    [MenuItem("cellVIEW/Load RNA")]
    public static void LoadRNA()
    {
        CellPackLoader.LoadRNA();
    }

    [MenuItem("cellVIEW/Options")]
    public static void ShowOptions()
    {
        EditorWindow.GetWindow(typeof(OptionsWindow), false, "Options");
    }

    // Add menu item named "My Window" to the Window menu
    [MenuItem("cellVIEW/Clear scene")]
    public static void ClearScene()
    {
        SceneManager.Get.ClearScene();
        EditorUtility.SetDirty(SceneManager.Get);
        EditorSceneManager.MarkAllScenesDirty();
    }

    // Add menu item named "My Window" to the Window menu
    [MenuItem("cellVIEW/Load cellPACK recipe")]
    public static void LoadCellPackRecipe()
    {
        CellPackLoader.LoadCellPackRecipe();
        EditorUtility.SetDirty(SceneManager.Get);
        EditorSceneManager.MarkAllScenesDirty();
    }

    // Add menu item named "My Window" to the Window menu
    [MenuItem("cellVIEW/Reload cellPACK recipe")]
    public static void ReloadCellPackRecipe()
    {
        CellPackLoader.ReloadCellPackRecipe();
        EditorUtility.SetDirty(SceneManager.Get);
        EditorSceneManager.MarkAllScenesDirty();
    }

    // Add menu item named "My Window" to the Window menu
    [MenuItem("cellVIEW/Load cellPACK positions")]
    public static void LoadCellPackPositions()
    {
        CellPackLoader.LoadCellPackPositions();
        EditorUtility.SetDirty(SceneManager.Get);
        EditorSceneManager.MarkAllScenesDirty();
    }

    // Add menu item named "My Window" to the Window menu
    [MenuItem("cellVIEW/Reload cellPACK positions")]
    public static void ReloadCellPackPositions()
    {
        CellPackLoader.ReloadCellPackPositions();
        EditorUtility.SetDirty(SceneManager.Get);
        EditorSceneManager.MarkAllScenesDirty();
    }

    // Add menu item named "My Window" to the Window menu
    //[MenuItem("cellVIEW/debug PDB Loader")]
    //public static void ReloadCellPackResults()
    //{
    //    PdbLoader.LoadAtomDataFull("1VU4CtoH_hex_manu");
    //}

    //[MenuItem("cellVIEW/Generate JSON with descriptions")]
    //public static void GenerateJSONWithDescriptions()
    //{
    //    InfoTextController.GenerateJSON();
    //}

    //// Add menu item named "My Window" to the Window menu
    //[MenuItem("cellVIEW/Reload cellPACK results")]
    //public static void ReloadCellPackResults()
    //{
    //    SceneManager.Get.ClearScene();
    //    CellPackLoader.ReloadCellPackResults();
    //    EditorUtility.SetDirty(SceneManager.Get);
    //}

    //// Add menu item named "My Window" to the Window menu
    //[MenuItem("cellVIEW/Load cellPACK results")]
    //public static void LoadCellPackResults()
    //{
    //    SceneManager.Get.ClearScene();
    //    CellPackLoader.LoadCellPackResults();
    //    EditorUtility.SetDirty(SceneManager.Get);
    //}


    //// Add menu item named "My Window" to the Window menu
    //[MenuItem("cellVIEW/Reload cellPACK results 2")]
    //public static void ReloadCellPackResults2()
    //{
    //    //SceneManager.Get.ClearScene();
    //    CellPackLoader.ReloadCellPackResults();
    //    EditorUtility.SetDirty(SceneManager.Get);
    //}

    //// Add menu item named "My Window" to the Window menu
    //[MenuItem("cellVIEW/Load cellPACK results 2")]
    //public static void LoadCellPackResults2()
    //{
    //    //SceneManager.Get.ClearScene();
    //    CellPackLoader.LoadCellPackResults();
    //    EditorUtility.SetDirty(SceneManager.Get);
    //}

    //// Add menu item named "My Window" to the Window menu
    //[MenuItem("cellVIEW/Load Membrane")]
    //public static void LoadMembrane()
    //{
    //    CellPackLoader.LoadMembrane();
    //}

    //  // Add menu item named "My Window" to the Window menu
    //  [MenuItem("cellVIEW/Add Cut Plane")]
    //  public static void AddCutPlane()
    //  {
    //      SceneManager.Get.AddCutObject(CutType.Plane);
    //  }

    //  // Add menu item named "My Window" to the Window menu
    //  [MenuItem("cellVIEW/Add Cut Sphere")]
    //  public static void AddCutSphere()
    //  {
    //      SceneManager.Get.AddCutObject(CutType.Sphere);
    //  }

    //  // Add menu item named "My Window" to the Window menu
    //  [MenuItem("cellVIEW/Add Cut Cube")]
    //  public static void AddCutCube()
    //  {
    //      SceneManager.Get.AddCutObject(CutType.Cube);
    //  }

    // Add menu item named "My Window" to the Window menu
    //[MenuItem("cellVIEW/Add protein")]
    //public static void DebugAddProtein()
    //{
    //    var atomSet = PdbLoader.LoadAtomSet("2hhb");

    //    int a = 0;
    //    //var spheres = AtomHelper.GetAtomSpheres(atomSet);
    //    //var bounds = AtomHelper.ComputeBounds(spheres);

    //    //SceneManager.Get.AddIngredient("MA_matrix_G1", bounds, spheres, Color.blue, new List<float>() { 0.2f, 0.1f, 0.05f, 0.03f });
    //    //SceneManager.Get.AddIngredientInstance("MA_matrix_G1", new Vector3(2, 2, 2), Quaternion.identity);
    //    //SceneManager.Get.CopyDataToGPU();
    //}
}