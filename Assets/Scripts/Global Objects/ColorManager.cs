using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Assets.Scripts.Loaders;
using Newtonsoft.Json;
//using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;


[ExecuteInEditMode]
public class ColorManager : MonoBehaviour
{
    // Declare the scene manager as a singleton
    private static ColorManager _instance = null;

    [HideInInspector]
    [Range(0, 1)]
    public float depthSlider = 0;

    public static ColorManager Get
    {
        get
        {
            if (_instance != null) return _instance;

            _instance = FindObjectOfType<ColorManager>();
            if (_instance == null)
            {
                var go = GameObject.Find("_ColorManager");
                if (go != null) DestroyImmediate(go);

                go = new GameObject("_ColorManager");
                _instance = go.AddComponent<ColorManager>();
                //_instance.hideFlags = HideFlags.HideInInspector;
                _instance.hideFlags = HideFlags.None;
            }

            return _instance;
        }
    }

    [HideInInspector]
    public bool UseHCL;
    [HideInInspector]
    public bool ShowAtoms;
    [HideInInspector]
    public bool ShowChains;
    [HideInInspector]
    public bool ShowResidues;
    [HideInInspector]
    public bool ShowSecondaryStructures;

    [HideInInspector]
    public float AtomDistance;
    [HideInInspector]
    public float ResidueDistance;
    [HideInInspector]
    public float SecondaryStructureDistance;

    //*******//
    [HideInInspector]
    public int NumLevelMax;
    [HideInInspector]
    public int DistanceMax = 200;
    [HideInInspector]
    public bool UseDistanceLevels = false;

    [HideInInspector]
    public float LevelLerpFactor = 0;


    private float[] _hueShifts = { 0f, 0.15f, 0.30f, 0.45f, 0.60f, 0.77f };

    [HideInInspector]
    public float[] LevelRanges = new float[0];


    //*******//

    public void InitColors()
    {
        CPUBuffers.Get.IngredientsInfo.Clear();
        CPUBuffers.Get.IngredientsDisplayInfo.Clear();
        CPUBuffers.Get.IngredientGroupsDisplayInfo.Clear();

        // Predefined colors

        CPUBuffers.Get.IngredientsColors.Clear();
        CPUBuffers.Get.IngredientGroupsColor.Clear();
        CPUBuffers.Get.ProteinIngredientsChainColors.Clear();

        var tempIngredientsColors = new Vector4[SceneManager.Get.NumAllIngredients];
        var tempIngredientsChainColors = new Vector4[SceneManager.Get.NumAllIngredients];

        // Properties to generate colors on the fly

        CPUBuffers.Get.IngredientGroupsLerpFactors.Clear();
        CPUBuffers.Get.IngredientGroupsColorRanges.Clear();
        CPUBuffers.Get.IngredientGroupsColorValues.Clear();
        CPUBuffers.Get.ProteinIngredientsRandomValues.Clear();

        foreach (var group in SceneManager.Get.IngredientGroups)
        {
            CPUBuffers.Get.IngredientGroupsDisplayInfo.Add(new DisplayInfo(group.NumIngredients, 0, 0, 0));

            var currentHue = _hueShifts[group.unique_id] * 360.0f;

            // Predified group color
            CPUBuffers.Get.IngredientGroupsColor.Add(MyUtility.ColorFromHSV(currentHue, 1, 1));

            //...
            CPUBuffers.Get.IngredientGroupsLerpFactors.Add(0);
            CPUBuffers.Get.IngredientGroupsColorValues.Add(new Vector4(currentHue, 75, 75));
            CPUBuffers.Get.IngredientGroupsColorRanges.Add(new Vector4(80, 0, 0));

            //*******//

            var offsetInc = 1.0f / group.Ingredients.Count;

            for (var i = 0; i < group.Ingredients.Count; i++)
            {
                if (!SceneManager.Get.AllIngredientNames.Contains(group.Ingredients[i].path))
                {
                    throw new Exception("Unknown ingredient: " + group.Ingredients[i].path);
                }

                CPUBuffers.Get.IngredientsDisplayInfo.Add(new DisplayInfo(group.Ingredients[i].nbMol, 0, 0, 0));

                var currentChroma = Random.Range(0.5f, 1);

                CPUBuffers.Get.IngredientsInfo.Add(new Vector4(group.unique_id, group.Ingredients[i].nbChains, CPUBuffers.Get.ProteinIngredientsChainColors.Count, 0));

                // Predefined ingredient color
                CPUBuffers.Get.IngredientsColors.Add(MyUtility.ColorFromHSV(currentHue, currentChroma, 1));

                for (var j = 0; j < group.Ingredients[i].nbChains; j++)
                {
                    var currentLuminance = Random.Range(0.5f, 1);
                    CPUBuffers.Get.ProteinIngredientsChainColors.Add(MyUtility.ColorFromHSV(currentHue, currentChroma, currentLuminance));
                }

                // ...
                CPUBuffers.Get.ProteinIngredientsRandomValues.Add(new Vector4(i * offsetInc, Random.Range(0.0f, 1.0f), 0));
            }
        }

        int a = 0;
    }

