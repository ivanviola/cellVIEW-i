using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;

public class SimulationReader_ : MonoBehaviour
{
    private FileStream fs;

    public int numFrameMax = 1000;
    //public UIWidgets.Progressbar progressbar;
    //public UIWidgets.CenteredSlider slider;
    public Text labelFrame;
    public Slider slider;
    public Toggle play_pause;

    int numInstances = 18515;//23207;

    int positionValueOffset = 4;
    int rotationValueOffset = 9;

    int positionsArraySize;// = numInstances * positionValueOffset;
    int rotationArraySize;// = numInstances * rotationValueOffset;

    int frameHeaderOffset = 60;
    int positionOffset;// = positionsArraySize * sizeof(double);
    int rotationOffset;// = rotationArraySize * sizeof(double);
    int frameOffset;// = frameHeaderOffset + positionOffset + rotationOffset;
    
    private static double[] positions;// = new double[positionsArraySize];
    private static float[] positions_float;// = new float[positionsArraySize];
    private static double[] rotations;// = new double[rotationArraySize];
    private static float[] rotations_float;// = new float[rotationArraySize];

    private static Vector4[] quat_float;// = new Vector4[numInstances];//quaternion

    const int positionValueOffsetFixed = 4;
	const int rotationValueOffsetFixed = 4;

	int positionsArraySizeFixed;// = numInstances * positionValueOffsetFixed;
    int rotationArraySizeFixed;// = numInstances * rotationValueOffsetFixed;

    int frameHeaderOffsetFixed = 0;//60 original file
	int positionOffsetFixed;// = positionsArraySizeFixed * sizeof(float);
    int rotationOffsetFixed;// = rotationArraySizeFixed * sizeof(float);
    int frameOffsetFixed;// = frameHeaderOffsetFixed + positionOffsetFixed + rotationOffsetFixed;

    private static float[] positions_floatFixed;// = new float[positionsArraySizeFixed];
    private static float[] rotations_floatFixed;// = new float[rotationArraySizeFixed];

    public bool corrected = true;

    public void setupArray() {
        positionsArraySize  = numInstances * positionValueOffset;
        rotationArraySize  = numInstances * rotationValueOffset;
        positionOffset  = positionsArraySize * sizeof(double);
        rotationOffset  = rotationArraySize * sizeof(double);
        frameOffset  = frameHeaderOffset + positionOffset + rotationOffset;

        positions  = new double[positionsArraySize];
        positions_float  = new float[positionsArraySize];
        rotations  = new double[rotationArraySize];
        rotations_float  = new float[rotationArraySize];
        quat_float = new Vector4[numInstances];//quaternion

        positionsArraySizeFixed  = numInstances * positionValueOffsetFixed;
        rotationArraySizeFixed  = numInstances * rotationValueOffsetFixed;

        positionOffsetFixed = positionsArraySizeFixed * sizeof(float);
        rotationOffsetFixed  = rotationArraySizeFixed * sizeof(float);
        frameOffsetFixed  = frameHeaderOffsetFixed + positionOffsetFixed + rotationOffsetFixed;

        positions_floatFixed = new float[positionsArraySizeFixed];
        rotations_floatFixed  = new float[rotationArraySizeFixed];
    }

    void ReadFrame(int frame)
    {
        
        var positionByteArray = new byte[positionOffset];
        var rotationByteArray = new byte[rotationOffset];

        fs.Seek(frame * frameOffset + frameHeaderOffset, SeekOrigin.Begin);
        fs.Read(positionByteArray, 0, positionOffset);
        fs.Read(rotationByteArray, 0, rotationOffset);

        if (fs.Length == fs.Position)
        {
           Debug.Log("End of file has been reached.");
           numFrameMax = frame - 1;
        }

        Buffer.BlockCopy(positionByteArray, 0, positions, 0, positionByteArray.Length);
        Buffer.BlockCopy(rotationByteArray, 0, rotations, 0, rotationByteArray.Length);
        int count = 0;

        for(int i = 0; i < positions.Length; i++)
        {
            //skip lLDL 152+18 and iLDL 9586+18 
           // if (((count > 152) && (count <=( 152 + 18))) || (count > 9586 && count <= (9586 + 18)))
           // {
           //     continue;
           // }
            positions_float[i] = Convert.ToSingle(positions[i]);
            if (((i%4) == 0)&&(i!=0))
            {
                positions_float[i - 4] = -positions_float[i - 4];
                count += 1;
            }
        }

        //count = 0;
        int countQuat = 0;
        for (int i = 0; i < rotations.Length; i++)
        {
            //skip lLDL 152+18 and iLDL 9586+18 
            //if (((count > 152) && (count <= (152 + 18))) || (count > 9586 && count <= (9586 + 18)))
            //    continue;

            rotations_float[i] = Convert.ToSingle(rotations[i]);
            if (((i % 9) == 0) && (i != 0))
            {
                count += 1;
                Matrix4x4 m = Matrix4x4.identity;
                m.m00 = rotations_float[i - 9];
                m.m01 = rotations_float[i - 8];
                m.m02 = rotations_float[i - 7];
                m.m10 = rotations_float[i - 6];
                m.m11 = rotations_float[i - 5];
                m.m12 = rotations_float[i - 4];
                m.m20 = rotations_float[i - 3];
                m.m21 = rotations_float[i - 2];
                m.m22 = rotations_float[i - 1];
                //Debug.Log(m.ToString());
                //Debug.Log(m.transpose.ToString());
                var euler = MyUtility.euler_from_matrix(m.transpose);
                //Debug.Log(euler.ToString());
                Quaternion rotation = MyUtility.MayaRotationToUnity(euler);
                //Debug.Log(rotation.x.ToString()+ " " +rotation.y.ToString() + " "+ rotation.z.ToString() + " "+ rotation.w.ToString() );
                //break;
                quat_float[countQuat] = MyUtility.QuanternionToVector4(rotation);
                countQuat++;
            }
        }

        GPUBuffers.Get.ProteinInstanceRotations.SetData(quat_float);
        GPUBuffers.Get.ProteinInstancePositions.SetData(positions_float);
    }

