using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum RecorderState
{
    Recording,
    Paused,
    Stopped, 
    Replaying
}



//[ExecuteInEditMode]
public class InteractionRecorder : MonoBehaviour {

    public MainCameraController                 CameraController;
    [HideInInspector]
    public RecorderState                        currentState = RecorderState.Stopped;
    // UI
    public Dropdown                             RecordedFlythroughsDropdown;
    public Button                               RecButton;
    public Button                               StopButton;
    public Button                               PlayButton;
    //public Button                               StopReplayButton;
    public Text                                 replayTimeText;
    public Button                               LoadButton;
    public Button                               SaveButton;

    private List<Flythrough>                    recordedFlythroughs = new List<Flythrough>();
    private Flythrough                          currentFlythrough;
    // playback variables
    private float                               playbackStartTime; // current playback start time
    private Event                               nextEvent;
    private float                               nextEventTime;
    private Vector2                             nextEventMousePos;
    private Vector3                             nextEventCameraPos;
    private Quaternion                          nextEventCameraRot;
    private bool                                nextEventAlreadyServiced = true;

    private List<float>                         eventsTriggeredTimes = new List<float>();

    private static InteractionRecorder _instance = null;

    public static InteractionRecorder Get
    {
        get
        {
            if (_instance != null) return _instance;

            _instance = FindObjectOfType<InteractionRecorder>();
            if (_instance == null)
            {
                var go = GameObject.Find("_SceneManager");
                if (go != null) DestroyImmediate(go);

                go = new GameObject("_SceneManager");
                _instance = go.AddComponent<InteractionRecorder>();
                //_instance.hideFlags = HideFlags.HideInInspector;
            }
            
            return _instance;
        }
    }

    void OnEnable()
    {
        RecordedFlythroughsDropdown.ClearOptions();
    }
    

    void Update()
    {
        if (currentState == RecorderState.Replaying)
        {
            replayTimeText.text = "Replay started: " + playbackStartTime + ", current game time: " + Time.realtimeSinceStartup.ToString();

            if (nextEventAlreadyServiced)
            {
                if (!currentFlythrough.NothingToReplay())
                {
                    var e = currentFlythrough.GetNextEvent();
                    nextEvent = e.triggeredEvent;
                    nextEventTime = e.triggerTime;
                    nextEventAlreadyServiced = false;
                    nextEventMousePos = e.triggeredEvent.mousePosition;
                    nextEventCameraPos = e.cameraPosition;
                    nextEventCameraRot = e.cameraRotation;
                } 
                else
                {
                    StopReplaying();
                    return;
                }
            }
           
            NewSelectionManager.Instance.MousePosition = nextEventMousePos;
            replayTimeText.text += ", nextEventTime = " + nextEventTime;

            if (Time.realtimeSinceStartup >= playbackStartTime + nextEventTime)
            {
                var triggeredTime = Time.realtimeSinceStartup - playbackStartTime;
                eventsTriggeredTimes.Add(triggeredTime);
                var eventTriggeredDelay = triggeredTime - nextEventTime;
                //NewSelectionManager.Instance.MousePosition = 
                //CameraController.ProcessEvent(nextEvent, eventTriggeredDelay);

                // might look like shit but we'll see
                CameraController.transform.position = nextEventCameraPos;
                CameraController.transform.rotation = nextEventCameraRot;
                CameraController.MainCamera.transform.position = nextEventCameraPos;
                CameraController.MainCamera.transform.rotation = nextEventCameraRot;

                CameraController.ProcessEvent(nextEvent);
                nextEventAlreadyServiced = true;
            }
        }
    }