    public void InitColors2()
    {
        var tempIngredientGroupColor = new Color[SceneManager.Get.NumAllIngredients];

        var tempIngredientsInfo = new Vector4[SceneManager.Get.NumAllIngredients];
        var tempIngredientsColors = new Vector4[SceneManager.Get.NumAllIngredients];
        var tempIngredientsChainColors = new Vector4[SceneManager.Get.NumAllIngredients];

        foreach (var group in SceneManager.Get.IngredientGroups)
        {
            var currentHue = _hueShifts[group.unique_id] * 360.0f;
            tempIngredientGroupColor[group.unique_id] = MyUtility.ColorFromHSV(currentHue, 1, 1);

            //*******//

            var offsetInc = 1.0f / group.Ingredients.Count;
            var ingredientCount = 0;

            foreach (var ingredient in group.Ingredients)
            {
                if (!SceneManager.Get.ProteinIngredientNames.Contains(ingredient.path))
                {
                    throw new Exception("Unknown ingredient: " + ingredient.path);
                }

                var currentChroma = Random.Range(0.5f, 1);

                tempIngredientsInfo[ingredient.ingredient_id] = new Vector4(group.unique_id, ingredient.nbChains, CPUBuffers.Get.ProteinIngredientsChainColors.Count, 0);
                tempIngredientsColors[ingredient.ingredient_id] = (MyUtility.ColorFromHSV(currentHue, currentChroma, 1));

                //for (var j = 0; j < ingredient.nbChains; j++)
                //{
                //    var currentLuminance = Random.Range(0.5f, 1);
                //    CPUBuffers.Get.IngredientsChainColors.Add(MyUtility.ColorFromHSV(currentHue, currentChroma, currentLuminance));
                //}

                ingredientCount++;
            }
        }

        CPUBuffers.Get.IngredientsInfo = tempIngredientsInfo.ToList();
        CPUBuffers.Get.IngredientGroupsColor = tempIngredientGroupColor.ToList();
        CPUBuffers.Get.IngredientsColors = tempIngredientsColors.ToList();
        CPUBuffers.Get.ProteinIngredientsChainColors = tempIngredientsChainColors.ToList();
    }

    public void ReloadColors()
    {
        InitColors();
        CPUBuffers.Get.CopyDataToGPU();

        //Debug.Log("Reloading colors !!");
        //setHueCircleColors();


        //GPUBuffers.Get.IngredientGroupsColor.SetData(CPUBuffers.Get.IngredientGroupsColor.ToArray());
        //GPUBuffers.Get.IngredientsColors.SetData(CPUBuffers.Get.IngredientsColors.ToArray());
        //GPUBuffers.Get.IngredientsChainColors.SetData(CPUBuffers.Get.ProteinIngredientsChainColors.ToArray());

        //GPUBuffers.Get.IngredientGroupsLerpFactors.SetData(CPUBuffers.Get.IngredientGroupsLerpFactors.ToArray());
        //GPUBuffers.Get.IngredientGroupsColorValues.SetData(CPUBuffers.Get.IngredientGroupsColorValues.ToArray());
        //GPUBuffers.Get.IngredientGroupsColorRanges.SetData(CPUBuffers.Get.IngredientGroupsColorRanges.ToArray());
        //GPUBuffers.Get.ProteinIngredientsRandomValues.SetData(CPUBuffers.Get.ProteinIngredientsRandomValues.ToArray());

    }

