//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using SimpleJSON;

//using UnityEngine;
//#if UNITY_EDITOR
//using UnityEditor;
//#endif


//public static class CellPackLoader
//{
//	public static int current_color;
//	public static List<Vector3> ColorsPalette;
//	public static List<Vector3> ColorsPalette2;
//	public static Dictionary<int,List<int>> usedColors;

//    public static void ReloadCellPackResults()
//    {
//        if (File.Exists(GlobalProperties.Get.LastSceneLoaded))
//        {
//            LoadCellPackResults(GlobalProperties.Get.LastSceneLoaded);
//        }
//        else
//        {
//            LoadCellPackResults();
//        }
//    }

   

//    public static void LoadCellPackResults(string path = null)
//    {
//        if (string.IsNullOrEmpty(path))
//        {
//            #if UNITY_EDITOR
//                Debug.Log("Loading");
//                var directory = "";

//                if (string.IsNullOrEmpty(GlobalProperties.Get.LastSceneLoaded) || !Directory.Exists(Path.GetDirectoryName(GlobalProperties.Get.LastSceneLoaded)))
//                {
//                    directory = Application.dataPath;
//                }
//                else
//                {
//                    directory = Path.GetDirectoryName(GlobalProperties.Get.LastSceneLoaded);
//                }

//                path = EditorUtility.OpenFilePanel("Select .json", directory, "json");
//            #endif
//        }
        
//        if (string.IsNullOrEmpty(path)) return;

//        SceneManager.Get.SceneName = Path.GetFileNameWithoutExtension(path);

//        GlobalProperties.Get.LastSceneLoaded = path;
//        LoadIngredients(path);

//        Debug.Log("*****");
//        Debug.Log("Total protein atoms number: " + SceneManager.Get.TotalNumProteinAtoms);

//        // Upload scene data to the GPU
//        CPUBuffers.Get.CopyDataToGPU();
//    }

//    public static void LoadIngredients(string recipePath)
//    {
//        Debug.Log("*****");
//        Debug.Log("Loading scene: " + recipePath);
        
//        var cellPackSceneJsonPath = recipePath;//Application.dataPath + "/../Data/HIV/cellPACK/BloodHIV1.0_mixed_fixed_nc1.json";
//        if (!File.Exists(cellPackSceneJsonPath)) throw new Exception("No file found at: " + cellPackSceneJsonPath);

//        var resultData = MyUtility.ParseJson(cellPackSceneJsonPath);

//        //we can traverse the json dictionary and gather ingredient source (PDB,center), sphereTree, instance.geometry if we want.
//        //the recipe is optional as it will gave more information than just the result file.

//        //idea: use secondary color scheme for Compartments, and analogous color for ingredient from the recipe baseColor
//        current_color = 0;
//        //first grab the total number of object
//        int nIngredients = 0;
//        if (resultData["cytoplasme"] != null) nIngredients += resultData["cytoplasme"]["ingredients"].Count;

//        for (int i = 0; i < resultData["Compartments"].Count; i++)
//        {
//            nIngredients += resultData["Compartments"][i]["interior"]["ingredients"].Count;
//            nIngredients += resultData["Compartments"][i]["surface"]["ingredients"].Count;
//        }
//        //generate the palette
//        //ColorsPalette   = ColorGenerator.Generate(nIngredients).Skip(2).ToList(); 
//        ColorsPalette = ColorGenerator.Generate(8).Skip(2).ToList();//.Skip(2).ToList();
//        List<Vector3> startKmeans = new List<Vector3>(ColorsPalette);
//        //paletteGenerator.initKmeans (startKmeans);

//        usedColors = new Dictionary<int, List<int>>();
//        ColorsPalette2 = ColorPaletteGenerator.generate(
//                6, // Colors
//                ColorPaletteGenerator.testfunction,
//                false, // Using Force Vector instead of k-Means
//                50 // Steps (quality)
//                );
//        // Sort colors by differenciation first
//        //ColorsPalette2 = paletteGenerator.diffSort(ColorsPalette2);
//        //check if cytoplasme present

//        Color baseColor = new Color(1.0f, 107.0f / 255.0f, 66.0f / 255.0f);
//        if (resultData["cytoplasme"] != null)
//        {
//            usedColors.Add(current_color, new List<int>());
//            baseColor = new Color(1.0f, 107.0f / 255.0f, 66.0f / 255.0f);
//            AddRecipeIngredients(resultData["cytoplasme"]["ingredients"], baseColor, "root", "cytoplasme");
//            current_color += 1;
//        }

