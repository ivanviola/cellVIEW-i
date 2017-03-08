using UnityEngine;

[ExecuteInEditMode]
public class GlobalProperties : MonoBehaviour
{
    // Declare the GlobalProperties as a singleton
    private static GlobalProperties _instance = null;
    public static GlobalProperties Get
    {
        get
        {
            if (_instance != null) return _instance;

            _instance = FindObjectOfType<GlobalProperties>();
            if (_instance == null)
            {
                var go = GameObject.Find("_GlobalProperties");
                if (go != null) DestroyImmediate(go);

                go = new GameObject("_GlobalProperties") { hideFlags = HideFlags.HideInInspector };
                _instance = go.AddComponent<GlobalProperties>();
            }
            return _instance;
        }
    }
    
    //*********************************//

    public string LastSceneLoaded;
    public string LastRecipeFileLoaded;
    public string LastMembraneFileLoaded;
    public string LastPositionsFileLoaded;
    public string LastColorPaletteLoaded;
    
    // Base settings
    public float Scale = 0.065f;
    public int ContourOptions;
    public float ContourStrength;
    public bool DebugObjectCulling;
    public bool EnableOcclusionCulling;

    //DNA/RNA settings
    public bool EnableDNAConstraints;
    public float DistanceContraint;
    public float AngularConstraint;

    // Lod infos
    public bool EnableLod;
    public float FirstLevelOffset = 0;
    public Vector4[] LodLevels = new Vector4[8];
}