    private void setHueCircleColors()
    {
        CPUBuffers.Get.IngredientGroupsColor.Clear();
        CPUBuffers.Get.IngredientGroupsLerpFactors.Clear();
        CPUBuffers.Get.IngredientGroupsColorValues.Clear();
        CPUBuffers.Get.IngredientGroupsColorRanges.Clear();
        CPUBuffers.Get.ProteinIngredientsRandomValues.Clear();
        CPUBuffers.Get.ProteinIngredientsChainColors.Clear();
        CPUBuffers.Get.IngredientsColors.Clear();

        int[] numMembersIngredientGroups = new int[SceneManager.Get.IngredientGroups.Count];
        ArrayList numMembersIngredients = new ArrayList();
        for (int i = 0; i < SceneManager.Get.IngredientGroups.Count; i++)
        {
            numMembersIngredientGroups[i] = SceneManager.Get.IngredientGroups[i].Ingredients.Count;
            for (int j = 0; j < SceneManager.Get.IngredientGroups[i].Ingredients.Count; j++)
            {
                numMembersIngredients.Add(SceneManager.Get.IngredientGroups[i].Ingredients[j].nbChains);
            }
        }
        float[] anglefractions;
        float[] angleCentroids;
        float[] ingredientsAnglefractions;
        float[] ingredientsAngleCentroids;
        float startangle = 0;
        float endangle = 360;

        getFractionsAndCentroid(numMembersIngredientGroups, startangle, endangle, out anglefractions, out angleCentroids);
        getFractionsAndCentroid(numMembersIngredients.OfType<int>().ToArray(), startangle, endangle, out ingredientsAnglefractions, out ingredientsAngleCentroids);

        for (int i = 0; i < SceneManager.Get.IngredientGroups.Count; i++)
        {
            Debug.Log("anglecentroid i " + i + " " + angleCentroids[i]);
            Debug.Log("anglefractions i " + i + " " + anglefractions[i]);
            CPUBuffers.Get.IngredientGroupsColor.Add(new Color(angleCentroids[i] / 360f, 60f / 100f, 70f / 100f));
            var group = SceneManager.Get.IngredientGroups[i];
            var offsetInc = 1.0f / group.Ingredients.Count;
            for (int j = 0; j < group.Ingredients.Count; j++)
            {
                Debug.Log("j loop i " + i);
                Debug.Log("loop anglecentroid i " + i + " " + angleCentroids[i]);
                CPUBuffers.Get.IngredientsColors.Add(new Vector4(angleCentroids[i] + anglefractions[i] * (j * offsetInc - 0.5f), 60, 70));
                CPUBuffers.Get.IngredientGroupsLerpFactors.Add(0);
                CPUBuffers.Get.IngredientGroupsColorValues.Add(new Vector4(angleCentroids[i], 60, 90));// 15 + Random.value * 85));
                CPUBuffers.Get.IngredientGroupsColorRanges.Add(new Vector4(anglefractions[i], 0, 0));
                CPUBuffers.Get.ProteinIngredientsRandomValues.Add(new Vector4(j * offsetInc, 0, 0));


                var ingredient = group.Ingredients[j];
                var chainOffset = 1.0f / ingredient.nbChains;
                for (int k = 0; k < ingredient.nbChains; k++)
                {
                    float currentHue = ingredientsAngleCentroids[j] + (k * chainOffset - 0.5f) * ingredientsAnglefractions[j];
                    float currentChroma = 60f;
                    float currentLuminance = 60f;
                    CPUBuffers.Get.ProteinIngredientsChainColors.Add(new Vector4(Random.value * 360, currentChroma, 50 + Random.value * 20));

                }

            }
        }





    }