//        Debug.Log(resultData["compartments"].Count);

//        for (int i = 0; i < resultData["compartments"].Count; i++)
//        {
//            Debug.Log(resultData["compartments"][i].Value);

//            var compartmentName = resultData["compartments"].GetKey(i);

//            baseColor = new Color(148.0f / 255.0f, 66.0f / 255.0f, 255.0f / 255.0f);
//            usedColors.Add(current_color, new List<int>());

//            AddRecipeIngredients(resultData["compartments"][i]["interior"]["ingredients"], baseColor, "root", compartmentName, "interior");

//            current_color += 1;
//            baseColor = new Color(173.0f / 255.0f, 255.0f / 255.0f, 66.0f / 255.0f);
//            usedColors.Add(current_color, new List<int>());

//            AddRecipeIngredients(resultData["compartments"][i]["surface"]["ingredients"], baseColor, "root", compartmentName, "surface");
//            current_color += 1;
//        }
//    }

//	public static void AddRecipeIngredients(JSONNode recipeDictionary, Color baseColor, params string[] pathElements)
//    {
//		for (int j = 0; j < recipeDictionary.Count; j++)
//		{
//            if (recipeDictionary[j]["nbCurve"] != null)
//            {
//                AddCurveIngredients(recipeDictionary[j], pathElements);
//            }
//            else
//            {
//                AddProteinIngredientToCPUBuffer(recipeDictionary[j], pathElements);
//            }
//        }
//	}

//    public static void AddProteinIngredientToCPUBuffer(JSONNode ingredientDictionary, params string[] pathElements)
//    {
//        var name = ingredientDictionary["name"];
//        var path = MyUtility.GetUrlPath(pathElements.ToList(), name);
//        var biomt = (bool)ingredientDictionary["source"]["biomt"].AsBool;
//        var center = (bool)ingredientDictionary["source"]["transform"]["center"].AsBool;
//        var pdbName = ingredientDictionary["source"]["pdb"].Value.Replace(".pdb", "");

//        Debug.Log("*****");
//        Debug.Log("Adding ingredient: " + path);

//        // Load atom set from pdb file
//        var atomSet = new List<Atom>();
//        var lodProxies = new List<List<Vector4>>();

//        var oneLOD = false;
//        var containsACarbonOnly = false;

//        //if (biomt) return;

//        if ((pdbName == "") || (pdbName == "null") || (pdbName == "None") || pdbName.StartsWith("EMDB"))
//        {
//            var filePath = PdbLoader.GetFile(PdbLoader.DefaultPdbDirectory, ingredientDictionary["name"], "bin");
//            if (File.Exists(filePath))
//            {
//                atomSet = new List<Atom>();
//                var points = MyUtility.ReadBytesAsFloats(filePath);
//                for (var i = 0; i < points.Length; i += 4)
//                {
//                    var currentAtom = new Atom();
//                    currentAtom.position = new Vector3(points[i], points[i + 1], points[i + 2]);
//                    currentAtom.radius = points[i + 3];
//                    currentAtom.symbolId = -1;
//                    atomSet.Add(currentAtom);
//                }

//                containsACarbonOnly = true;
//                oneLOD = true;
//            }

//            // If the set is empty return
//            if (atomSet.Count == 0) throw new Exception("Atom set empty: " + name);
//        }
//        else
//        {
//            // Load atom set from pdb file
//            atomSet = PdbLoader.LoadAtomSet(pdbName);

//            // If the set is empty return
//            if (atomSet.Count == 0) throw new Exception("Atom set empty: " + name);

//            //atomSpheres = AtomHelper.GetAtomSpheres(atomSet);
//            containsACarbonOnly = AtomHelper.ContainsCarbonAlphaOnly(atomSet);
//            if (containsACarbonOnly) AtomHelper.OverwriteRadii(ref atomSet, 3);
//        }

//        // Define cluster decimation levels
//        var clusterLevelFactors = new List<float>() { 0.15f, 0.10f, 0.05f };
//        if (containsACarbonOnly) clusterLevelFactors = new List<float>() { 1, 1, 1 };
//        if (oneLOD) clusterLevelFactors = new List<float>() { 1, 1, 1 };

//        // Compute lod proxies
//        if (!biomt)
//        {
//            // Center atoms before computing the lod proxies
//            AtomHelper.CenterAtoms(ref atomSet);

