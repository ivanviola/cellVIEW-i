using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PanelsUIController : MonoBehaviour {

    // public settings
    public bool InitialModeIsExpertMode = false;

    // references to ui objects
    //public GameObject RecorderPanel;
    public GameObject CutObjectsPanel;
    public GameObject HierarchyPanel;

    public GameObject StartScreen;

    public GameObject InfoText;

    public GameObject Tutorial;
    public int        TutorialProgress = -1;
    public bool       tutAlreadyReplayed = false;

    // panels state
    private bool panelsInExpertMode = false;

    // cut object settings elements

    private Transform cutObjectsList;
    private Transform addDeleteCutObject;

    // cut objects settings
    private Transform cutObjectsSettingsGO;

    private Transform decay;
    private Transform distance;
    private Transform gamma;
    private Transform invertCut;
    private Transform objectAlpha;
    private Transform cutOpacity;
    private Transform cutAperture;
    private Transform weigthThreshold;
    private Transform quantityThreshold;
    private Transform densityThreshold;
    private Transform plotPanel;

    private TreeViewController treeViewController;
    private MainCameraController cameraController;

    private GameObject tutorialOrbiting;
    private GameObject tutorialZooming;
    private GameObject tutorialPanning;
    private GameObject tutorialSelecting;
    private GameObject tutorialFinish;

    private float _keyDownStartTime;

    public void OnEnable()
    {
        cutObjectsList = CutObjectsPanel.transform.Find("Cut Objects List Section");
        addDeleteCutObject = CutObjectsPanel.transform.Find("Add Delete Cut Object Section");

        treeViewController = GameObject.Find("Scroll View/Viewport/Content").GetComponent<TreeViewController>();
        cameraController = GameObject.Find("Main Camera Controller").GetComponent<MainCameraController>();

        cutObjectsSettingsGO = CutObjectsPanel.transform.Find("Cut Object Settings Section/Fuzzy Panel");
        decay = cutObjectsSettingsGO.transform.Find("Decay");
        distance = cutObjectsSettingsGO.transform.Find("Distance");
        gamma = cutObjectsSettingsGO.transform.Find("Gamma");
        invertCut = cutObjectsSettingsGO.transform.Find("Invert Cut");
        objectAlpha = cutObjectsSettingsGO.transform.Find("Object Alpha");
        cutOpacity = cutObjectsSettingsGO.transform.Find("Cut Opacity");
        cutAperture = cutObjectsSettingsGO.transform.Find("Cut Aperture");
        weigthThreshold = cutObjectsSettingsGO.transform.Find("Weight Threshold");
        quantityThreshold = cutObjectsSettingsGO.transform.Find("Quantity Threshold");
        densityThreshold = cutObjectsSettingsGO.transform.Find("Density Threshold");
        plotPanel = cutObjectsSettingsGO.transform.Find("Plot Panel");

        tutorialOrbiting = GameObject.Find("Tutorial/Orbiting");
        tutorialZooming = GameObject.Find("Tutorial/Zooming");
        tutorialPanning = GameObject.Find("Tutorial/Panning");
        tutorialSelecting = GameObject.Find("Tutorial/Selecting");
        tutorialFinish = GameObject.Find("Tutorial/Finish");

        var path = Application.dataPath;
        path += "/../Data/RecordedFlythroughs/tutorial.json"; // I think this will not work when I run this from build
        InteractionRecorder.Get.LoadFlythroughs(path);

    }

    public void Start()
    {
        InitInMode(InitialModeIsExpertMode);
        //HideCutObjectsPanel();

        StartScreen.SetActive(true);
        cameraController.ShouldBlockInteraction = true;
        NewSelectionManager.Instance.ClickingDisabled = true;
        var go = GameObject.Find("Cut Object Panel/Selected Cut Object Type Section/Combobox/Button");
        var butt = go.GetComponent<Button>();
        butt.interactable = false;

        tutorialOrbiting.SetActive(false);
        tutorialZooming.SetActive(false);
        tutorialPanning.SetActive(false);
        tutorialSelecting.SetActive(false);
        tutorialFinish.SetActive(false);
    }

    public void Update()
    {
        //Debug.Log("TutorialProgress = " + TutorialProgress);
        //if (TutorialProgress >= 0 && TutorialProgress <= 3)
        //{
        //    if (TutorialProgress == 3) // selecting
        //    {
        //        tutAlreadyReplayed = true;
        //    }
        //    if (InteractionRecorder.Get.currentState == RecorderState.Stopped)
        //    {
        //        InteractionRecorder.Get.ReplayFlythrough(TutorialProgress);
        //        tutAlreadyReplayed = true;
        //    }
        //}
    }

    void OnGUI()
    {
        if (AnyKeyPushedTest())
        {
            TutorialProgress += 1;
            StartScreen.SetActive(false);
            ShowTutorial(TutorialProgress);
        }
        //if (Event.current.type == EventType.KeyDown)
        //{

        //    if (TutorialProgress >= 0) // already replaying tutorial
        //    {
        //        if (tutAlreadyReplayed)
        //        {
        //            TutorialProgress += 1;
        //            tutAlreadyReplayed = false;

        //            StartScreen.SetActive(false);
        //            ShowTutorial(TutorialProgress);
        //        }
        //    } else // this means its from welcome screen to first tut
        //    {
        //        TutorialProgress += 1;
                
        //        ShowTutorial(0);
        //        StartScreen.SetActive(false);
        //    }

        //    if (InteractionRecorder.Get.currentState == RecorderState.Replaying)
        //        InteractionRecorder.Get.Stop();
            
        //    if (TutorialProgress >= 3)
        //    {
        //        //StartScreen.SetActive(false);
        //        tutorialSelecting.SetActive(false);
        //        cameraController.ShouldBlockInteraction = false;
        //        NewSelectionManager.Instance.ClickingDisabled = false;
        //    }
        //}
    }

    void ShowTutorial(int whichTutorial)
    {
        if (whichTutorial == 0)
        {
            tutorialOrbiting.SetActive(true);
            tutorialZooming.SetActive(false);
            tutorialPanning.SetActive(false);
            tutorialSelecting.SetActive(false);
            tutorialFinish.SetActive(false);
        }
        else if (whichTutorial == 1)
        {
            tutorialOrbiting.SetActive(false);
            tutorialZooming.SetActive(true);
            tutorialPanning.SetActive(false);
            tutorialSelecting.SetActive(false);
            tutorialFinish.SetActive(false);
        }
        else if (whichTutorial == 2)
        {
            tutorialOrbiting.SetActive(false);
            tutorialZooming.SetActive(false);
            tutorialPanning.SetActive(true);
            tutorialSelecting.SetActive(false);
            tutorialFinish.SetActive(false);
        }
        else if (whichTutorial == 3)
        {
            tutorialOrbiting.SetActive(false);
            tutorialZooming.SetActive(false);
            tutorialPanning.SetActive(false);
            tutorialSelecting.SetActive(true);
            tutorialFinish.SetActive(false);
        }
        else if (whichTutorial == 4)
        {
            tutorialOrbiting.SetActive(false);
            tutorialZooming.SetActive(false);
            tutorialPanning.SetActive(false);
            tutorialSelecting.SetActive(false);
            tutorialFinish.SetActive(true);
        } else if (whichTutorial > 4)
        {
            //StartScreen.SetActive(false);
            tutorialFinish.SetActive(false);
            cameraController.ShouldBlockInteraction = false;
            NewSelectionManager.Instance.ClickingDisabled = false;
            Tutorial.SetActive(false);

            var go = GameObject.Find("Cut Object Panel/Selected Cut Object Type Section/Combobox/Button");
            var butt = go.GetComponent<Button>();
            butt.interactable = true;
        }
    }

    public void InitInMode(bool initialModeIsExpert)
    {
        // hierarchy panel
        var treeViewControllerGO = HierarchyPanel.transform.Find("Viewport/Content");
        var treeViewController = treeViewControllerGO.GetComponent<TreeViewController>();
        treeViewController.SwitchToMode(initialModeIsExpert);

        // cut object panel
        if (initialModeIsExpert)
        {
            cutObjectsList.gameObject.SetActive(true);
            addDeleteCutObject.gameObject.SetActive(true);

            decay.gameObject.SetActive(true);
            distance.gameObject.SetActive(true);
            gamma.gameObject.SetActive(true);
            invertCut.gameObject.SetActive(true);
            objectAlpha.gameObject.SetActive(true);
            cutOpacity.gameObject.SetActive(true);
            cutAperture.gameObject.SetActive(true);
            weigthThreshold.gameObject.SetActive(true);
            quantityThreshold.gameObject.SetActive(true);
            densityThreshold.gameObject.SetActive(true);
            plotPanel.gameObject.SetActive(true);
        }
        else
        {
            cutObjectsList.gameObject.SetActive(false);
            addDeleteCutObject.gameObject.SetActive(false);

            decay.gameObject.SetActive(false);
            distance.gameObject.SetActive(false);
            gamma.gameObject.SetActive(false);
            //invertCut.gameObject.SetActive(true);
            //objectAlpha.gameObject.SetActive(true);
            cutOpacity.gameObject.SetActive(false);
            cutAperture.gameObject.SetActive(false);
            weigthThreshold.gameObject.SetActive(false);
            quantityThreshold.gameObject.SetActive(false);
            densityThreshold.gameObject.SetActive(false);
            plotPanel.gameObject.SetActive(false);
        }
    }

    public void SwitchMode()
    {
        panelsInExpertMode = !panelsInExpertMode;

        var newText = panelsInExpertMode ? "NORMAL MODE" : "EXPERT MODE";
        var button = GameObject.Find("Expert Mode Button").GetComponent<Button>();
        button.GetComponentInChildren<Text>().text = newText;

        // hierarchy panel
        var treeViewControllerGO = HierarchyPanel.transform.Find("Viewport/Content");
        var treeViewController = treeViewControllerGO.GetComponent<TreeViewController>();
        //treeViewController.SwitchMode();
        treeViewController.SwitchToMode(panelsInExpertMode);

        // cut object panel
        if (panelsInExpertMode)
        {
            cutObjectsList.gameObject.SetActive(true);
            addDeleteCutObject.gameObject.SetActive(true);

            decay.gameObject.SetActive(true);
            distance.gameObject.SetActive(true);
            gamma.gameObject.SetActive(true);
            invertCut.gameObject.SetActive(true);
            objectAlpha.gameObject.SetActive(true);
            cutOpacity.gameObject.SetActive(true);
            cutAperture.gameObject.SetActive(true);
            weigthThreshold.gameObject.SetActive(true);
            quantityThreshold.gameObject.SetActive(true);
            densityThreshold.gameObject.SetActive(true);
            plotPanel.gameObject.SetActive(true);
        } 
        else
        {
            cutObjectsList.gameObject.SetActive(false);
            addDeleteCutObject.gameObject.SetActive(false);

            decay.gameObject.SetActive(false);
            distance.gameObject.SetActive(false);
            gamma.gameObject.SetActive(false);
            //invertCut.gameObject.SetActive(true);
            //objectAlpha.gameObject.SetActive(true);
            cutOpacity.gameObject.SetActive(false);
            cutAperture.gameObject.SetActive(false);
            weigthThreshold.gameObject.SetActive(false);
            quantityThreshold.gameObject.SetActive(false);
            densityThreshold.gameObject.SetActive(false);
            plotPanel.gameObject.SetActive(false);
        }

    }

    bool AnyKeyPushedTest()
    {
        var keyPushed = false;

        if (Event.current.type == EventType.KeyDown)
        {
            _keyDownStartTime = Time.realtimeSinceStartup;
        }
        
        if (Event.current.type == EventType.KeyUp)
        {
            var delta = Time.realtimeSinceStartup - _keyDownStartTime;
            if (delta < 0.5f)
            {
                keyPushed = true;
            }
        }

        return keyPushed;
    }

    /* ***** */

    //public void SwitchRecorderPanelVisibility()
    //{
    //    if (RecorderPanel.activeSelf)
    //    {
    //        HideRecorderPanel();
    //    }
    //    else
    //    {
    //        ShowRecorderPanel();
    //    }
    //}

    public void SwitchCutObjectsPanelVisibility()
    {
        if (CutObjectsPanel.activeSelf)
        {
            HideCutObjectsPanel();
        }
        else
        {
            ShowCutObjectsPanel();
        }
    }

    public void SwitchHierarchyPanelVisibility()
    {
        if (HierarchyPanel.activeSelf)
        {
            HideHierarchyPanel();
        }
        else
        {
            ShowHierarchyPanel();
        }
    }

    public void ExitCellVIEW()
    {
        Application.Quit();
    }

    /* **** */

    //private void ShowRecorderPanel()
    //{
    //    RecorderPanel.SetActive(true);
    //}

    //private void HideRecorderPanel()
    //{
    //    RecorderPanel.SetActive(false);
    //}

    private void ShowCutObjectsPanel()
    {
        CutObjectsPanel.SetActive(true);
    }

    private void HideCutObjectsPanel()
    {
        CutObjectsPanel.SetActive(false);
    }

    private void ShowHierarchyPanel()
    {
        HierarchyPanel.SetActive(true);
    }

    private void HideHierarchyPanel()
    {
        HierarchyPanel.SetActive(false);
    }
}