	void OnGUI()
    {
        if (currentState == RecorderState.Recording)
        {
            // var currentEvent = Event.current
            // var eventTime = ...
            // recordedEvents.add(pair(currentEvent, eventTime))

#if UNITY_EDITOR
            if (Event.current.type == EventType.Layout || Event.current.type == EventType.Repaint)
            {
                EditorUtility.SetDirty(this); // this is important, if omitted, "Mouse down" will not be display
            }
#endif

            if (Event.current.type == EventType.MouseUp)            // mouse button RELEASED...
            {
                if (Event.current.button == 0)                          // ... LEFT mouse button
                {
                    var pos = CameraController.MainCamera.transform.position;
                    var rot = CameraController.MainCamera.transform.rotation;
                    currentFlythrough.AddEvent(Event.current, Time.realtimeSinceStartup, pos, rot);
                }
            }
            else if (Event.current.type == EventType.MouseDrag)     // mouse DRAGGED
            {
                if (Event.current.button == 0)                          // ... LEFT mouse button
                {
                    var pos = CameraController.MainCamera.transform.position;
                    var rot = CameraController.MainCamera.transform.rotation;
                    currentFlythrough.AddEvent(Event.current, Time.realtimeSinceStartup, pos, rot);
                }
                else if (Event.current.button == 1)                     // ... RIGHT mouse button
                {
                    // TODO: DoZoomToCenter()
                }
                else if (Event.current.button == 2)                     // ... MIDDLE mouse button
                {
                    var pos = CameraController.MainCamera.transform.position;
                    var rot = CameraController.MainCamera.transform.rotation;
                    currentFlythrough.AddEvent(Event.current, Time.realtimeSinceStartup, pos, rot);
                }
            }
            else if (Event.current.type == EventType.ScrollWheel)   // mouse wheel SCROLLED
            {
                var pos = CameraController.MainCamera.transform.position;
                var rot = CameraController.MainCamera.transform.rotation;
                currentFlythrough.AddEvent(Event.current, Time.realtimeSinceStartup, pos, rot);
            }
        }

    }

    // These functions will be called upon certain button clicks
    public void StartRecording()
    {
        //Debug.Log("-------------------------------------------------------------------- Recording Started");
        currentFlythrough = new Flythrough("Flythrough " + recordedFlythroughs.Count, Time.realtimeSinceStartup);
        //currentFlythrough.CameraInitialTransform = CameraController.MainCamera.transform;
        currentFlythrough.CameraInitialPosition = CameraController.MainCamera.transform.position;
        currentFlythrough.CameraInitialRotation = CameraController.MainCamera.transform.rotation;

        RecButton.interactable = false;
        StopButton.interactable = true;

        SaveButton.interactable = false;
        LoadButton.interactable = false;
        PlayButton.interactable = false;

        Color col = replayTimeText.color;
        col.a = 1;
        replayTimeText.color = col;
        replayTimeText.text = "Recording..." + currentFlythrough.Name;


        currentState = RecorderState.Recording;
    }

    public void StopRecording()
    {
        //Debug.Log("-------------------------------------------------------------------- Recording Stopped");
        recordedFlythroughs.Add(currentFlythrough);

        RecButton.interactable = true;
        StopButton.interactable = false;

        SaveButton.interactable = true;
        LoadButton.interactable = true;
        PlayButton.interactable = true;

        Color col = replayTimeText.color;
        col.a = 0;
        replayTimeText.color = col;

        currentState = RecorderState.Stopped;
        RecordedFlythroughsDropdown.options.Add(new Dropdown.OptionData(currentFlythrough.Name));
        RecordedFlythroughsDropdown.value = RecordedFlythroughsDropdown.options.Count - 1; // set the last item as selected
    }

    public void ReplayFlythrough(int whichFlythrough)
    {
        eventsTriggeredTimes.Clear();
        currentState = RecorderState.Replaying;
        playbackStartTime = Time.realtimeSinceStartup;

        Flythrough replayedFlythrough = null;
        if (recordedFlythroughs.Count > 0)
        {
            replayedFlythrough = recordedFlythroughs[whichFlythrough];
        }

        if (replayedFlythrough != null)
        {
            currentFlythrough = replayedFlythrough;
            CameraController.ControlsDisabled = true;
            CameraController.MainCamera.transform.position = currentFlythrough.CameraInitialPosition;
            CameraController.MainCamera.transform.rotation = currentFlythrough.CameraInitialRotation;
            CameraController.transform.position = currentFlythrough.CameraInitialPosition;
            CameraController.transform.rotation = currentFlythrough.CameraInitialRotation;
          
        }
    }