//            var atomSpheres = AtomHelper.GetAtomSpheres(atomSet);
//            lodProxies = AtomHelper.ComputeLodProxies(atomSpheres, clusterLevelFactors);            
//        }
//        else
//        {
//            var atomSpheres = AtomHelper.GetAtomSpheres(atomSet);
//            var biomtTransforms = PdbLoader.LoadBiomtTransforms(pdbName);

//            // Compute centered lod proxies
//            lodProxies = AtomHelper.ComputeLodProxiesBiomt(atomSpheres, biomtTransforms, clusterLevelFactors);
            
//            // Assemble the atom set from biomt transforms and center
//            atomSet = AtomHelper.BuildBiomt(atomSet, biomtTransforms);
            
//            var centerPosition = AtomHelper.ComputeBounds(atomSet).center;

//            // Center atoms
//            AtomHelper.OffsetAtoms(ref atomSet, centerPosition);

//            // Center proxies
//            for(int i = 0; i < lodProxies.Count; i++ )
//            {
//                var t = lodProxies[i];
//                AtomHelper.OffsetSpheres(ref t, centerPosition);
//            }
//        }
        
//        var color = new Color(UnityEngine.Random.Range(0f,1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
//        SceneManager.Get.AddProteinIngredientToCPUBuffer(path, atomSet, color, lodProxies);

//        for (int k = 0; k < ingredientDictionary["results"].Count; k++)
//        {
//            var p = ingredientDictionary["results"][k][0];
//            var r = ingredientDictionary["results"][k][1];

//            var position = new Vector3(-p[0].AsFloat, p[1].AsFloat, p[2].AsFloat);
//            var rotation = new Quaternion(r[0].AsFloat, r[1].AsFloat, r[2].AsFloat, r[3].AsFloat);

//            var mat = MyUtility.quaternion_matrix(rotation);
//            var euler = MyUtility.euler_from_matrix(mat);
//            rotation = MyUtility.MayaRotationToUnity(euler);

//            SceneManager.Get.AddProteinInstance(path, position, rotation);
//        }
        
//        Debug.Log("Ingredient added : " + path);
//    }

//    public static void AddCurveIngredients(JSONNode ingredientDictionary , params string[] pathElements)
//    {
//        var name = ingredientDictionary["name"];
//        var path = MyUtility.GetUrlPath(pathElements.ToList(), name);
//        var numCurves = ingredientDictionary["nbCurve"].AsInt;
//        var pdbName = ingredientDictionary["source"]["pdb"].Value.Replace(".pdb", "");

//        SceneManager.Get.AddCurveIngredient(path, pdbName);
        
//        for (int i = 0; i < numCurves; i++)
//        {
//            //if (i < nCurve-10) continue;
//            var controlPoints = new List<Vector4>();
//            if (ingredientDictionary["curve" + i.ToString()].Count < 4) continue;

//            for (int k = 0; k < ingredientDictionary["curve" + i.ToString()].Count; k++)
//            {
//                var p = ingredientDictionary["curve" + i.ToString()][k];
//                controlPoints.Add(new Vector4(-p[0].AsFloat, p[1].AsFloat, p[2].AsFloat, 1));
//            }

//            SceneManager.Get.AddCurveInstance(path, controlPoints);
//            //break;
//        }

//        Debug.Log("*****");
//        Debug.Log("Added curve ingredient: " + path);
//        Debug.Log("Num curves: " + numCurves);
//    }
	
//    public static void DebugMethod()
//    {
//        Debug.Log("Hello World");
//    }


//    public static void LoadMembrane()
//    {
//#if UNITY_EDITOR
//        Debug.Log("Loading");
//        var directory = "";

//        if (string.IsNullOrEmpty(GlobalProperties.Get.LastSceneLoaded) || !Directory.Exists(Path.GetDirectoryName(GlobalProperties.Get.LastSceneLoaded)))
//        {
//            directory = Application.dataPath;
//        }
//        else
//        {
//            directory = Path.GetDirectoryName(GlobalProperties.Get.LastSceneLoaded);
//        }

//        var path = EditorUtility.OpenFilePanel("Select .mbr", directory, "mbr");
//        if (string.IsNullOrEmpty(path)) return;

//        SceneManager.Get.AddMembrane(path, Vector3.zero, Quaternion.identity);

//        // Upload scene data to the GPU
//        CPUBuffers.Get.CopyDataToGPU();
//#endif
//    }
//}
