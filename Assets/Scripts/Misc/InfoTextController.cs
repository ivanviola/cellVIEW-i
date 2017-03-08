using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using SimpleJSON;
using System.IO;

[ExecuteInEditMode]
public class InfoTextController : MonoBehaviour {

    public RawImage InfoTextParent;

    private Dictionary<string, string> infoTexts;
    private Dictionary<string, string> infoNames;

    private GameObject Heading = null;
    private GameObject Body = null;

    private static InfoTextController _instance = null;
    public static InfoTextController Get
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<InfoTextController>();
                if (_instance == null)
                {
                    var go = GameObject.Find("_InfoTextController");
                    if (go != null)
                        DestroyImmediate(go);

                    go = new GameObject("_InfoTextController"); //{ hideFlags = HideFlags.HideInInspector };
                    _instance = go.AddComponent<InfoTextController>();
                }
            }
            return _instance;
        }
    }

    public void OnEnable()
    {
        if (Heading == null || Body == null)
        {
            foreach (Transform child in InfoTextParent.transform) // this is a very stupid way to get children of a parent GameObject...
            {
                if (child.name == "Heading") Heading = child.gameObject;
                if (child.name == "Body") Body = child.gameObject;
            }
        }

        LoadInfosAndNames();
        //HideInfo();
    }

    public void Start()
    {
        HideInfo();
    }

    private void LoadInfosAndNames()
    {
        infoTexts = new Dictionary<string, string>();
        infoNames = new Dictionary<string, string>();

        string strContent = "";
        // TODO parse JSON file with descriptions
        try
        {
            strContent = File.ReadAllText(Application.dataPath + "/Resources/compartment-descriptions.json"); // ReadAllText vs ReadAllLines????
        } catch
        {
            Debug.LogError("Could NOT load up description file!");
            return;
        }
        

        var content = JSON.Parse(strContent);
        Debug.Log(content["key"]["description"]);

        foreach (var key in content.GetAllKeys())
        {
            infoTexts.Add(key, content[key]["descr"]);
            infoNames.Add(key, content[key]["name"]);
        }
    }

    public void ShowInfoFor(string path)
    {
        LookUpInfoText(path);
        SetInfoTextVisibility(true);
        //AdjustSizeAndPosition(); // not needed, I accomplished it by using UI tools
    }

    public void ShowInfoForInstance(int selectedInstanceId)
    {
        if (selectedInstanceId == -123)         // RNA
        {
            ShowInfoFor("root.interior2_HIV_RNA");
        }
        else if (selectedInstanceId >= 100000)  // Lipid
        {
            // there are only two lipid types so it should be fairly easy
            var ingrId = CPUBuffers.Get.LipidInstanceInfos[selectedInstanceId - 100000].x;
            //Debug.Log("ShowInfoForInstance: Lipid, id = " + ingrId);
            var ingredientPath = SceneManager.Get.AllIngredientNames[(int)ingrId];
            //Debug.Log("ShowInfoFor path = " + ingredientPath);
            ShowInfoFor(ingredientPath);
        }
        else if (selectedInstanceId >= 0)       // Protein
        {
            var ingrId = CPUBuffers.Get.ProteinInstanceInfos[selectedInstanceId].x;
            //Debug.Log("ShowInfoForInstance: Protein, id = " + ingrId);
            var ingredientPath = SceneManager.Get.ProteinIngredientNames[(int)ingrId];
            ShowInfoFor(ingredientPath);
        }
        else                                    // Nothing selected (-1)
        {
            HideInfo();
            return;
        }

        //var ingredientId = CPUBuffers.Get.ProteinInstanceInfos[selectedInstanceId];

    }

    public void HideInfo()
    {
        SetInfoTextVisibility(false);
    }

    private void AdjustSizeAndPosition()
    {
        // TODO:
    }

    /// <summary>
    /// Looks up text to show for a certain compartment or protein type
    /// </summary>
    /// <param name="path">hierarchy path to the element that we want to set into text for</param>
    private void LookUpInfoText(string path)
    {
        var nameToShow = "Dummy Name";
        var descriptionToShow = "Dummy Description";

        if (infoNames.ContainsKey(path)) nameToShow = infoNames[path];
        if (infoTexts.ContainsKey(path)) descriptionToShow = infoTexts[path];

        var headingText = Heading.GetComponent<Text>();
        headingText.text = nameToShow;
        var bodyText = Body.GetComponent<Text>();
        bodyText.text = descriptionToShow;
    }

    /// <summary>
    /// Method for showing and hiding info text area
    /// </summary>
	public void SetInfoTextVisibility(bool visible)
    {
        //GameObject heading = null;
        //GameObject body = null;
        //foreach (Transform child in InfoTextParent.transform) // this is a very stupid way to get children of a parent GameObject...
        //{
        //    if (child.name == "Heading") heading = child.gameObject;
        //    if (child.name == "Body") body = child.gameObject;
        //}

        if (visible)
        {
            var bg = InfoTextParent.GetComponent<RawImage>();
            bg.color = new Color(bg.color.r, bg.color.g, bg.color.b, 0.235f);
            var elem = Heading.GetComponent<Text>();
            elem.color = new Color(elem.color.r, elem.color.g, elem.color.b, 1.0f);
            elem = Body.GetComponent<Text>();
            elem.color = new Color(elem.color.r, elem.color.g, elem.color.b, 1.0f);
        }
        else
        {
            var bg = InfoTextParent.GetComponent<RawImage>();
            bg.color = new Color(bg.color.r, bg.color.g, bg.color.b, 0.0f);
            var elem = Heading.GetComponent<Text>();
            elem.color = new Color(elem.color.r, elem.color.g, elem.color.b, 0.0f);
            elem = Body.GetComponent<Text>();
            elem.color = new Color(elem.color.r, elem.color.g, elem.color.b, 0.0f);
        }
    }

    // DEBUG!!!!!!!!!!!!!!!!!!!!!!!!!
    public static void GenerateJSON()
    {
        var hierarchy = SceneManager.Get.SceneHierarchy;
        var path = Application.dataPath + "/Resources/compartment-descriptions.json";
        string jsonOutput = "{";

        foreach (var ingredientPath in hierarchy)
        {
            jsonOutput += "\"" + ingredientPath + "\"" + ":{";
            jsonOutput += "\"name\":\"Some Name\",\"descr\":\"This is a description\"}";

            if (hierarchy.IndexOf(ingredientPath) != hierarchy.Count - 1) jsonOutput += ",";
        }

        jsonOutput += "}";
        File.WriteAllText(path, jsonOutput);
    }
}