    public void Stop()
    {
        //Debug.Log("-------------------------------------------------------------------- Replaying Stopped");

        currentState = RecorderState.Stopped;
        CameraController.ControlsDisabled = false;
        currentFlythrough.Reset(); // reset for future replaying 
        
    }

    public void StartReplaying()
    {
        //Debug.Log("-------------------------------------------------------------------- Replaying Started");
        eventsTriggeredTimes.Clear();
        currentState = RecorderState.Replaying;
        playbackStartTime = Time.realtimeSinceStartup;

        Flythrough replayedFlythrough = null;
        if (recordedFlythroughs.Count > 0)
        {
            replayedFlythrough = recordedFlythroughs[RecordedFlythroughsDropdown.value];
        }
        
        if (replayedFlythrough != null)
        {
            currentFlythrough = replayedFlythrough;
            CameraController.ControlsDisabled = true;
            CameraController.MainCamera.transform.position =  currentFlythrough.CameraInitialPosition;
            CameraController.MainCamera.transform.rotation =  currentFlythrough.CameraInitialRotation;
            CameraController.transform.position = currentFlythrough.CameraInitialPosition;
            CameraController.transform.rotation = currentFlythrough.CameraInitialRotation;

            //Debug.Log("Replaying of " + currentFlythrough.Name + " started.");

            StopButton.interactable = true;

            PlayButton.interactable = false;
            RecButton.interactable = false;
            SaveButton.interactable = false;
            LoadButton.interactable = false;

            Color col = replayTimeText.color;
            col.a = 1;
            replayTimeText.color = col;

        }
    }

    public void StopButtonAction()
    {
        if (currentState == RecorderState.Recording)
        {
            StopRecording();
        }
        else if (currentState == RecorderState.Replaying)
        {
            StopReplaying();
        }
    }

    public void StopReplaying()
    {
        //Debug.Log("-------------------------------------------------------------------- Replaying Stopped");

        currentState = RecorderState.Stopped;
        CameraController.ControlsDisabled = false;
        currentFlythrough.Reset(); // reset for future replaying 

        PlayButton.interactable = true;
        RecButton.interactable = true;
        //StopReplayButton.interactable = false;
        StopButton.interactable = false;

        SaveButton.interactable = true;
        LoadButton.interactable = true;

        Color col = replayTimeText.color;
        col.a = 0;
        replayTimeText.color = col;

       // Debug.Log("Replaying ended.");
    }


    /*
     * Ghost structures for data that need to be serialized 
     */
    public struct Vector3Serializable
    {
        public float x;
        public float y;
        public float z;
    }

    public struct Vector2Serializable
    {
        public float x;
        public float y;
    }

    public struct QuaternionSerializable
    {
        public float x;
        public float y;
        public float z;
        public float w;
    }

    public struct EventSerializable
    {
        public EventType type;
        public int button;
        public Vector2Serializable delta;
        public Vector2Serializable mousePosition;
    }

    public struct RecordedEventSerializable
    {
        public float triggerTime;
        //public Event triggeredEvent; // maybe Event will also have to have "ghost" struct
        public EventSerializable triggeredEvent; 
        public Vector3Serializable cameraPosition;
        public QuaternionSerializable cameraRotation;
    }

    public struct FlythroughSerializable
    {
        public float startTime;
        public string name;
        public List<RecordedEventSerializable> recordedActions;
        public Vector3Serializable cameraInitialPosition;
        public QuaternionSerializable cameraInitialRotation;
    }

