using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum CameraState
{
    Free = 0, // Camera is free and can be positioned using tumble (arcball), pan and zoom (dolly) operations
    Focus = 1 // Camera is focused on one object (protein, compartment, ...) and user can tumble (arcball, orbit) around this object and also zoom in/out (dolly in/out)
}

[ExecuteInEditMode]
public class MainCameraController : MonoBehaviour
{
    public Camera MainCamera;

    public float DefaultDistance = 5.0f;
    public float DefaultProteinFocusDistance = 15.0f;

    public float ScrollingSpeed = 3.0f;
    public float PanningSpeed = 5.0f;
    public float OrbitingSpeed = 1.0f;

    public Vector4 CameraDefaultPosition;
    public Quaternion CameraDefaultRotation;

    public float Distance = 25; // this is the distance that we are trying to interpolate to

    [Range(0.01f,1)]
    public float Smoothing = 0.25f;
    
    public Transform TargetTransform; // this is not used anymore but it's used somewhere else in the code that is also not used so I'm just gonna keep it here for now and get rid of it later.

    //public Text FrameTimeText;

    /*****/
    private CameraState _cameraState = CameraState.Free;

    private float _deltaTime = 0;
    private float _lastUpdateTime = 0;


    // new implementation variables
    public bool ControlsDisabled { get; set; }
    public bool ShouldBlockInteraction { get; set; }
    
    private bool _leftDragging = false;
    private Vector3 _currentRotationPivot;

    // Zooming to cursor
    private Vector3 _currentMousePosition = new Vector3();
    

    // FRAMERATE INDEPENDENT MOVEMENT
    private float _lerpTime = 1f;
    // Panning
    private Vector3 _panStartPosition = new Vector3();
    private Vector3 _panEndPosition = new Vector3();
    private Vector3 _lastFramePosition = new Vector3();
    private float _panCurrentLerpTime = 0.0f;

    // zooming
    private float _zoomEndAmount = 0;
    private float _lastFrameZoomAmount = 0;
    private float _zoomCurrentLerpTime = 0.0f;

    // orbiting
    private float _orbitCurrentLerpTime = 0.0f;
    private float _orbitEndAngleX = 0.0f;
    private float _orbitEndAngleY = 0.0f;
    private float _lastFrameAngleX = 0f;
    private float _lastFrameAngleY = 0f;

    // Focus/Freecam transition
    private Vector3 _moveBy = new Vector3();
    

    /*****/

    void OnEnable()
    {
#if UNITY_EDITOR
        if (!EditorApplication.isPlaying)
        {
            EditorApplication.update += Update;
        }
        
#endif
    }

    void OnDisable()
    {
#if UNITY_EDITOR
        if (!EditorApplication.isPlaying)
        {
            EditorApplication.update = null;
        }
#endif
    }

    /*****/