    private void getFractionsAndCentroid(int[] numMembers, float startangle, float endangle, out float[] anglefractions, out float[] angleCentroids)
    {
        anglefractions = new float[numMembers.Length];
        angleCentroids = new float[numMembers.Length];
        float sum = 0;
        for (int i = 0; i < numMembers.Length; i++)
        {
            sum += numMembers[i];
        }
        //inbetween angle (since otherwise there will be 2 proteins with the same color at every edge).
        float inbetweenangle = (endangle - startangle) / 100f; //temp placeholder




        for (int i = 0; i < numMembers.Length; i++)
        {

            anglefractions[i] = ((endangle - startangle) * (((float)numMembers[i]) / ((float)sum)) - inbetweenangle);

        }

        angleCentroids[0] = 0;
        float currentcentroid = 0;
        for (int i = 1; i < numMembers.Length; i++)
        {
            currentcentroid += anglefractions[i] / 2 + anglefractions[i - 1] / 2 + inbetweenangle;
            angleCentroids[i] = currentcentroid;

        }
    }

    public struct CColor
    {
        public float x;
        public float y;
        public float z;
    }

    public void LoadColorPalette(string path = null)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            path = MyUtility.GetInputFile("json", GlobalProperties.Get.LastColorPaletteLoaded);
        }

        if (path == null || !File.Exists(path)) return;
        GlobalProperties.Get.LastColorPaletteLoaded = path;

        var str = File.ReadAllText(path);
        var palette = JsonConvert.DeserializeObject<Dictionary<string, CColor>>(str);

        foreach (var entry in palette)
        {
            var index = SceneManager.Get.AllIngredientNames.IndexOf(entry.Key);
            CPUBuffers.Get.IngredientsColors[index] = new Vector4(entry.Value.x / 255.0f, entry.Value.y / 255.0f, entry.Value.z / 255.0f);
        }

        GPUBuffers.Get.IngredientsColors.SetData(CPUBuffers.Get.IngredientsColors.ToArray());
    }

    public void SaveCurrentColorPalette()
    {
        var directory = Application.dataPath;

        if (Directory.Exists(Path.GetDirectoryName(GlobalProperties.Get.LastMembraneFileLoaded)))
        {
            directory = Path.GetDirectoryName(GlobalProperties.Get.LastMembraneFileLoaded);
        }

        //var path = EditorUtility.SaveFilePanel("Save current palette", directory, "palette_", "json");
        var path = "";

        var currentPalette = new Dictionary<string, CColor>();

        for (int i = 0; i < SceneManager.Get.AllIngredientNames.Count; i++)
        {
            var name = SceneManager.Get.AllIngredientNames[i];
            var color = CPUBuffers.Get.IngredientsColors[i];

            var ccolor = new CColor
            {
                x = Mathf.RoundToInt(color.x * 255),
                y = Mathf.RoundToInt(color.y * 255),
                z = Mathf.RoundToInt(color.z * 255)
            };

            currentPalette.Add(name, ccolor);
        }

        var str = JsonConvert.SerializeObject(currentPalette);
        File.WriteAllText(path, str);
    }

    public void UpdateColors(int[] groupIndices, float[] wedgeAngles, float[] wedgeRadii)
    {
        for (int i = 0; i < SceneManager.Get.IngredientGroups.Count; i++)
        {
            var index = groupIndices[i];
            CPUBuffers.Get.IngredientGroupsColorValues[index] = new Vector4(wedgeAngles[i], 75, 75);
            CPUBuffers.Get.IngredientGroupsColorRanges[index] = new Vector4(wedgeRadii[i], 0, 0);
        }

        GPUBuffers.Get.IngredientGroupsColorValues.SetData(CPUBuffers.Get.IngredientGroupsColorValues.ToArray());
        GPUBuffers.Get.IngredientGroupsColorRanges.SetData(CPUBuffers.Get.IngredientGroupsColorRanges.ToArray());
    }
}
