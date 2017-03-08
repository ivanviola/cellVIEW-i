using UnityEngine;
using System.Collections.Generic;

public struct RecordedEvent
{
    public float                   triggerTime;
    public Event                   triggeredEvent;
    public Vector3                 cameraPosition;
    public Quaternion              cameraRotation;
}

/// <summary>
/// This represents one recorded fly-through.
/// </summary>
public class Flythrough {

    public float                                    StartTime;
    public string                                   Name;
    
    public Vector3                                  CameraInitialPosition
    {
        get
        {
            return cameraInitialPosition;
        }
        set
        {
            cameraInitialPosition = value;
        }
    }

    public Quaternion                               CameraInitialRotation
    {
        get
        {
            return cameraInitialRotation;
        }
        set
        {
            cameraInitialRotation = value;
        }
    }

    public List<float> EventTriggerTimes
    {
        get
        {
            return eventTriggerTimes;
        }
    }
    
    public List<RecordedEvent>                     recordedActions = new List<RecordedEvent>();
    //private List<RecordedEvent>                     recordedActions = new List<RecordedEvent>();
    private List<float>                             eventTriggerTimes = new List<float>();
    private int                                     nextEventIndex = 0; // when replaying
    private Vector3                                 cameraInitialPosition;
    private Quaternion                              cameraInitialRotation;
   

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name"></param>
    /// <param name="startTime"></param>
    public Flythrough(string name, float startTime)
    {
        this.Name = name;
        this.StartTime = startTime;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e">Event to be added</param>
    /// <param name="t">Time since startup (will be subtracted to get time related to StartTime of Flythrough</param>
    public void AddEvent(Event e, float t, Vector3 pos, Quaternion rot)
    {
        var savedEvent = new Event();
        savedEvent.type = e.type;
        savedEvent.button = e.button;
        savedEvent.delta = new Vector2(e.delta.x, e.delta.y);
        savedEvent.mousePosition = new Vector2(e.mousePosition.x, e.mousePosition.y);
        
        RecordedEvent ev;
        ev.triggeredEvent = savedEvent;
        ev.triggerTime = t - StartTime;
        ev.cameraPosition = pos;
        ev.cameraRotation = rot;

        recordedActions.Add(ev);
        //recordedActions.Add(new KeyValuePair<Event, float>(savedEvent, t - StartTime));
        eventTriggerTimes.Add(t - StartTime);
        //Debug.Log("Recording Event: " + savedEvent);
    }

    //public KeyValuePair<Event, float> GetNextEvent()
    public RecordedEvent GetNextEvent()
    {
        var index = nextEventIndex;

        if (index < recordedActions.Count)
        {
            nextEventIndex += 1;
            return recordedActions[index];
        }
        
        return new RecordedEvent(); // this is a litle bit weird solution
    }

    public bool NothingToReplay()
    {
        if (nextEventIndex >= recordedActions.Count) return true;

        return false;
    }

    public void Reset()
    {
        nextEventIndex = 0;
    }
}