    /*
     * In Update function we take care of smooth interpolation
     */
    void Update()
    {
        if (MainCamera == null && Camera.main == null) return;
        if (MainCamera == null && Camera.main != null) MainCamera = Camera.main;

        _deltaTime = Time.realtimeSinceStartup - _lastUpdateTime;
        _lastUpdateTime = Time.realtimeSinceStartup;

        //FrameTimeText.text = "deltaTime = " + _deltaTime + ", lastUpdateTime = " + _lastUpdateTime;

        // just a little scripting for testing
        //_leftDragging = true;
        //_currentRotationPivot = new Vector3(0,0,0);
        //DoOrbitingAroundCursor(5, 0);


        Vector3 v = transform.position - _currentRotationPivot;

        // -----------------------------------------
        // ORBITING AROUND CURSOR - interpolating version
        // -----------------------------------------
        _orbitCurrentLerpTime += _deltaTime;
        if (_orbitCurrentLerpTime > _lerpTime) _orbitCurrentLerpTime = _lerpTime;
        float perc = _orbitCurrentLerpTime / _lerpTime;

        var currentAngleX = Mathf.Lerp(0, _orbitEndAngleX, EaseOutExp(perc));
        var currentAngleY = Mathf.Lerp(0, _orbitEndAngleY, EaseOutExp(perc));

        var rotIncrementX = currentAngleX - _lastFrameAngleX;
        var rotIncrementY = currentAngleY - _lastFrameAngleY;

        //Debug.Log("rotIncrementX = " + rotIncrementX + ", rotIncrementY = " + rotIncrementY);

        _lastFrameAngleX = currentAngleX;
        _lastFrameAngleY = currentAngleY;

        //Quaternion horizRot = Quaternion.Euler(0, rotIncrementY, 0);
        Quaternion horizRot = Quaternion.AngleAxis(rotIncrementY, Vector3.up);
        Quaternion vertRot = Quaternion.AngleAxis(rotIncrementX, transform.right);
        //Quaternion newRot = vertRot * horizRot;
        Quaternion newRot = horizRot * vertRot; // when it was the other way around it was causing an accumulated error in z-axis rotation
        var newPos = _currentRotationPivot + newRot * v;

        transform.position = newPos;
        transform.rotation = newRot * transform.rotation;


        // --------------------------------------------
        // PANNING - interpolating version
        // --------------------------------------------
        _panCurrentLerpTime += _deltaTime;
        if (_panCurrentLerpTime > _lerpTime) _panCurrentLerpTime = _lerpTime;
        perc = _panCurrentLerpTime / _lerpTime;

        var currentFramePosition = Vector3.Lerp(_panStartPosition, _panEndPosition, EaseOutExp(perc));
        Vector3 frameOffset = currentFramePosition - _lastFramePosition;
        _lastFramePosition = currentFramePosition;

        transform.position += frameOffset;

        // --------------------------------------------
        // ZOOMING TO CURSOR - interpolating version
        // --------------------------------------------
        _zoomCurrentLerpTime += _deltaTime;
        if (_zoomCurrentLerpTime > _lerpTime) _zoomCurrentLerpTime = _lerpTime; // basically just clamping it to <=1 
        perc = _zoomCurrentLerpTime / _lerpTime;
        
        float currentZoom = Mathf.Lerp(0, _zoomEndAmount, EaseOutExp(perc)); // currentZoom is in <0, _zoomEndAmount> based on current progress in the interpolation
        float change = currentZoom - _lastFrameZoomAmount;
        _lastFrameZoomAmount = currentZoom;

        float dummyDistance = 50f;
        Vector3 mousePos = _currentMousePosition;

        Vector3 mouseWorldPosBefore = MainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, MainCamera.pixelHeight - mousePos.y, dummyDistance));

        MainCamera.transform.position -= MainCamera.transform.forward * change; // this is here because I will need the MainCamera to be in the new position right away (not waiting for Update)
        transform.position -= MainCamera.transform.forward * change;
        dummyDistance += change;

        Vector3 mouseWorldPosAfter = MainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, MainCamera.pixelHeight - mousePos.y, dummyDistance));
        Vector3 offset = mouseWorldPosAfter - mouseWorldPosBefore;

        transform.position -= offset;

        // --------------------------------------------
        // FOCUS/FREECAM switch transition - interpolating version
        // --------------------------------------------
        var movementThisFrame = Smoothing * _moveBy;
        transform.position += movementThisFrame;
        _moveBy -= movementThisFrame;

        // --------------------------------------------
        // Applying all the changes to an actual camera
        // --------------------------------------------
        MainCamera.transform.position = transform.position;
        MainCamera.transform.rotation = transform.rotation;
        
    }

    /*
     * OnGUI is called for each event, so OnGUI is potentially called multiple times per frame
     */
    private void OnGUI()
    {

#if UNITY_EDITOR
        if (Event.current.type == EventType.Layout || Event.current.type == EventType.Repaint)
        {
            EditorUtility.SetDirty(this); // this is important, if omitted, "Mouse down" will not be display
        }
#endif
        //if (ShouldBlockInteraction)
        //{
        //    Debug.Log("BLOCKING INTERACTION");
        //    return;
        //}
        //Debug.Log("NOT BLOCKING INTERACTION");

        //if (ControlsDisabled) return;

        if (ShouldBlockInteraction)
        {
            return;
        }

        if (Event.current.type == EventType.MouseUp)            // mouse button RELEASED...
        {
            if (Event.current.button == 0)                          // ... LEFT mouse button
            {
                _leftDragging = false;
            } 
        }
        else if (Event.current.type == EventType.MouseDrag)     // mouse DRAGGED
        {
            if (Event.current.button == 0)                          // ... LEFT mouse button
            {
                //DoOrbitingAroundCursor(Event.current.delta.y * OrbitingSpeed, Event.current.delta.x * OrbitingSpeed);
                DoOrbitingAroundCursor(Event.current.delta.y, Event.current.delta.x);
            }
            else if (Event.current.button == 1)                     // ... RIGHT mouse button
            {
                // TODO: DoZoomToCenter()
                DoFineZoom(Event.current.delta.y/*, Event.current.mousePosition*/);
            }
            else if (Event.current.button == 2)                     // ... MIDDLE mouse button
            {
                // TODO: DoPanning()
                DoPanning(Event.current.delta.x, Event.current.delta.y);
            }
        }
        else if (Event.current.type == EventType.ScrollWheel)   // mouse wheel SCROLLED
        {
            //DoScrollingToCursor(Event.current.delta.y * ScrollingSpeed, Event.current.mousePosition);
            DoScrollingToCursor(Event.current.delta.y, Event.current.mousePosition);
        }
        
    }


    // This is a little bit redundant piece of code but it's been added here for for Event replaying
    public void ProcessEvent(Event e, float delay = 0.0f)
    {
        //NewSelectionManager.Instance.MousePosition = e.mousePosition;

        if (e.type == EventType.MouseUp)            // mouse button RELEASED...
        {
            if (e.button == 0)                          // ... LEFT mouse button
            {
                _leftDragging = false;
            }
        }
        else if (e.type == EventType.MouseDrag)     // mouse DRAGGED
        {
            if (e.button == 0)                          // ... LEFT mouse button
            {
                //DoOrbitingAroundCursor(e.delta.y * OrbitingSpeed, e.delta.x * OrbitingSpeed, delay);
                DoOrbitingAroundCursor(e.delta.y, e.delta.x, delay);
            }
            else if (e.button == 1)                     // ... RIGHT mouse button
            {
                // TODO: DoZoomToCenter()
            }
            else if (e.button == 2)                     // ... MIDDLE mouse button
            {
                DoPanning(e.delta.x, e.delta.y, delay);
            }
        }
        else if (e.type == EventType.ScrollWheel)   // mouse wheel SCROLLED
        {
            //DoScrollingToCursor(e.delta.y * ScrollingSpeed, e.mousePosition, delay);
            DoScrollingToCursor(e.delta.y, e.mousePosition, delay);
        }
    }


    /// <summary>
    /// Perform panning operation on camera
    /// </summary>
    void DoPanning(float deltaX, float deltaY, float delay = 0.0f)
    {
        _panStartPosition = transform.position;
        _lastFramePosition = _panStartPosition;

        _panEndPosition = transform.position;
        _panEndPosition += transform.up * deltaY * PanningSpeed;
        _panEndPosition -= transform.right * deltaX * PanningSpeed;

        _panCurrentLerpTime = 0.0f + delay;
    }
    
    /// <summary>
    /// Perform orbiting around protein that the cursor has started drag on
    /// </summary>
    /// <param name="deltaX">amount of degrees to rotate around X axis (vertical movement - up/down)</param>
    /// <param name="deltaY">amount of degrees to rotate around Y axis (horizontal movement - left/right)</param>
    void DoOrbitingAroundCursor(float deltaX, float deltaY, float delay = 0.0f)
    {

        if (!_leftDragging)
        {
            if (NewSelectionManager.Instance.HoveredInstanceId != -1)
            {
                _leftDragging = true;
                _currentRotationPivot = NewSelectionManager.Instance.HoveredInstancePosition * GlobalProperties.Get.Scale;
                //Debug.Log("Rotation Pivot has been set!");
            } 
            else
            {
                _leftDragging = true;
                _currentRotationPivot = new Vector3(0,0,0);
            }
        }

        if (_leftDragging)
        {
            _orbitCurrentLerpTime = 0.0f + delay;
            _lastFrameAngleX = 0f;
            _lastFrameAngleY = 0f;
            _orbitEndAngleX = deltaX * OrbitingSpeed;
            _orbitEndAngleY = deltaY * OrbitingSpeed;

            //Debug.Log("ORBITING around " + _currentRotationPivot);
        }

    }
    
    /// <summary>
    /// Perform scrolling with camera staying on cursor position
    /// </summary>
    /// <param name="deltaScroll">amount of scrolling</param>
    /// <param name="mousePos">current mouse position</param>
    void DoScrollingToCursor(float deltaScroll, Vector2 mousePos, float delay = 0.0f)
    {
        _currentMousePosition = mousePos;

        _zoomCurrentLerpTime = 0.0f + delay;
        _zoomEndAmount = deltaScroll * ScrollingSpeed;
        _lastFrameZoomAmount = 0.0f;
    }

    void DoFineZoom(float deltaScroll/*, Vector2 mousePos*/)
    {
        _currentMousePosition = new Vector2(Screen.width / 2, Screen.height / 2);

        //_zoomCurrentLerpTime = 0.0f + delay;
        _zoomCurrentLerpTime = 0.0f;
        _zoomEndAmount = deltaScroll * ScrollingSpeed;
        _lastFrameZoomAmount = 0.0f;
    }

    /*
     * I am not sure if this is the place where this logic should be implemented but for prototyping purposes I will just leave it here for now
     */
    private void SetProteinInfoVisibility(GameObject parentRect, bool visible)
    {
        GameObject heading = null;
        GameObject body = null;
        foreach (Transform child in parentRect.transform) // this is a very stupid way to get children of a parent GameObject...
        {
            if (child.name == "Heading") heading = child.gameObject;
            if (child.name == "Body") body = child.gameObject;
        }

        if (visible)
        {
            var bg = parentRect.GetComponent<RawImage>();
            bg.color = new Color(bg.color.r, bg.color.g, bg.color.b, 0.235f);
            var elem = heading.GetComponent<Text>();
            elem.color = new Color(elem.color.r, elem.color.g, elem.color.b, 1.0f);
            elem = body.GetComponent<Text>();
            elem.color = new Color(elem.color.r, elem.color.g, elem.color.b, 1.0f);
        } 
        else
        {
            var bg = parentRect.GetComponent<RawImage>();
            bg.color = new Color(bg.color.r, bg.color.g, bg.color.b, 0.0f);
            var elem = heading.GetComponent<Text>();
            elem.color = new Color(elem.color.r, elem.color.g, elem.color.b, 0.0f);
            elem = body.GetComponent<Text>();
            elem.color = new Color(elem.color.r, elem.color.g, elem.color.b, 0.0f);
        }
    }
    
    public void ZoomInOnSelected()
    {
        Vector4 pos = CPUBuffers.Get.ProteinInstancePositions[NewSelectionManager.Instance.SelectedProteinInstanceId];
        Vector3 proteinPosition = new Vector3(pos.x, pos.y, pos.z) * GlobalProperties.Get.Scale;
        var cameraBack = -MainCamera.transform.forward; // camera backwards vector is also vector from protein to camera
        var newCameraPos = proteinPosition + cameraBack * DefaultProteinFocusDistance;
        _moveBy = newCameraPos - transform.position;
        
    }

    //public void ShowInfoText()
    //{
    //    //SetProteinInfoVisibility(text, true);
    //}

    /// <summary>
    /// This method will be called from outside whenever it should switch the CameraState (for example from Free to Focus)
    /// </summary>
    /// <param name="newState">that's quite obvious, innit?</param>
    //public void SwitchToState(CameraState newState)
    //{
    //    //var text = GameObject.Find("SelectedObjectInfoText");
    //    Vector3 cameraBack;
    //    Vector3 newCameraPos;

    //    switch (newState)
    //    {
    //        case CameraState.Focus: // when going to focus state I want to set target position, zoom a little bit closer to the selected object 
    //                                // and maybe do some image effects (DoF, color turned down a little bit...)
    //            //if (_cameraState == CameraState.Free) // I think I have to differentiate between cases when I'm just jumping between proteins and when I'm going from Free cam
    //            //{
    //                Vector4 pos = CPUBuffers.Get.ProteinInstancePositions[NewSelectionManager.Instance.SelectedProteinInstanceId];
    //                Vector3 proteinPosition = new Vector3(pos.x, pos.y, pos.z) * GlobalProperties.Get.Scale;
    //                cameraBack = -MainCamera.transform.forward; // camera backwards vector is also vector from protein to camera
    //                newCameraPos = proteinPosition + cameraBack * DefaultProteinFocusDistance;
    //                _moveBy = newCameraPos - transform.position;

    //                _cameraState = CameraState.Focus;
    //            //}
    //            //else if (_cameraState == CameraState.Focus)
    //            //{
    //            //    Vector4 pos = CPUBuffers.Get.ProteinInstancePositions[NewSelectionManager.Instance.SelectedInstanceId];
    //            //    Vector3 proteinPosition = new Vector3(pos.x, pos.y, pos.z) * GlobalProperties.Get.Scale;
    //            //    cameraBack = -MainCamera.transform.forward; // camera backwards vector is also vector from protein to camera
    //            //    newCameraPos = proteinPosition + cameraBack * DefaultProteinFocusDistance;
    //            //    _moveBy = newCameraPos - transform.position;
                    
    //            //}
    //            //SetProteinInfoVisibility(text, true);
    //            break;
    //        case CameraState.Free:
    //            if (_cameraState == CameraState.Free) return;
    //            // when entering Free camera state I want to zoom out so that I can see whole data set
    //            cameraBack = -MainCamera.transform.forward;
    //            newCameraPos = Vector3.zero + cameraBack * DefaultDistance * 50;
    //            _moveBy = newCameraPos - transform.position;
    //            _cameraState = CameraState.Free;
    //            //SetProteinInfoVisibility(text, false);
    //            break;
    //    }
    //}

    public void ResetCamera()
    {
        transform.position = CameraDefaultPosition;
        transform.rotation = CameraDefaultRotation;
    }


    float EaseOutSin(float t)
    {
        return Mathf.Sin(t * Mathf.PI * 0.5f);
    }

    /// <summary>
    /// This is the best smoothing function
    /// </summary>
    float EaseOutExp(float t)
    {
        return -Mathf.Pow(2, -10 * t) + 1;
        //return t; // debug
    }
    
    float SmootherStep(float t)
    {
        return t*t*t * (t * (6f * t - 15f) + 10f);
    }

}
