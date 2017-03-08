//using UnityEngine;
//using System.Collections;
//using System.Xml.Serialization;
//using System.IO;
//using System.Collections.Generic;
//using System;

//public class tmp_util : MonoBehaviour {

//    private XmlSerializer serializer;
//    public string path = "C:\\proteins.xml";
//    private FileStream stream;

//	// Use this for initialization
//	void Start ()
//    {
//	}
	

//    public void ExportProteinSettings()
//    {
//        try
//        {
//            CutParametersContainer exportParams = new CutParametersContainer();

//            foreach (CutObject cuto in SceneManager.Get.CutObjects)
//            {
//                CutObjectProperties props = new CutObjectProperties();
//                props.ProteinTypeParameters = cuto.IngredientCutParameters;
//                props.Inverse = cuto.Inverse;
//                props.CutType = (int)cuto.CutType;
//                props.rotation = cuto.transform.rotation;
//                props.position = cuto.transform.position;
//                props.scale = cuto.transform.localScale;
//                exportParams.CutObjectProps.Add(props);            
//            }

//            ////write
//            serializer = new XmlSerializer(typeof(CutParametersContainer));
//            stream = new FileStream(path, FileMode.Create);
//            serializer.Serialize(stream, exportParams);
//            stream.Close();
//        }
//        catch(Exception e)
//        {
//            Debug.Log("export failed: " + e.ToString());
//            return;
//        }

//        Debug.Log("exported cutobject settings to " + path);
//    }

//    public void ImportProteinSettings()
//    {
//        try
//        {
//            ////read
//            serializer = new XmlSerializer(typeof(CutParametersContainer));
//            stream = new FileStream(path, FileMode.Open);
//            CutParametersContainer importParams = serializer.Deserialize(stream) as CutParametersContainer;
//            stream.Close();

//            for (int i = 0; i < importParams.CutObjectProps.Count && i < SceneManager.Get.CutObjects.Count; i++)
//            {
//                SceneManager.Get.CutObjects[i].IngredientCutParameters = importParams.CutObjectProps[i].ProteinTypeParameters;
//                SceneManager.Get.CutObjects[i].Inverse = importParams.CutObjectProps[i].Inverse;
//                SceneManager.Get.CutObjects[i].CutType = (CutType) importParams.CutObjectProps[i].CutType;

//                //restore transform info
//                SceneManager.Get.CutObjects[i].transform.rotation = importParams.CutObjectProps[i].rotation;
//                SceneManager.Get.CutObjects[i].transform.position = importParams.CutObjectProps[i].position;
//                SceneManager.Get.CutObjects[i].transform.localScale = importParams.CutObjectProps[i].scale;
//            }
//        }
//        catch(Exception e)
//        {
//            Debug.Log("import failed: " + e.ToString());
//            return;
//        }

//        Debug.Log("imported cutobject settings from " + path);
//    }

//    [XmlRoot("CutParametersContainer")]
//    public class CutParametersContainer
//    {
//        [XmlArray("List of ParamSets")]
//        [XmlArrayItem("ParamSet")]
//        public List<CutObjectProperties> CutObjectProps = new List<CutObjectProperties>();
//        //public List<List<CutParameters>> ProteinTypeParameters = new List<List<CutParameters>>();
//    }

//    public class CutObjectProperties
//    {
//        public List<CutParameters> ProteinTypeParameters = new List<CutParameters>();
//        public bool Inverse;
//        public int CutType;
//        public Quaternion rotation;
//        public Vector3 position;
//        public Vector3 scale;

//    }
//}