    public void SaveFlythroughs()
    {
        if (recordedFlythroughs.Count < 1)
        {
            Debug.Log("ERROR: No Flythroughs to be saved!");
            return;
        }

        var path = Application.dataPath;
        path += "/../Data/RecordedFlythroughs/record1.json"; // I think this will not work when I run this from build

        var flythroughsToSerialize = new List<FlythroughSerializable>();
        foreach (var flythrough in recordedFlythroughs)
        {
            var item = new FlythroughSerializable();
            item.cameraInitialPosition = new Vector3Serializable() { x = flythrough.CameraInitialPosition.x, y = flythrough.CameraInitialPosition.y, z = flythrough.CameraInitialPosition.z };
            item.cameraInitialRotation = new QuaternionSerializable() { x = flythrough.CameraInitialRotation.x, y = flythrough.CameraInitialRotation.y,
                                                                        z = flythrough.CameraInitialRotation.z, w = flythrough.CameraInitialRotation.w };
            item.name = flythrough.Name;
            item.startTime = flythrough.StartTime;

            item.recordedActions = new List<RecordedEventSerializable>();
            foreach (var recEvent in flythrough.recordedActions)
            {
                var recEventToSer = new RecordedEventSerializable();
                recEventToSer.cameraPosition = new Vector3Serializable() { x = recEvent.cameraPosition.x, y = recEvent.cameraPosition.y, z = recEvent.cameraPosition.z };
                recEventToSer.cameraRotation = new QuaternionSerializable() { x = recEvent.cameraRotation.x, y = recEvent.cameraRotation.y,
                                                                              z = recEvent.cameraRotation.z, w = recEvent.cameraRotation.w };
                recEventToSer.triggerTime = recEvent.triggerTime;
                recEventToSer.triggeredEvent = new EventSerializable()
                {
                    type = recEvent.triggeredEvent.type,
                    button = recEvent.triggeredEvent.button,
                    delta = new Vector2Serializable() { x = recEvent.triggeredEvent.delta.x, y = recEvent.triggeredEvent.delta.y },
                    mousePosition = new Vector2Serializable() { x = recEvent.triggeredEvent.mousePosition.x, y = recEvent.triggeredEvent.mousePosition.y }
                };

                item.recordedActions.Add(recEventToSer);
            }

            flythroughsToSerialize.Add(item);
        }

        var serializedStr = JsonConvert.SerializeObject(flythroughsToSerialize, Formatting.Indented);
        File.WriteAllText(path, serializedStr);

        Debug.Log("--------------------------------------------------- Flythroughs have been successfully saved!");
    }

    public void LoadFlythroughsDefault()
    {
        var path = Application.dataPath;
        path += "/../Data/RecordedFlythroughs/record1.json"; // I think this will not work when I run this from build
        LoadFlythroughs(path);
    }

    public void LoadFlythroughs(string path)
    {
        //var path = Application.dataPath;
        //path += "/../Data/RecordedFlythroughs/record1.json"; // I think this will not work when I run this from build

        var str = File.ReadAllText(path);

        var loadedFlythroughs = JsonConvert.DeserializeObject<List<FlythroughSerializable>>(str);

        recordedFlythroughs.Clear();
        RecordedFlythroughsDropdown.ClearOptions();

        foreach (var flythrough in loadedFlythroughs)
        {
            var loadedFlythrough = new Flythrough(flythrough.name, flythrough.startTime);
            loadedFlythrough.CameraInitialPosition = new Vector3(flythrough.cameraInitialPosition.x, flythrough.cameraInitialPosition.y, flythrough.cameraInitialPosition.z);
            loadedFlythrough.CameraInitialRotation = new Quaternion(flythrough.cameraInitialRotation.x, flythrough.cameraInitialRotation.y, flythrough.cameraInitialRotation.z,
                                                                    flythrough.cameraInitialRotation.w);

            foreach (var action in flythrough.recordedActions)
            {
                var loadedAction = new RecordedEvent();
                loadedAction.cameraPosition = new Vector3(action.cameraPosition.x, action.cameraPosition.y, action.cameraPosition.z);
                loadedAction.cameraRotation = new Quaternion(action.cameraRotation.x, action.cameraRotation.y, action.cameraRotation.z, action.cameraRotation.w);
                loadedAction.triggerTime = action.triggerTime;
                loadedAction.triggeredEvent = new Event()
                {
                    type = action.triggeredEvent.type,
                    button = action.triggeredEvent.button,
                    delta = new Vector2(action.triggeredEvent.delta.x, action.triggeredEvent.delta.y),
                    mousePosition = new Vector2(action.triggeredEvent.mousePosition.x, action.triggeredEvent.mousePosition.y)
                };

                loadedFlythrough.recordedActions.Add(loadedAction);
            }

            recordedFlythroughs.Add(loadedFlythrough);
            RecordedFlythroughsDropdown.options.Add(new Dropdown.OptionData(flythrough.name));
        }

    }
}