	void ReadFrameFixed(int frame)
	{
		var positionByteArray = new byte[positionOffsetFixed];
		var rotationByteArray = new byte[rotationOffsetFixed];
		//Debug.Log (frame);
		//Debug.Log (frameHeaderOffsetFixed);
		//Debug.Log (frameOffsetFixed);
		//Debug.Log (frame * frameOffsetFixed + frameHeaderOffsetFixed);

		fs.Seek(frame * frameOffsetFixed + frameHeaderOffsetFixed, SeekOrigin.Begin);
		fs.Read(positionByteArray, 0, positionOffsetFixed);
		fs.Read(rotationByteArray, 0, rotationOffsetFixed);

		if (fs.Length == fs.Position)
		{
			Debug.Log("End of file has been reached.");
			numFrameMax = frame - 1;
		}

		Buffer.BlockCopy(positionByteArray, 0, positions_floatFixed, 0, positionByteArray.Length);
		Buffer.BlockCopy(rotationByteArray, 0, rotations_floatFixed, 0, rotationByteArray.Length);

		GPUBuffers.Get.ProteinInstanceRotations.SetData(rotations_floatFixed);
		GPUBuffers.Get.ProteinInstancePositions.SetData(positions_floatFixed);
	}

    string InputFilePath;// = @"C:\Users\ludovic\Documents\cellVIEW_bdbox\Data\packing_results\hivbd\pack_bloodhiv_bd_100.moldb";
    string DictionaryPath;// = @"C:\Users\ludovic\Documents\cellVIEW_bdbox\Data\packing_results\hivbd\pack_bloodhiv_bd.prm";
    string InputFilePathFixed;// = @"C:\Users\ludovic\Documents\cellVIEW_bdbox\Data\packing_results\test_full_final_nctr_fixed_tr.molb";
	//string InputFilePathFixed = @"C:\Users\ludovic\Documents\cellVIEW_bdbox\Data\packing_results\pack_hiv_from_ncfix_serialized.bin";
	//string InputFilePath = @"C:\Users\mathieu\Downloads\hivbd\test_full_final_nctr.molb";

    public static string GetProteinNameFromCellPackName(string name)
    {
        var split = name.Split(new[] { "__" }, StringSplitOptions.RemoveEmptyEntries);
        if (name.StartsWith("ext")){ return "root.cytoplasme." + split[1]; }
        if (name.StartsWith("surf_0")){ return "root.HIV1_envelope_Pack_145_0_2_0.surface." + split[1]; }
        if (name.StartsWith("surf_1")){ return "root.HIV1_capsid_3j3q_PackInner_0_1_0.surface." + split[1]; }
        if (name.StartsWith("int_0")){ return "root.HIV1_envelope_Pack_145_0_2_0.interior." + split[1]; }
        if (name.StartsWith("int_1")){ return "root.HIV1_capsid_3j3q_PackInner_0_1_0.interior." + split[1]; }

        return "";
    }

    void OnApplicationQuit() {
        fs.Close();
    }
    // Use this for initialization
    void Start ()
    {
        //InputFilePath = @"D:\Data\HIV\packing_results\pack_bloodhiv_bd.molb";

        DictionaryPath = @"D:\Data\HIV\pack_bloodhiv_bd.prm";
		InputFilePathFixed = @"D:\Data\HIV\pack_bloodhiv_bd_fixed.molb";
		//InputFilePathFixed = @"D:\Mathieu\Git\cellView_bdbox\trunk\Data\packing_results\pack_bloodhiv_bd_fixed.molb";

        Debug.Log("*****");
        Debug.Log("Loading order: " + DictionaryPath);
        if (!File.Exists(DictionaryPath)) throw new Exception("No file found at: " + DictionaryPath);

        var proteinInstanceInfo = new List<Vector4>();
        numInstances = 0;
        foreach (var line in File.ReadAllLines(DictionaryPath))
        {
            if (line.StartsWith("object"))
            {
                var split = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                var str_1 = split[1];
                var str_2 = GetProteinNameFromCellPackName(split[1]);
                var id = SceneManager.Get.ProteinIngredientNames.IndexOf(GetProteinNameFromCellPackName(split[1]));
                //if (str_1.Contains("LDL")) continue;
                Debug.Log(str_2);
                Debug.Log(id);
                if (id < 0)
                {
                    throw new Exception("Index not found");
                }

                var nb = int.Parse(split[2]);
                numInstances += nb;
                for (int i = 0; i < nb; i++)
                {
                    proteinInstanceInfo.Add(new Vector4(id, (int)InstanceState.Normal, 0));
                }
            }
        }
        setupArray();
        GPUBuffers.Get.ProteinInstancesInfo.SetData(proteinInstanceInfo.ToArray());
		if(corrected)
            fs = new FileStream(InputFilePathFixed, FileMode.Open);
        else
            fs = new FileStream(InputFilePath, FileMode.Open);
    }

    private bool pause = true;
    bool forceNextFrame = true;
    bool forcePreviousFrame;

    public int currentFrame;
    public int previsoucurrentFrame;
    int previousFrame;

    public int temporalResolution = 1;

    private float elapsedTimeSinceLastFrame = 0;
    public float threshold = 0;

    public void togglePause(bool toggle) {
        pause = !toggle;
    }

    // Update is called once per frame
    void Update ()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {

            pause = !pause;
            //play_pause.isOn = !pause;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) && pause) forceNextFrame = true;
        if (Input.GetKeyDown(KeyCode.LeftArrow) && pause) forcePreviousFrame = true;

        if (Time.realtimeSinceStartup - elapsedTimeSinceLastFrame > threshold)
        {
            ReadNextFrame();
            elapsedTimeSinceLastFrame = Time.realtimeSinceStartup;
        }
	}

    private void ReadNextFrame()
    {
        // If there is a new frame to display
        
        if (currentFrame != previousFrame)
        {
            //Debug.Log("Read frame: " + currentFrame);
            //ReadFrame(currentFrame);
            if (corrected)
                ReadFrameFixed(currentFrame);
            else
                ReadFrame(currentFrame);

            previousFrame = currentFrame;
        }
        
        if (!pause)
        {
            currentFrame += temporalResolution;
        }
        else if (forceNextFrame)
        {
            currentFrame += temporalResolution;
            forceNextFrame = false;
        }
        else if (forcePreviousFrame)
        {
            currentFrame -= temporalResolution;
            forcePreviousFrame = false;
        }

        forceNextFrame = forcePreviousFrame = false;

        if (currentFrame > numFrameMax - 1) currentFrame = 0;
        if (currentFrame < 0) currentFrame = numFrameMax - 1;
    }

    public float progress = 0;

    public void OnChangeSlider() {
        Debug.Log("slider_changed");
        pause = true;
        currentFrame = (int)(slider.value* numFrameMax);
        play_pause.isOn = !pause;
    }

    void OnGUI()
    {
        progress = (float)currentFrame / (float)numFrameMax;

        //progressbar.Max = numFrameMax;
        //progressbar.Value = currentFrame;

        //slider.LimitMax = numFrameMax;
        //if (!pause)
        //{
        //    slider.value = progress;
        //}
        //labelFrame.text = "Frame :"+currentFrame.ToString() + " / " + numFrameMax.ToString();
        //progressbar.
        GUI.contentColor = Color.white;
        //GUILayout.Label("Delta time: " + Time.deltaTime);
        //GUILayout.Label("Current frame: " + currentFrame);
        //GUILayout.Label("Current time: " + (double)currentFrame * TIME_STEP);

            float newProgress = GUI.HorizontalSlider(new Rect(25, Screen.height - 25, Screen.width - 50, 30), progress, 0.0F, 1.0F);

             if (progress != newProgress)
             {
                   currentFrame = (int)(((float)numFrameMax - 1.0f) * newProgress);
              }
    }
}
